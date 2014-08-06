using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteeringCarFromAbove
{
    class MathTools
    {
        //http://stackoverflow.com/questions/10065080/mod-explanation
        static double Mod(double a, double b)
        {
            return ((a % b) + b) % b;
        }
    }
}
