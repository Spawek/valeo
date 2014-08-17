using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public delegate void NewSteeringWheelSettingCalculatedEventHandler(object sender, NewSteeringWheelSettingCalculateddEventArgs args);
    public class NewSteeringWheelSettingCalculateddEventArgs : EventArgs
    {
        private double steeringWheelAngleSetting;
        public NewSteeringWheelSettingCalculateddEventArgs(double setting)
        {
            steeringWheelAngleSetting = setting;
        }

        public double getSteeringWheelAngleSetting()
        {
            return steeringWheelAngleSetting;
        }
    }

    public interface ISteeringWheelAngleRegulator
    {

        event NewSteeringWheelSettingCalculatedEventHandler evNewSteeringWheelSettingCalculated;

        ICar Car
        {
            get;
        }

        double WheelAngleSteering
        {
            get;
        }

        IDictionary<string, double> GetRegulatorParameters();
    }
}
