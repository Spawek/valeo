using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;

namespace CarController
{
    public class CarWithFakeRegulators : ICar
    {
        public event EventHandler evAlertBrake;
        public event TargetSpeedChangedEventHandler evTargetSpeedChanged;
        public event TargetSteeringWheelAngleChangedEventHandler evTargetSteeringWheelAngleChanged;

        public ISteeringWheelAngleRegulator SteeringWheelAngleRegulator { get; private set; }
        public ISpeedRegulator SpeedRegulator { get; private set; }
        public IBrakeRegulator BrakeRegulator { get; private set; }

        public DefaultCarController Controller { get; private set; }
        public ICarCommunicator CarComunicator { get; private set; }
        public CarInformations CarInfo { get; private set; }

        public CarWithFakeRegulators(DefaultCarController parent)
        {
            Controller = parent;
            CarInfo = new CarInformations();

            SteeringWheelAngleRegulator = new FakeSteeringWheelRegulator();
            SpeedRegulator = new FakeSpeedRegulator();
            BrakeRegulator = new FakeBrakeRegulator();

            CarComunicator = new RealCarCommunicator(this);
        }

        public void SetTargetWheelAngle(double targetAngle)
        {
            TargetSteeringWheelAngleChangedEventHandler temp = evTargetSteeringWheelAngleChanged;
            if (temp != null)
            {
                temp(this, new TargetSteeringWheelAngleChangedEventArgs(targetAngle));
            }
        }

        public void SetTargetSpeed(double targetSpeed)
        {
            TargetSpeedChangedEventHandler temp = evTargetSpeedChanged;
            if (temp != null)
            {
                temp(this, new TargetSpeedChangedEventArgs(targetSpeed));
            }
        }

        public void ActivateAlertBrake()
        {
            EventHandler temp = evAlertBrake;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }


        public void OverrideTargetBrakeSetting(double setting)
        {
            throw new NotImplementedException();
        }

        public void EndTargetBrakeSteeringOverriding()
        {
            throw new NotImplementedException();
        }


        public DeviceManager deviceManager
        {
            get { throw new NotImplementedException(); }
        }
    }
}
