using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;
using CarController.Model.Regulators;
using CarController.Model.Communicators;

namespace CarController.Model.Car
{
    public class RealCar : ICar
    {
        public event EventHandler evAlertBrake;
        public event TargetSpeedChangedEventHandler evTargetSpeedChanged;
        public event TargetSteeringWheelAngleChangedEventHandler evTargetSteeringWheelAngleChanged;

        public ICarCommunicator CarComunicator { get; private set; }
        public DefaultCarController Controller { get; private set; }
        public ISpeedRegulator SpeedRegulator { get; private set; }
        public ISteeringWheelAngleRegulator SteeringWheelAngleRegulator { get; private set; }
        public IBrakeRegulator BrakeRegulator { get; private set; }
        public DeviceManager deviceManager { get; private set; }

        public bool IsAlertBrakeActive { get; private set; }
        public CarInformations CarInfo { get; private set; }

        public RealCar(DefaultCarController parent)
        {
            System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.Highest;

            Controller = parent;
            CarInfo = new CarInformations();

            IsAlertBrakeActive = false;

            deviceManager = DeviceManager.GlobalDeviceManager;

            CarComunicator = new RealCarCommunicator(this);

            SpeedRegulator = new PIDSpeedRegulator(this);
            SteeringWheelAngleRegulator = new PIDSteeringWheelAngleRegulator(this);
            BrakeRegulator = new SimpleBrakeRegulator(this);

            CarComunicator.InitRegulatorsEventsHandling();  //TODO: REFACTOR THIS SHIT!!! //for now this is needed, because reagulators does not exists when communicator constructor is invoked

            InternalEventHandlingInitialization();
        }

        private void InternalEventHandlingInitialization()
        {
            evAlertBrake += ExampleFakeCar_evAlertBrake;
            evTargetSpeedChanged += ExampleFakeCar_evTargetSpeedChanged;
            evTargetSteeringWheelAngleChanged += ExampleFakeCar_evTargetSteeringWheelAngleChanged;
            deviceManager.evDeviceManagerOverallStateHasChanged += deviceManager_evDeviceStateHasChanged;
        }

        void deviceManager_evDeviceStateHasChanged(object sender, DeviceStateHasChangedEventArgs args)
        {
            switch (args.GetDeviceState())
            {
                case DeviceOverallState.Error:
                    OverrideTargetBrakeSetting(100.0);
                    break;

                case DeviceOverallState.OK:
                    EndTargetBrakeSteeringOverriding();
                    break;

                case DeviceOverallState.Warrning:
                    //do nothing
                    break;

                default:
                    throw new ApplicationException("unhandled device state");
            }
        }

        private void ExampleFakeCar_evTargetSteeringWheelAngleChanged(object sender, TargetSteeringWheelAngleChangedEventArgs args)
        {
            Logger.Log(this, String.Format("target wheel angle changed to: {0}", args.GetTargetWheelAngle()));
        }
        private void ExampleFakeCar_evTargetSpeedChanged(object sender, TargetSpeedChangedEventArgs args)
        {
            Logger.Log(this, String.Format("target speed changed to: {0}", args.GetTargetSpeed()));
        }
        private void ExampleFakeCar_evAlertBrake(object sender, EventArgs e)
        {
            Logger.Log(this, "ALERT BRAKE!");
        }

        public void ActivateAlertBrake()
        {
            EventHandler temp = evAlertBrake;
            if (temp != null)
            {
                temp(this, EventArgs.Empty);
            }
        }
        public void SetTargetSpeed(double speed)
        {
            CarInfo.TargetSpeed = speed;

            TargetSpeedChangedEventHandler temp = evTargetSpeedChanged;
            if (temp != null)
            {
                temp(this, new TargetSpeedChangedEventArgs(speed));
            }
        }
        public void SetTargetWheelAngle(double angle)
        {
            CarInfo.TargetWheelAngle = angle;

            TargetSteeringWheelAngleChangedEventHandler temp = evTargetSteeringWheelAngleChanged;
            if (temp != null)
            {
                temp(this, new TargetSteeringWheelAngleChangedEventArgs(angle));
            }
        }

        public void OverrideTargetBrakeSetting(double setting)
        {
            BrakeRegulator.OverrideTargetBrakeSetting(setting);
        }

        public void EndTargetBrakeSteeringOverriding()
        {
            BrakeRegulator.EndTargetBrakeSteeringOverriding();
        }

    }
}
