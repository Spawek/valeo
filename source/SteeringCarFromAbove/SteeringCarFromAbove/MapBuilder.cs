using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public class MapBuilder
    {
        public MapBuilder(MarkerFinder markerFinder,
            ObstaclesFinder obstaclesFinder,
            ObjectsToTrace objectsToTrace)
        {
            markerFinder_ = markerFinder;
            obstaclesFinder_ = obstaclesFinder;
            objectsToTrace_ = objectsToTrace;
        }

        public Map BuildMap(Image image)
        {
            Map map = new Map(image.maxX, image.maxY);
            map.car = markerFinder_.FindMarker(image, objectsToTrace_.carMarker);
            map.parking = markerFinder_.FindMarker(image, objectsToTrace_.parkingMarker);
            map.markers = objectsToTrace_.stableMarkers.ToDictionary(x => x, x => markerFinder_.FindMarker(image, x));

            return map;
        }

        public void UpdateCarPosition(Map baseMap, Image image)
        {
            PositionAndOrientation carPosition =
                markerFinder_.FindMarker(image, objectsToTrace_.carMarker);
            IDictionary<Marker, PositionAndOrientation> stableMarkersPosition =
                objectsToTrace_.stableMarkers.ToDictionary(x => x, x => markerFinder_.FindMarker(image, x));

            double averageAngleChange =
                stableMarkersPosition.Average(x => x.Value.angle - baseMap.markers[x.Key].angle);
            IDictionary<Marker, PositionAndOrientation> stableMarkersPositionWithAngleCorrection =
                stableMarkersPosition.ToDictionary(x => x.Key,
                    x => TransformPositionOnAngleChange(x.Value, -averageAngleChange, image.maxX, image.maxY));
            double averageXChange =
                stableMarkersPositionWithAngleCorrection.Average(x => x.Value.x - baseMap.markers[x.Key].x);
            double averageYChange =
                stableMarkersPositionWithAngleCorrection.Average(x => x.Value.y - baseMap.markers[x.Key].y);

            PositionAndOrientation correctedCarPosition =
                TransformPositionOnAngleChange(carPosition, -averageAngleChange, image.maxX, image.maxY);
            correctedCarPosition.x -= averageXChange;
            correctedCarPosition.y -= averageYChange;

            baseMap.car = correctedCarPosition;
        }

        /// <summary>
        /// transformation by change to polar coordinate system
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <param name="angleChange"></param>
        /// <param name="imageMaxX"></param>
        /// <param name="imageMaxY"></param>
        /// <returns></returns>
        private PositionAndOrientation TransformPositionOnAngleChange(
            PositionAndOrientation currentPosition, double angleChange, int imageMaxX, int imageMaxY)
        {
            PositionAndOrientation output = new PositionAndOrientation();
            output.angle = currentPosition.angle + angleChange;

            double oldX = currentPosition.x - (imageMaxX / 2);
            double oldY = currentPosition.y - (imageMaxY / 2);
            double r = Math.Sqrt(Math.Pow(oldX, 2) + Math.Pow(oldY, 2));
            double oldAngle = Math.Acos(oldX / r) * Math.Sign(oldY);
            double newAngle = oldAngle + angleChange / 180.0f * Math.PI;
            output.x = r * Math.Cos(newAngle);
            output.y = r * Math.Sin(newAngle);

            return output;
        }

        private MarkerFinder markerFinder_;
        private ObstaclesFinder obstaclesFinder_;
        private ObjectsToTrace objectsToTrace_;
    }
}
