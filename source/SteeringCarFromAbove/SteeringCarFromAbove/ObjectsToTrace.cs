using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public struct ObjectsToTrace
    {
        public ObjectsToTrace(List<Marker> _stableMarkers,
            Marker _carMarker, Marker _parkingMarker)
        {
            stableMarkers = _stableMarkers;
            carMarker = _carMarker;
            parkingMarker = _parkingMarker;
        }

        public List<Marker> stableMarkers;
        public Marker carMarker;
        public Marker parkingMarker;
    }
}
