using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helpers
{
    public static class Logger
    {
        private static string logFileName = "Log";
        private static string logFileExtension = ".txt";
        private static bool isItFirstLog = true;
        private const int MAX_PRIORITY = 10;
        private const int MIN_PRIORITY_TO_SHOW_IN_CONSOLE = 0; //in range[0, 10]
        private const int MIN_PRIORITY_TO_WRINT_IN_FILE = 0; //10 = disabled

        /// <summary></summary>
        /// <param name="loggingObj"></param>
        /// <param name="msg"></param>
        /// <param name="priority">0 = lowest, more -> bigger priority (MAX=10)</param>
        public static void Log(Object loggingObj, string msg, int priority = 0)
        {
            if (priority > MAX_PRIORITY)
                throw new ArgumentException("priority is too big", "priority");

            if (isItFirstLog)
            {
                isItFirstLog = false;

                //removing old log files
                for (int i = 0; i <= MAX_PRIORITY; i++) //for priorities > 0
                {
                    if (File.Exists(logFileName + Convert.ToString(i)))
                    {
                        File.Delete(logFileName + Convert.ToString(i));
                    }    
                }

            }

            string priorityMsg = String.Empty;
            if (priority > 0)
            {
                priorityMsg = String.Format("<<<PRIORITY:{0}>>>", priority);
            }

            string loggingObjName = String.Empty;
            if(loggingObj != null)
            {
                loggingObjName = loggingObj.ToString();
            }

            string msgWithDateAndObjectName = String.Format("{0}[{1}]:<<'{2}'>>:   {3}",
                priorityMsg,
                loggingObjName,
                String.Format(@"{0:mm\:ss\:ff}", Time.GetTimeFromProgramBeginnig()),
                msg
            );
            
            if(priority >= MIN_PRIORITY_TO_SHOW_IN_CONSOLE)
            {
                Console.WriteLine(msgWithDateAndObjectName);
            }

            for (int i = MIN_PRIORITY_TO_WRINT_IN_FILE; i <= priority; i++)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(logFileName + Convert.ToString(i) + logFileExtension, true)) //TODO: add some buffors or something, files are being oppened and closed all the time now
                    {
                        sw.WriteLine(msgWithDateAndObjectName);
                    } 
                }
                catch (Exception e)
                {
                    Console.WriteLine(String.Format("Logger couldn't write above msg to log, error msg: {0}", e.Message)); //TODO: //IMPORTANT: IT HAPPEN SOMETIMES - it shouldnt
                }
            }

        }

    }
}
