using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Helpers;

namespace CarController
{
    public class FakeCarCommunicator : ICarCommunicator
    {
        public event SpeedInfoReceivedEventHander evSpeedInfoReceived;
        public event SteeringWheelAngleInfoReceivedEventHandler evSteeringWheelAngleInfoReceived;
        public event BrakePositionReceivedEventHandler evBrakePositionReceived;

        private CarModel model;

        public ICar ICar { get; private set; }
        public ISpeedRegulator SpeedRegulator
        {
            get
            {
                return ICar.SpeedRegulator;
            }
        }
        public ISteeringWheelAngleRegulator SteeringWheelAngleRegulator
        {
            get
            {
                return ICar.SteeringWheelAngleRegulator;
            }
        }
        public IBrakeRegulator BrakeRegulator
        {
            get
            {
                return ICar.BrakeRegulator;
            }
        }

        private Thread communicationThread;
        private const int COMMUNICATION_THREAD_SLEEP_PER_LOOP = 10;

        public FakeCarCommunicator(ICar car)
        {
            ICar = car;
            //mFakeThread = new System.Threading.Thread(new ThreadStart(mFakeThreadTasks));
            //mFakeThread.Start();

            model = new CarModel(this);

            communicationThread = new Thread(ContinousCommunication);
            communicationThread.Start();
        }

        /// <summary>
        /// this has to be invoked before 1st use
        /// </summary>
        public void InitRegulatorsEventsHandling()
        {
            SpeedRegulator.evNewSpeedSettingCalculated += ISpeedRegulator_evNewSpeedSettingCalculated;
            SteeringWheelAngleRegulator.evNewSteeringWheelSettingCalculated += ISteeringWheelAngleRegulator_evNewSteeringWheelSettingCalculated;
            BrakeRegulator.evNewBrakeSettingCalculated += BrakeRegulator_evNewBrakeSettingCalculated;
        }

        void BrakeRegulator_evNewBrakeSettingCalculated(object sender, NewBrakeSettingCalculatedEventArgs args)
        {
            model.BrakeSteering = args.GetBrakeSetting();
        }

        void ISteeringWheelAngleRegulator_evNewSteeringWheelSettingCalculated(object sender, NewSteeringWheelSettingCalculateddEventArgs args)
        {
            model.WheelAngleSteering = args.getSteeringWheelAngleSetting();
        }

        void ISpeedRegulator_evNewSpeedSettingCalculated(object sender, NewSpeedSettingCalculatedEventArgs args)
        {
            model.SpeedSteering = args.getSpeedSetting();
        }

        void ContinousCommunication()
        {
            while (true)
            {
                try
                {
                    SteeringWheelAngleInfoReceivedEventHandler tempAngleEvent = evSteeringWheelAngleInfoReceived;
                    if (tempAngleEvent != null)
                    {
                        tempAngleEvent(this, new SteeringWheelAngleInfoReceivedEventArgs(model.WheelAngle));
                    }

                    SpeedInfoReceivedEventHander tempSpeedEvent = evSpeedInfoReceived;
                    if (tempSpeedEvent != null)
                    {
                        tempSpeedEvent(this, new SpeedInfoReceivedEventArgs(model.Speed));
                    }

                    BrakePositionReceivedEventHandler temp = evBrakePositionReceived;
                    if (temp != null)
                    {
                        temp(this, new BrakePositionReceivedEventArgs(model.BrakePosition));
                    }

                    Thread.Sleep(COMMUNICATION_THREAD_SLEEP_PER_LOOP);
                }
                catch (Exception e)
                {
                    Logger.Log(this, String.Format("Fake car communicator exception catched: {0}", e.Message), 2);
                    Logger.Log(this, String.Format("Fake car communicator exception stack: {0}", e.StackTrace), 1);
                }
            }
        }

        void mFakeThreadTasks()
        {
            System.Threading.Thread.Sleep(1000); //wait 1s
            evSpeedInfoReceived(this, new SpeedInfoReceivedEventArgs(25.0));
            evSteeringWheelAngleInfoReceived.Invoke(this, new SteeringWheelAngleInfoReceivedEventArgs(10.0));

            System.Threading.Thread.Sleep(1000); //wait 1s
            evSpeedInfoReceived(this, new SpeedInfoReceivedEventArgs(30.0));
            evSteeringWheelAngleInfoReceived.Invoke(this, new SteeringWheelAngleInfoReceivedEventArgs(5.0));

            System.Threading.Thread.Sleep(1000); //wait 1s
            evSpeedInfoReceived(this, new SpeedInfoReceivedEventArgs(35.0));
            evSteeringWheelAngleInfoReceived.Invoke(this, new SteeringWheelAngleInfoReceivedEventArgs(0.0));
        }

        public void SetGear(Gear gear)
        {
            model.Gear = gear;
        }


        public event GearPositionReceivedEventHandler evGearPositionReceived;
    }
}

