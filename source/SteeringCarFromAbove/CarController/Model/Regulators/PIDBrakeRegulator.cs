using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;

namespace CarController.Model.Regulators
{
    /// <summary>
    /// calculates new steering whenever new target is set or new brake possition is received
    /// </summary>
    public class PIDBrakeRegulator : IBrakeRegulator
    {
        public event NewBrakeSettingCalculatedEventHandler evNewBrakeSettingCalculated;

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
            return regulator.GetRegulatorParameters();
        }

        private bool alertBrakeActive = false;
        private const double ALERT_BRAKE_BRAKE_SETTING = 100;

        private class Settings : PIDSettings
        {
            public Settings() 
            { 
                //P part settings
                P_FACTOR_MULTIPLER = 2;

                //I part settings
                I_FACTOR_MULTIPLER = 0; //hypys radzi, żeby to wyłączyć bo może być niestabilny (a tego baardzo nie chcemy)
                I_FACTOR_SUM_MAX_VALUE = 100;
                I_FACTOR_SUM_MIN_VALUE = -100; 
                I_FACTOR_SUM_SUPPRESSION_PER_SEC = 0.96; // = 0.88; //1.0 = suppresing disabled

                //D part settings
                D_FACTOR_MULTIPLER = 0;
                D_FACTOR_SUPPRESSION_PER_SEC = 0;
                D_FACTOR_SUM_MIN_VALUE = 0;
                D_FACTOR_SUM_MAX_VALUE = 0;

                //steering limits
                MAX_FACTOR_CONST = 100; // = 100.0;
                MIN_FACTOR_CONST = -100; // = -100.0;
            }
        }

        private PIDRegulator regulator;

        public PIDBrakeRegulator(ICar car)
        {
            ICar = car;
            regulator = new PIDRegulator(new Settings(), "brake PID regulator");

            ICar.evTargetSpeedChanged += ICar_evTargetSpeedChanged;
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

        private bool stopModeOn = false;

        void ICar_evTargetSpeedChanged(object sender, TargetSpeedChangedEventArgs args)
        {
            double targetSpeed = args.GetTargetSpeed();

            if (targetSpeed > -0.1 && targetSpeed < 0.1)
            {
                stopModeOn = true;
            }
            else
            {
                stopModeOn = false;
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
            else if (stopModeOn)
            {
                calculatedSteering = regulator.SetTargetValue(100);
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
            throw new NotImplementedException();
        }

        public void EndTargetBrakeSteeringOverriding()
        {
            throw new NotImplementedException();
        }
    }
}
