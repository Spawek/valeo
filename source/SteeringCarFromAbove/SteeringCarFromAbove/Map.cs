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
        public Map(int mapX, int mapY)
        {
            mapSizeX = mapX;
            mapSizeY = mapY;
        }

        public int mapSizeX { get; private set; }
        public int mapSizeY { get; private set; }

        public PositionAndOrientation car = null;
        public PositionAndOrientation parking = null;
        public IDictionary<string, PositionAndOrientation> markers = null;
        public List<Rectangle> obstacles = new List<Rectangle>();
    }

}
