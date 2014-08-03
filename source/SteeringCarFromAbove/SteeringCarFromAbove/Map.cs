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
        public Map(int biggestDimSize, int imageMaxX, int imageMaxY)
        {
            //imageMaxX_ = imageMaxX;
            //imageMaxY_ = imageMaxY;

            //if (imageMaxX >= imageMaxY)
            //{
            //    int smallestDimSize = biggestDimSize * imageMaxX / imageMaxY;
            //    mapSizeX_ = biggestDimSize;
            //    mapSizeY_ = smallestDimSize;
            //}
            //else
            //{
            //    int smallestDimSize = biggestDimSize * imageMaxY / imageMaxX;
            //    mapSizeX_ = smallestDimSize;
            //    mapSizeY_ = biggestDimSize;
            //}

            //map_ = new MapPoint[mapSizeX_, mapSizeY_];
        }

        private int mapSizeX_;
        private int mapSizeY_;

        public int mapSizeX { get { return mapSizeX_; } }
        public int mapSizeY { get { return mapSizeY_; } }

        public double distanceBetweenPoints = -666.666f;  // in m

        private int imageMaxX_;
        private int imageMaxY_;

        public PositionAndOrientation car = null;
        public PositionAndOrientation parking = null;
        public IDictionary<Marker, PositionAndOrientation> markers = null;

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
