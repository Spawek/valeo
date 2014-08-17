using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Helpers
{
    public abstract class PIDSettings
    {
        //P part settings
        public double P_FACTOR_MULTIPLER;

        //I part settings
        public double I_FACTOR_MULTIPLER;
        public double I_FACTOR_SUM_MAX_VALUE; // 100 is normal value
        public double I_FACTOR_SUM_MIN_VALUE; // -100 is normal value
        public double I_FACTOR_SUM_SUPPRESSION_PER_SEC; //1.0 = suppresing disabled

        //D part settings
        public double D_FACTOR_MULTIPLER; 
        public double D_FACTOR_SUPPRESSION_PER_SEC; //1.0 = suppressing disabled //
        public double D_FACTOR_SUM_MIN_VALUE; // 100 is normal value
        public double D_FACTOR_SUM_MAX_VALUE; // -100 is normal value

        //steering limits
        public double MAX_FACTOR_CONST; // 100 is normal value
        public double MIN_FACTOR_CONST; // -100.0 is normal value
    }
}
