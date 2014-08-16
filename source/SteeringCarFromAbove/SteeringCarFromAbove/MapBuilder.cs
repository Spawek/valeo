using AForge.Vision.GlyphRecognition;
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

        public Map BuildMap(System.Drawing.Bitmap bitmap, List<ExtractedGlyphData> glyphs)
        {
            Map map = new Map(bitmap.Size.Width, bitmap.Size.Height);

            IDictionary<string, PositionAndOrientation> markersPositions = markerFinder_.FindMarkers(glyphs);

            if (markersPositions.ContainsKey("car") && markersPositions.Count > 1)
            {
                map.car = markersPositions["car"];
                map.markers = markersPositions.Where(x => x.Key.StartsWith("static")).ToDictionary(x => x.Key, x => x.Value);
                map.obstacles = obstaclesFinder_.FindObstacles(bitmap);
            }
            else
            {
                //System.Windows.Forms.MessageBox.Show("not sufficient informations to build a map");

                return null;
            }

            return map;
        }

        public void UpdateCarPosition(Map baseMap, List<ExtractedGlyphData> glyphs)
        {
            IDictionary<string, PositionAndOrientation> markersPositions = markerFinder_.FindMarkers(glyphs);

            if (markersPositions.ContainsKey("car") && markersPositions.All(x => baseMap.markers.ContainsKey(x.Key)))
            {
                PositionAndOrientation carPosition = markersPositions["car"];
                IDictionary<string, PositionAndOrientation> stableMarkersPosition =
                    markersPositions.Where(x => x.Key.StartsWith("static")).ToDictionary(x => x.Key, x => x.Value);

                double averageAngleChange =
                    stableMarkersPosition.Average(x => x.Value.angle - baseMap.markers[x.Key].angle);
                IDictionary<string, PositionAndOrientation> stableMarkersPositionWithAngleCorrection =
                    stableMarkersPosition.ToDictionary(x => x.Key,
                        x => TransformPositionOnAngleChange(x.Value, -averageAngleChange, baseMap.mapSizeX, baseMap.mapSizeY));
                double averageXChange =
                    stableMarkersPositionWithAngleCorrection.Average(x => x.Value.x - baseMap.markers[x.Key].x);
                double averageYChange =
                    stableMarkersPositionWithAngleCorrection.Average(x => x.Value.y - baseMap.markers[x.Key].y);

                PositionAndOrientation correctedCarPosition =
                    TransformPositionOnAngleChange(carPosition, -averageAngleChange, baseMap.mapSizeX, baseMap.mapSizeY);
                correctedCarPosition.x -= averageXChange;
                correctedCarPosition.y -= averageYChange;

                baseMap.car = correctedCarPosition;
                Console.WriteLine("car position has been updated");
            }
            else
            {
                Console.WriteLine("car position cannot be updated basing on current frame");
            }
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
