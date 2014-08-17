using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public class FakeBrakeRegulator : IBrakeRegulator
    {
        public event NewBrakeSettingCalculatedEventHandler evNewBrakeSettingCalculated;

        public double BrakeSteering { get; private set; }

        public ICar ICar { get; private set; }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return new Dictionary<string, double>();
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
