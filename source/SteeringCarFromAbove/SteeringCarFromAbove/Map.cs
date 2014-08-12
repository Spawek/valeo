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
        public IDictionary<Marker, PositionAndOrientation> markers = null;
        public List<Rectangle> obstacles = null;

        private MapPoint[,] map_;
        public MapPoint this[int x, int y]
        {
            get { return map_[x, y]; }
            set { map_[x, y] = value; }
        }

        public int MapPointsCountX { get { return map_.GetLength(0); } }
        public int MapPointsCountY { get { return map_.GetLength(1); } }
    }

}
