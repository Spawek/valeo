using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public class Image
    {
        public Image(int _maxX, int _maxY)
        {
            maxX = _maxX;
            maxY = _maxY;
        }

        public int maxX;
        public int maxY;
    }
}
