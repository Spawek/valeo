using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteeringCarFromAbove
{
    public class MathTools
    {
        /// <summary>
        /// http://stackoverflow.com/questions/10065080/mod-explanation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Mod(double a, double b)
        {
            return ((a % b) + b) % b;
        }

        /// <summary>
        /// http://blog.lexique-du-net.com/index.php?post/Calculate-the-real-difference-between-two-angles-keeping-the-sign
        /// modified for angles in range [0, 360] deg
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AnglesEqual(double a, double b, double tolerance)
        {
            double difference = Math.Abs(a - b);

            if (difference > 180.0d)
                return (360.0d - difference) < tolerance;
            else
                return difference < tolerance;
        }
    }
}
