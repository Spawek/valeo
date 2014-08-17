using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using CarController.Model.Regulators;

namespace CarController
{
    public class PIDSpeedRegulator : ISpeedRegulator
    {
        public event NewSpeedSettingCalculatedEventHandler evNewSpeedSettingCalculated;

        public ICar Car { get; private set; }
        public ICarCommunicator CarComunicator{ get; private set; }

        /// <summary>
        /// setting this value will also send event "evNewSpeedSettingCalculated"
        /// </summary>
        public double SpeedSteering
        {
            private set
            {
                if (alertBrakeActive)
                {
                    __speedSteering__ = ALERT_BRAKE_SPEED_STEERING;
                }
                else
                {
                    if (Mode == RideMode.forward)
                    {
                        __speedSteering__ = value;
                    }
                    else if (Mode == RideMode.backward)
                    {
                        __speedSteering__ = value * -1;
                    }
                    else if (Mode == RideMode.standStill)
                    {
                        __speedSteering__ = Math.Abs(value) * -1; //this value has to be < 0 - only braking is allowed, its not dependent on direction of current move;
                    }
                }

                NewSpeedSettingCalculatedEventHandler newSpeedCalculatedEvent = evNewSpeedSettingCalculated;
                if (newSpeedCalculatedEvent != null)
                {
                    newSpeedCalculatedEvent(this, new NewSpeedSettingCalculatedEventArgs(__speedSteering__));
                }
            }
            get { return __speedSteering__; }
        }
        private double __speedSteering__;

        //copies of speed informations
        private double targetSpeedLocalCopy = 0.0;
        private double currentSpeedLocalCopy = 0.0;
        private double lastSteeringSeetingSend = 0.0;

        //alert brake fields
        private const double ALERT_BRAKE_SPEED_STEERING = 0.0;
        private bool alertBrakeActive = false;

        //consts
        private const double MAX_STAND_STILL_SPEED_IN_MPS = 1; //MPS = meter per s = m/s = 3.6km/h
        private const double MIN_STAND_STILL_SPEED_IN_MPS = -1;

        private enum RideMode
        {
            forward,
            standStill,
            backward
        }

        /// <summary>
        /// changing mode also changes gear
        /// </summary>
        private RideMode Mode
        {
            get
            {
                return __mode__;
            }
            set
            {
                if (__mode__ != value)
                {
                    switch (value)
                    {
                        case RideMode.forward:
                            Car.CarComunicator.SetGear(Gear.drive);
                            break;

                        case RideMode.standStill:
                            Car.CarComunicator.SetGear(Gear.neutral);
                            break;

                        case RideMode.backward:
                            Car.CarComunicator.SetGear(Gear.reverse);
                            break;
                    }
                    __mode__ = value;
                    Logger.Log(this, String.Format("Mode has been changed to: {0}, gear has been changed", value.ToString()), 1);
                }
            }
        }
        private RideMode __mode__;

        private class Settings : PIDSettings
        {
            public Settings()
            {
                //P part settings
                P_FACTOR_MULTIPLER = 1.5;

                //I part settings
                I_FACTOR_MULTIPLER = 1; //hypys radzi, żeby to wyłączyć bo może być niestabilny (a tego baardzo nie chcemy)
                I_FACTOR_SUM_MAX_VALUE = 100;
                I_FACTOR_SUM_MIN_VALUE = -100;
                I_FACTOR_SUM_SUPPRESSION_PER_SEC = 0.95; // = 0.88; //1.0 = suppresing disabled

                //D part settings
                D_FACTOR_MULTIPLER = 3;
                D_FACTOR_SUPPRESSION_PER_SEC = 0.5;
                D_FACTOR_SUM_MIN_VALUE = -100;
                D_FACTOR_SUM_MAX_VALUE = 100;

                //steering limits
                MAX_FACTOR_CONST = 100; // = 100.0; //MAX throtle
                MIN_FACTOR_CONST = -100; // = -100.0; //MAX brake
            }
        }

        private PIDRegulator regulator;

        public PIDSpeedRegulator(ICar parent)
        {
            Car = parent;
            CarComunicator = parent.CarComunicator;

            regulator = new PIDRegulator(new Settings(), "speed PID regulator");

            Car.evAlertBrake += new EventHandler(Car_evAlertBrake);
            Car.evTargetSpeedChanged += new TargetSpeedChangedEventHandler(Car_evTargetSpeedChanged);
            evNewSpeedSettingCalculated += new NewSpeedSettingCalculatedEventHandler(PIDSpeedRegulator_evNewSpeedSettingCalculated);
            CarComunicator.evSpeedInfoReceived += new SpeedInfoReceivedEventHander(CarComunicator_evSpeedInfoReceived);
        }

        //handling external events
        void CarComunicator_evSpeedInfoReceived(object sender, SpeedInfoReceivedEventArgs args)
        {
            currentSpeedLocalCopy = args.GetSpeedInfo();
            Logger.Log(this, String.Format("new current speed value acquired: {0}", args.GetSpeedInfo()));

            //this setter also sends event "evNewSpeedSettingCalculated"
            SpeedSteering = regulator.ProvideObjectCurrentValueToRegulator(currentSpeedLocalCopy);
        }

        /// <summary>
        /// depending on what is target speed it can have different meanings
        /// speed >> 0 ---> going forward
        /// speed ~= 0 ---> stand still mode
        /// speed << 0 ---> going backward
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Car_evTargetSpeedChanged(object sender, TargetSpeedChangedEventArgs args)
        {
            targetSpeedLocalCopy = args.GetTargetSpeed();
            Logger.Log(this, String.Format("target speed changed to: {0}", args.GetTargetSpeed()));

            if (targetSpeedLocalCopy <= MAX_STAND_STILL_SPEED_IN_MPS && targetSpeedLocalCopy >= MIN_STAND_STILL_SPEED_IN_MPS)
            {
                Mode = RideMode.standStill;
            }
            else if (targetSpeedLocalCopy > MAX_STAND_STILL_SPEED_IN_MPS)
            {
                Mode = RideMode.forward;
            }
            else //if targetSpeedLocalCopy < MIN_STAND_STILL_SPEED_IN_MPS
            {
                Mode = RideMode.backward;
            }

            //this setter also sends event "evNewSpeedSettingCalculated"
            SpeedSteering = regulator.SetTargetValue(targetSpeedLocalCopy);
        }

        void PIDSpeedRegulator_evNewSpeedSettingCalculated(object sender, NewSpeedSettingCalculatedEventArgs args)
        {
 	        Logger.Log(this, String.Format("new speed setting calculated: {0}", args.getSpeedSetting()));
        }

        void Car_evAlertBrake(object sender, EventArgs e)
        {
            alertBrakeActive = true;
            SpeedSteering = ALERT_BRAKE_SPEED_STEERING;
            Logger.Log(this, "ALERT BRAKE ACRIVATED!", 2);
        }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return regulator.GetRegulatorParameters();
        }
    }
}
