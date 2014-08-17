using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public static class ReScaller
    {
        public static double ReScale(ref double value, double currMinValue, double currMaxValue, double targetMinValue, double targetMaxValue)
        {
            //if (value < currMinValue || value > currMaxValue)
            //{
            //    throw new ArgumentException("calue is not in current range");
            //}

            return value = (value - currMinValue) * (targetMaxValue - targetMinValue) / (currMaxValue - currMinValue) + targetMinValue;
        }
    }
}
