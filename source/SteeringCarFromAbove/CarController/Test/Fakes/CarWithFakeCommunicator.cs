using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CarController.Model.Regulators;
using Helpers;

namespace CarController
{
    public class CarWithFakeCommunicator : ICar
    {
        public event EventHandler evAlertBrake;
        public event TargetSpeedChangedEventHandler evTargetSpeedChanged;
        public event TargetSteeringWheelAngleChangedEventHandler evTargetSteeringWheelAngleChanged;

        public ISteeringWheelAngleRegulator SteeringWheelAngleRegulator { get; private set; }
        public ISpeedRegulator SpeedRegulator { get; private set; }
        public IBrakeRegulator BrakeRegulator { get; private set; }

        public DefaultCarController Controller { get; private set; }
        public ICarCommunicator CarComunicator { get; private set; }
        public CarInformations CarInfo { get; set; }

        public CarWithFakeCommunicator(DefaultCarController parent)
        {
            Controller = parent;
            CarInfo = new CarInformations();

            CarComunicator = new FakeCarCommunicator(this);

            SteeringWheelAngleRegulator = new PIDSteeringWheelAngleRegulator(this);
            SpeedRegulator = new PIDSpeedRegulator(this);
            BrakeRegulator = new PIDBrakeRegulator(this);

            CarComunicator.InitRegulatorsEventsHandling();
        }

        public void SetTargetWheelAngle(double targetAngle)
        {
            CarInfo.TargetWheelAngle = targetAngle;

            TargetSteeringWheelAngleChangedEventHandler temp = evTargetSteeringWheelAngleChanged;
            if (temp != null)
            {
                temp(this, new TargetSteeringWheelAngleChangedEventArgs(targetAngle));
            }
        }

        public void SetTargetSpeed(double targetSpeed)
        {
            CarInfo.TargetSpeed = targetSpeed;

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
                temp(this, EventArgs.Empty);
            }

            Logger.Log(this, "alert brake activated!", 2);
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
