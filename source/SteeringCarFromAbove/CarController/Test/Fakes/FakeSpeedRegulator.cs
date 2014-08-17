using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public class FakeSpeedRegulator : ISpeedRegulator
    {
        public event NewSpeedSettingCalculatedEventHandler evNewSpeedSettingCalculated;

        public ICar Car { get; private set; }

        public double SpeedSteering { get; private set; }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return new Dictionary<string, double>();    
        }
    }
}
