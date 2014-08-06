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

        static double Mod(double a, double n)
        {
            return ((a % n) + n) % n;
        }

        public double x;
        public double y;

        private double __angle__;
        public double angle
        {
            get { return __angle__; }
            set { __angle__ = Mod(value, 360.0f); }
        }
    }
}
