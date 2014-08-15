using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public struct ObjectsToTrace
    {
        public ObjectsToTrace(List<string> _stableMarkers,
            string _carMarker, string _parkingMarker)
        {
            stableMarkers = _stableMarkers;
            carMarker = _carMarker;
            parkingMarker = _parkingMarker;
        }

        public List<string> stableMarkers;
        public string carMarker;
        public string parkingMarker;
    }
}
