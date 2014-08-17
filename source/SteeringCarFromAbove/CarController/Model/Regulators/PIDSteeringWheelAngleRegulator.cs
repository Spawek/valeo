using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CarController.Model.Regulators;
using Helpers;

namespace CarController
{
    public class PIDSteeringWheelAngleRegulator : ISteeringWheelAngleRegulator
    {
        public event NewSteeringWheelSettingCalculatedEventHandler evNewSteeringWheelSettingCalculated;

        public ICar Car { get; private set; }
        public ICarCommunicator CarComunicator
        {
            get
            {
                return Car.CarComunicator;
            }
        }
        public double WheelAngleSteering { get; private set; }

        private class Settings : PIDSettings
        {
            public Settings()
            {
                //P part settings
                P_FACTOR_MULTIPLER = 5;

                //I part settings
                I_FACTOR_MULTIPLER = 0; //hypys radzi, żeby to wyłączyć bo może być niestabilny (a tego baardzo nie chcemy)
                I_FACTOR_SUM_MAX_VALUE = 0;
                I_FACTOR_SUM_MIN_VALUE = 0;
                I_FACTOR_SUM_SUPPRESSION_PER_SEC = 1; // = 0.88; //1.0 = suppresing disabled

                //D part settings
                D_FACTOR_MULTIPLER = 4;
                D_FACTOR_SUPPRESSION_PER_SEC = 0.3;
                D_FACTOR_SUM_MIN_VALUE = -1000;
                D_FACTOR_SUM_MAX_VALUE = 1000;

                //steering limits
                MAX_FACTOR_CONST = 100; // = 100.0;
                MIN_FACTOR_CONST = -100; // = -100.0;
            }
        }

        private PIDRegulator regulator;

        public PIDSteeringWheelAngleRegulator(ICar car)
        {
            Car = car;
            regulator = new PIDRegulator(new Settings(), "steering wheel angle regulator");

            car.evTargetSteeringWheelAngleChanged += new TargetSteeringWheelAngleChangedEventHandler(car_evTargetSteeringWheelAngleChanged);
            car.CarComunicator.evSteeringWheelAngleInfoReceived += new SteeringWheelAngleInfoReceivedEventHandler(CarComunicator_evSteeringWheelAngleInfoReceived);
        }

        void CarComunicator_evSteeringWheelAngleInfoReceived(object sender, SteeringWheelAngleInfoReceivedEventArgs args)
        {
            double calculatedSteering = regulator.ProvideObjectCurrentValueToRegulator(args.GetAngle());

            NewSteeringWheelSettingCalculatedEventHandler newWheelSteeringCalculatedEvent = evNewSteeringWheelSettingCalculated;
            if (newWheelSteeringCalculatedEvent != null)
            {
                newWheelSteeringCalculatedEvent(this, new NewSteeringWheelSettingCalculateddEventArgs(calculatedSteering));
            }
        }

        void car_evTargetSteeringWheelAngleChanged(object sender, TargetSteeringWheelAngleChangedEventArgs args)
        {
            double calculatedSteering = regulator.SetTargetValue(args.GetTargetWheelAngle());

            NewSteeringWheelSettingCalculatedEventHandler newWheelSteeringCalculatedEvent = evNewSteeringWheelSettingCalculated;
            if (newWheelSteeringCalculatedEvent != null)
            {
                newWheelSteeringCalculatedEvent(this, new NewSteeringWheelSettingCalculateddEventArgs(calculatedSteering));
            }
        }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return regulator.GetRegulatorParameters();
        }
    }
}
