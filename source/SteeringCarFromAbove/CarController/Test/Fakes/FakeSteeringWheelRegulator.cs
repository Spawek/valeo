using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public class FakeSteeringWheelRegulator : ISteeringWheelAngleRegulator
    {
        public event NewSteeringWheelSettingCalculatedEventHandler evNewSteeringWheelSettingCalculated;

        public ICar Car { get; set; }

        public double WheelAngleSteering { get; set; }

        public IDictionary<string, double> GetRegulatorParameters()
        {
            return new Dictionary<string, double>();
        }
    }
}
