using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteeringCarFromAbove
{
    public class PositionAndOrientation
    {
        public int x;
        public int y;

        private float __angle__;
        public float angle
        {
            get { return __angle__; }
            set
            {
                if (x >= 0.0f && x <= 1.0f)
                    __angle__ = value;
                else
                    throw new ArgumentException("angle out of range!");
            }
        }
    }
}
