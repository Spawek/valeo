using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public delegate void NewBrakeSettingCalculatedEventHandler(object sender, NewBrakeSettingCalculatedEventArgs args);
    public class NewBrakeSettingCalculatedEventArgs : EventArgs
    {
        private double brakeSetting;
        
        public NewBrakeSettingCalculatedEventArgs(double setting)
        {
            brakeSetting = setting;
        }

        public double GetBrakeSetting()
        {
            return brakeSetting;
        }
    }

    public interface IBrakeRegulator
    {
        event NewBrakeSettingCalculatedEventHandler evNewBrakeSettingCalculated;
    
        double BrakeSteering
        {
            get;
        }

        ICar ICar
        {
            get;
        }

        IDictionary<string, double> GetRegulatorParameters();

        void OverrideTargetBrakeSetting(double setting);

        void EndTargetBrakeSteeringOverriding();
    }
}
