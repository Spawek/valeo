using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pololu.UsbWrapper;
using Pololu.Usc;
using Helpers;
using CarController;
using CarController.Model.Communicators;

//Created by Mateusz Nowakowski
//Refactored and merged by Maciej (Spawek) Oziebly 
namespace car_communicator
{
    public class ServoDriver : Device
    {

        public override string ToString()
        {
            return "Servo Driver";
        }

        public const int THROTTLE_CHANNEL = 1;

        public const int MAX_THROTTLE = 7000;
        public const int MIN_THROTTLE = 3900;

        Usc Driver = null;  

        private bool effectorsActive = false;
        private double lastThrottleInPercentsWantedToBeSet = 0.0; //this throttle could not been set because effectors could be not active

        protected override void Initialize()
        {
            List<DeviceListItem> list = Usc.getConnectedDevices();

            if (list.Count == 1)
            {
                Driver = new Usc(list[0]);
                this.overallState = DeviceOverallState.OK;
            }
            else if (list.Count == 0)
            {
                Logger.Log(this, "there are no connected USC devices - servo driver can't start", 2);
                this.overallState = DeviceOverallState.Error;
            }
            else //more than 1 device
            {
                Logger.Log(this, "there are more than 1 USC devices - trying to connect last of them", 2);
                Driver = new Usc(list[list.Count - 1]); //last device
                //TODO: add device recognising
                this.overallState = DeviceOverallState.Warrning;
            }
        }

        protected override void StartSensors()
        {
            //no sensors in here
        }

        protected override void StartEffectors()
        {
            effectorsActive = true;
            setThrottle(lastThrottleInPercentsWantedToBeSet);
        }

        protected override void PauseEffectors()
        {
            effectorsActive = false;
            setThrottle(0.0);
        }

        protected override void EmergencyStop()
        {
            effectorsActive = false;
            setThrottle(0.0);
        }

        private void setTarget(byte channel, ushort target)
        {
            if (Driver != null) //checking if ServoDriver is Initialized
            {
                if (channel == THROTTLE_CHANNEL)
                {
                    if (target < MIN_THROTTLE || target > MAX_THROTTLE)
                    {
                        throw new ApplicationException("wrong target");
                    }
                }
                else
                {
                    throw new ApplicationException("unknown channel");
                }

                try
                {
                    Driver.setTarget(channel, target);
                }
                catch (Exception)
                {
                    Logger.Log(this, "couldnt send msg to servo!", 2);
                }
            }
            else
            {
                Logger.Log(this, "target was not set, because ServoDriver is not initialized", 1);
            }
        }

        public void setThrottle(double valueInPercents)
        {
            if (effectorsActive)
            {
                if (valueInPercents < 0 || valueInPercents > 100)
                    throw new ApplicationException("wrong values - it should be in range 0 to 100%");

                Helpers.ReScaller.ReScale(ref valueInPercents, 0, 100, (double)MIN_THROTTLE, (double)MAX_THROTTLE);

                setTarget(THROTTLE_CHANNEL, (ushort)valueInPercents);
            }
            else
            {
                Logger.Log(this, "target throttle was not set, effectors are disabled");
            }
        }
            
    }
}
        