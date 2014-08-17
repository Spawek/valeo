using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public static class Time
    {
        static DateTime firstGetTimeUsageDate;
        static bool isItFirstTimeGet = true;
        /// <summary>
        /// return time from 1st usage of this foo
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTimeFromProgramBeginnig()
        {
            if (isItFirstTimeGet)
            {
                isItFirstTimeGet = false;
                firstGetTimeUsageDate = DateTime.Now;
            }

            return DateTime.Now - firstGetTimeUsageDate;
        }
    }
}
