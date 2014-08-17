using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using System.Threading;

namespace CarController
{
    public class MFCSPeedRegulator : ISpeedRegulator
    {
        public event NewSpeedSettingCalculatedEventHandler evNewSpeedSettingCalculated;

        public ICar Car { get; private set; }

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

        //alert brake fields
        private const double ALERT_BRAKE_SPEED_STEERING = 0.0;
        private bool alertBrakeActive = false;

        public CarSimulator.CarModel CarModel { get; private set; }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            throw new NotImplementedException();
        }

        private const int MODEL_TIMER_INTERVAL_IN_MS = 20;
        private System.Timers.Timer ModelTimer = new System.Timers.Timer(MODEL_TIMER_INTERVAL_IN_MS);
        private Thread ModelThread;

        MFCSPeedRegulator(ICar car, CarSimulator.CarModel carModel)
        {
            Car = car;
            CarModel = carModel;

            RegisterModelForSteeringEvents();

            ModelThread = new Thread(ContinousCarSimulation);
            ModelThread.Start();
        }

        private void RegisterModelForSteeringEvents()
        {
            Car.BrakeRegulator.evNewBrakeSettingCalculated += BrakeRegulator_evNewBrakeSettingCalculated;
            this.evNewSpeedSettingCalculated += MFCSPeedRegulator_evNewSpeedSettingCalculated;
        }

        void MFCSPeedRegulator_evNewSpeedSettingCalculated(object sender, NewSpeedSettingCalculatedEventArgs args)
        {
 	        CarModel.ThrottleOppeningLevel = args.getSpeedSetting() / 100.0d;
        }

        void BrakeRegulator_evNewBrakeSettingCalculated(object sender, NewBrakeSettingCalculatedEventArgs args)
        {
 	        CarModel.BrakingLevel = args.GetBrakeSetting() / 100.0d;
        }

        void ContinousCarSimulation()
        {
            while (true)
            {
                try
                {
                    CarModel.CalculationsTick();
                    Thread.Sleep(MODEL_TIMER_INTERVAL_IN_MS);
                }
                catch (Exception e)
                {
                    Logger.Log(this, String.Format("MFC model exception catched: {0}", e.Message), 2);
                    Logger.Log(this, String.Format("MFC model exception stack: {0}", e.StackTrace), 1);
                }
            }
        }
    }
}
