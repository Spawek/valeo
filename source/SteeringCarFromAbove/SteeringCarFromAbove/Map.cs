using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public enum MapPoint
    {
        FREE, OBSTACLE, CAR, PARKING
    }

    public class Map
    {
        public Map(int width, int height)
        {
            mapWidth = width;
            mapHeight = height;
        }

        public int mapWidth { get; private set; }
        public int mapHeight { get; private set; }

        public PositionAndOrientation car = null;
        public PositionAndOrientation parking = null;
        public IDictionary<string, PositionAndOrientation> markers = null;
        public List<Rectangle> obstacles = new List<Rectangle>();

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("Map:\nwidth{0}\nheight:{1}\n\n", mapWidth, mapHeight));
            if (car != null)
            {
                sb.Append(String.Format("car:\n{0}\n\n", car.ToString()));
            }
            if (parking != null)
            {
                sb.Append(String.Format("parking:\n{0}\n\n", parking.ToString()));
            }
            foreach (var marker in markers)
            {
                sb.Append(String.Format("{0}:\n{1}\n\n", marker.Key, marker.Value.ToString()));
            }

            return sb.ToString();
        }
    }

}
