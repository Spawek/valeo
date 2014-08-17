using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;

namespace CarController.Model.Regulators
{
    class SimpleBrakeRegulator : IBrakeRegulator
    {
        public event NewBrakeSettingCalculatedEventHandler evNewBrakeSettingCalculated;

        private double targetValue;
        private double currentValue;

        private bool targetBrakeOverriden = false;
        private double targetBrakeOverridenValue = 0.0;

        /// <summary>
        /// setting this property will also send evNewBrakeSettingCalculated event with set value
        /// </summary>
        public double BrakeSteering
        {
            get{ return __brakeSteering__; }
            private set
            {
                __brakeSteering__ = value;

                NewBrakeSettingCalculatedEventHandler temp = evNewBrakeSettingCalculated;
                if (temp != null)
                {
                    temp(this, new NewBrakeSettingCalculatedEventArgs(__brakeSteering__));
                }
            }
        }
        private double __brakeSteering__;

        public ICar ICar { get; private set; }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return new Dictionary<string, double>();
        }

        private bool alertBrakeActive = false;
        private const double ALERT_BRAKE_BRAKE_SETTING = 100;

        private SimpleRegulator regulator;

        private class Settings : SimpleRegulatorSettings
        {
            public Settings()
            {
                HYSTERESIS_IN_PERCENTS = 15;
                STEERING_WHEN_OVER_HYSTERESIS = 80;
            }
        };

        public SimpleBrakeRegulator(ICar car)
        {
            ICar = car;
            regulator = new SimpleRegulator(new Settings(), "brake PID regulator");

            ICar.SpeedRegulator.evNewSpeedSettingCalculated += SpeedRegulator_evNewSpeedSettingCalculated;
            ICar.CarComunicator.evBrakePositionReceived += CarComunicator_evBrakePositionReceived;
            evNewBrakeSettingCalculated += PIDBrakeRegulator_evNewBrakeSettingCalculated;
            ICar.evAlertBrake += ICar_evAlertBrake;
        }

        void ICar_evAlertBrake(object sender, EventArgs e)
        {
            alertBrakeActive = true;
            Logger.Log(this, "ALERT BRAKE ACTIVATED!", 2);
        }

        void SpeedRegulator_evNewSpeedSettingCalculated(object sender, NewSpeedSettingCalculatedEventArgs args)
        {
            if (args.getSpeedSetting() > 0)
            {
                SetTarget(0);
            }
            else
            {
                SetTarget(-1 * args.getSpeedSetting());
            }
        }

        void PIDBrakeRegulator_evNewBrakeSettingCalculated(object sender, NewBrakeSettingCalculatedEventArgs args)
        {
            Logger.Log(this, String.Format("New brake steering has been calculated: {0}", args.GetBrakeSetting()));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">has to be in range[0, 100][%]</param>
        public void SetTarget(double target)
        {
            if (Limiter.LimitAndReturnTrueIfLimitted(ref target, 0, 100))
            {
                Logger.Log(this, "target brake is not in range [0, 100]", 1);
            }

            double calculatedSteering;
            if (alertBrakeActive)
            {
                calculatedSteering = regulator.SetTargetValue(ALERT_BRAKE_BRAKE_SETTING);
            }
            else if (targetBrakeOverriden)
            {
                calculatedSteering = regulator.SetTargetValue(targetBrakeOverridenValue);
            }
            else
            {
                calculatedSteering = regulator.SetTargetValue(target);
            }

            //this will also invoke evNewBrakeSettingCalculated event
            BrakeSteering = calculatedSteering;
        }

        void CarComunicator_evBrakePositionReceived(object sender, BrakePositionReceivedEventArgs args)
        {
            //this will also invoke evNewBrakeSettingCalculated event
            BrakeSteering = regulator.ProvideObjectCurrentValueToRegulator(args.GetPosition());
        }

        public void OverrideTargetBrakeSetting(double setting)
        {
            targetBrakeOverriden = true;
            targetBrakeOverridenValue = setting;
        }

        public void EndTargetBrakeSteeringOverriding()
        {
            targetBrakeOverriden = false;
        }
    }
}
