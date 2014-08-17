using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using car_communicator;
using Helpers;
using System.Threading;

namespace CarController.Model.Communicators
{
    class Speedometer : Device
    {
        struct SpeedMeasurementPoint
        {
            public int ticks;
            public DateTime measurementTime;

            public SpeedMeasurementPoint(int _ticks)
            {
                ticks = _ticks;
                measurementTime = DateTime.Now;
            }
        };


        public override string ToString()
        {
            return "Speedometer";
        }

        public event SpeedInfoReceivedEventHander evSpeedInfoReceived;

        private USB4702 extentionCardCommunicator;

        private const int SPEED_MEASURING_THREAD_SLEEP_PER_LOOP_IN_MS = 10; //should be lowest possible
        private const double WHEEL_CIRCUIT_IN_M = 0.548 * Math.PI; //informacja of Filipa Godlewskiego z grupy mechaniki - oby prawdziwa ;)
        private const double NO_OF_HAAL_METERS = 5.0;
        private const double DISTANCE_PER_HAAL_METER_CATCHED = WHEEL_CIRCUIT_IN_M / NO_OF_HAAL_METERS;
        private const int TICKS_TO_RESTART = 10000;
        private const double MIN_SECONDS_ON_MEASUREMENTS_LIST = 1.0;
        private const int MIN_MEASUREMENTS_ON_MEASUREMENTS_LIST = 3;

        private Thread SpeedMeasuringThread;
        private int lastTicks = 0;
        private LinkedList<SpeedMeasurementPoint> measurePoints = new LinkedList<SpeedMeasurementPoint>();

        public Speedometer(USB4702 extentionCard)
        {
            extentionCardCommunicator = extentionCard;

            SpeedMeasuringThread = new Thread(new ThreadStart(ConstantSpeedMeasuring));
        }

        void ConstantSpeedMeasuring()
        {
            while (true)
            {
                try
                {
                    int ticks = extentionCardCommunicator.getSpeedCounterStatus();
                    int newTicks = ticks - lastTicks;
                    measurePoints.AddLast(new SpeedMeasurementPoint(newTicks));

                    CleanUpMeasurePoints();

                    TimeSpan totalTimeOnMeasurementsList = measurePoints.Last().measurementTime - measurePoints.First().measurementTime;
                    int totalTicksOnMeasurementsList = measurePoints.Sum(x => x.ticks);

                    double speedInMetersPerSecond = totalTicksOnMeasurementsList * DISTANCE_PER_HAAL_METER_CATCHED / totalTimeOnMeasurementsList.TotalSeconds;

                    if (ticks > TICKS_TO_RESTART)
                    {
                        extentionCardCommunicator.RestartSpeedCounter();
                        lastTicks = 0;
                    }

                    if (Double.IsNaN(speedInMetersPerSecond) || Double.IsInfinity(speedInMetersPerSecond)) //WORKARROUND
                    {
                        Logger.Log(this, "speed from speedometer is NaN", 2);
                    }
                    else
                    {
                        SpeedInfoReceivedEventHander SpeedEvent = evSpeedInfoReceived;
                        if (SpeedEvent != null)
                        {
                            SpeedEvent(this, new SpeedInfoReceivedEventArgs(speedInMetersPerSecond));
                        }
                    }

                    lastTicks = ticks;

                    Thread.Sleep(SPEED_MEASURING_THREAD_SLEEP_PER_LOOP_IN_MS);
                }
                catch (Exception e)
                {
                    Logger.Log(this, String.Format("Speed measuring exception catched: {0}", e.Message), 2);
                    Logger.Log(this, String.Format("Speed measuring exception stack: {0}", e.StackTrace), 1);
                }
            }
        }

        /// <summary>
        /// removes points from front of measurePoints if theare are measurements
        /// from longer that MIN_SECONDS_ON_MEASUREMENTS_LIST seconds AND there are more than MIN_MEASUREMENTS_ON_MEASUREMENTS_LIST measurements
        /// </summary>
        private void CleanUpMeasurePoints()
        {
            bool measurementRemoved = true;
            while(measurementRemoved)
            {
                measurementRemoved = false;
                if(measurePoints.Count > 3)
                {
                    DateTime secondMeasurementTime = measurePoints.First.Next.Value.measurementTime;
                    TimeSpan timeFromSecondToLastMeasurement = measurePoints.Last().measurementTime - secondMeasurementTime;
 	                while(timeFromSecondToLastMeasurement.TotalSeconds > MIN_SECONDS_ON_MEASUREMENTS_LIST)
                    {
                        measurePoints.RemoveFirst();
                        measurementRemoved = true;

                        secondMeasurementTime = measurePoints.First.Next.Value.measurementTime;
                        timeFromSecondToLastMeasurement = measurePoints.Last().measurementTime - secondMeasurementTime;
                    }
                }
            }
        }

        protected override void Initialize()
        {
            //no init - its dummy device - wrapper for speed measuring
        }

        protected override void StartSensors()
        {
            SpeedMeasuringThread.Start();
        }

        protected override void StartEffectors()
        {
            //no effectors in here 
        }

        protected override void PauseEffectors()
        {
            //no effectors in here
        }

        protected override void EmergencyStop()
        {
            //no effectors in here
        }
    }
}
