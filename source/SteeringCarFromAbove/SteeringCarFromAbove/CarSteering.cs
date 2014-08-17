using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteeringCarFromAbove
{
    public class CarSteering
    {
        private double __speed__;
        public double speed
        {
            get { return __speed__; }
            set
            {
                if (value < -100.0d || value > 100.0d)
                    throw new ArgumentException();
                else __speed__ = value;
            }
        }

        private double __angle__;
        public double angle
        {
            get { return __angle__; }
            set
            {
                if (value < -60.0d || value > 60.0d)
                    throw new ArgumentException();
                else __angle__ = value;
            }
        }

        private double __brake__;
        public double brake
        {
            get { return __brake__; }
            set
            {
                if (value < 0.0d || value > 1.0d)
                    throw new ArgumentException();
                else __brake__ = value;
            }
        }

        public override string ToString()
        {
            return String.Format("Car steering:\nspeed:{0:0.##}\nangle:{1:0.##}\nbrake:{2:0.##}\n", speed, angle, brake);
        }
    }
}
