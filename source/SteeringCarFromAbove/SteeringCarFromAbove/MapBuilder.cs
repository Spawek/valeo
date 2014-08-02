using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public class MapBuilder
    {
        public MapBuilder(MarkerFinder markerFinder, ObstaclesFinder obstaclesFinder, ObjectsToTrace objectsToTrace)
        {
            markerFinder_ = markerFinder;
            obstaclesFinder_ = obstaclesFinder;
            objectsToTrace_ = objectsToTrace;
        }

        public Map BuildMap(Image image)
        {
            throw new NotImplementedException();
        }

        public Map RebuildMap(Image image, Map lastMap)
        {
            throw new NotImplementedException();
        }

        private MarkerFinder markerFinder_;
        private ObstaclesFinder obstaclesFinder_;
        private ObjectsToTrace objectsToTrace_;
    }
}
