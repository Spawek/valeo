using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Helpers
{
    public class StatsCollector
    {
        private IDictionary<string, LinkedList<double>> dict = new Dictionary<string, LinkedList<double>>();
        private StreamWriter sw;
        private bool keysWritten = false;
        private const int NEW_STATS_NO_BETW_WRITE_TO_FILE = 1000;
        private string outFileName;
        private Object FileLock = new Object();

        public StatsCollector()
        {
            outFileName = String.Format("stats_{0}.txt", DateTime.Now.ToString("d_M_yyyy__HH_mm_ss"));
            if (File.Exists(outFileName))
            { //but this file shouldn't exist anyway
                File.Delete(outFileName);
            }

            sw = new StreamWriter(outFileName);
        }

        private int putsFromLastSave = 0;
        public void PutNewStat(string variable, double value)
        {
            //create vector for key if not exists
            if (!dict.ContainsKey(variable))
            {
                if (keysWritten)
                    throw new ApplicationException("You can't add new keys after first save to file");

                dict[variable] = new LinkedList<double>();
            }

            dict[variable].AddLast(value);

            if (++putsFromLastSave > NEW_STATS_NO_BETW_WRITE_TO_FILE)
            {
                WriteStatsToFile();
                putsFromLastSave = 0;
            }
        }

        private void WriteStatsToFile()
        {
            Logger.Log(this, String.Format("Starting to save new stats: {0}", outFileName));

            lock (FileLock)
            {
                //write keys
                if (!keysWritten)
                {
                    foreach (string key in dict.Keys)
                    {
                        sw.Write("{0}; ", key);
                    }
                    sw.WriteLine();
                    keysWritten = true;
                }

                //write values
                bool somethingWasWritten = true;
                while (somethingWasWritten)
                {
                    somethingWasWritten = false;

                    foreach (string key in dict.Keys)
                    {
                        if (dict[key].Count > 0)
                        {
                            sw.Write("{0}; ", dict[key].First());
                            dict[key].RemoveFirst();
                            somethingWasWritten = true;
                        }
                        else
                        {
                            sw.Write(";");
                        }

                        if (key == dict.Keys.Last())
                        {
                            sw.WriteLine();
                        }
                    }
                }
            }
            Logger.Log(this, String.Format("Saving to file ended successfully: {0}", outFileName));
        }

    }
}
