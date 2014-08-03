using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public class PositionAndOrientation
    {
        public PositionAndOrientation(double _x, double _y, double _angle)
        {
            x = _x;
            y = _y;
            angle = _angle;
        }

        public PositionAndOrientation()
        {
        }

        public double x;
        public double y;
        public double angle;
    }
}
