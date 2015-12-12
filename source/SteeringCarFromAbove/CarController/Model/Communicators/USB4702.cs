using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Automation.BDaq;
using Helpers;
using CarController.Model.Communicators;
using System.Threading;

namespace car_communicator
{
    /// <summary>
    /// extension card connector
    /// </summary>
    public class USB4702 : Device
    {

        public override string ToString()
        {
            return "extentionCardCommunicator";    
        }

        static int buffer;
        static InstantAoCtrl instantAoCtrl = new InstantAoCtrl(); //for analog outputs
        static InstantDoCtrl instantDoCtrl = new InstantDoCtrl(); //for digital outputs
        static InstantDiCtrl instantDiCtrl = new InstantDiCtrl(); //for digital inputs
        static EventCounterCtrl eventSpeedCounterCtrl = new EventCounterCtrl(); // for initialize counter

        const string USB4702_DEVICE_DESCRIPTION_STRING = "USB-4702,BID#0"; // '0' -> 1st extension card

        const int MAX_TRIES_TO_INITIALIZE_BEFORE_ERROR = 60;
        const int SLEEP_BEETWEEN_TRIES_TO_INITIALIZE_IN_MS = 10; //needed, because initialization uses all the system resources when it is in initialization loop

        const int STEERING_WHEEL_SET_PORT = 0;
        const double STEERING_WHEEL_MIN_SET_VALUE_IN_VOLTS = 1.2; //1.0 can cause error but its teoretical min
        const double STEERING_WHEEL_MID_SET_VALUE_IN_VOLTS = 2.5; // środek
        const double STEERING_WHEEL_MAX_SET_VALUE_IN_VOLTS = 3.8; //4.0 is teoretical max

        const int BRAKE_STRENGTH_SET_PORT = 1;
        const double BRAKE_MIN_SET_VALUE_IN_VOLTS = 0.0; //from 0
        const double BRAKE_MAX_SET_VALUE_IN_VOLST = 4.2; //from 5

        const double MIN_STRENGTH_FOR_BRAKE_TO_REACT_IN_PERCENTS = 5;

        const int BRAKE_ENABLE_PORT_NO = 0;
        const byte BRAKE_ENABLE_ON_PORT_LEVEL = 0; 
        const byte BRAKE_ENABLE_OFF_PORT_LEVEL = 1;

        const int BRAKE_DIRECTION_PORT_NO = 1;
        const int BRAKE_BACKWARD_PORT_LEVEL = 1; 
        const int BRAKE_FORWARD_PORT_LEVEL = 0;

        const int BRAKE_STOP_PORT_NO = 2;
        const byte BRAKE_STOP_ON_PORT_LEVEL = 0;
        const byte BRAKE_STOP_OFF_PORT_LEVEL = 1;

        const int IGNITION_PORT_NO = 4; //zaplon
        const int IGNITION_ON_PORT_LEVEL = 0;
        const int IGNITION_OFF_PORT_LEVEL = 1;
        const int IGNTITION_OFF_TIME_IN_MS = 5000;
        const int IGNITION_ON_TIME_IN_MS = 3500;

        const int STARTER_PORT_NO = 5; //rozrusznik
        const int STARTER_ON_PORT_LEVEL = 1;
        const int STARTER_OFF_PORT_LEVEL = 0;
        const int STARTER_ON_TIME_IN_MS = 1700;

        const int STEERING_WHEEL_ENABLER_PORT_NO = 3; //if you dont turn it on you wouldn't be able to move wheel (it'll be set to 0 power (2,5V))
        const int STEERING_WHEEL_ENABLER_ON_PORT_LEVEL = 1;
        const int STEERING_WHEEL_ENABLER_OFF_PORT_LEVEL = 0;

        const int DIGITAL_INTPUT_PORT_NO = 0;

        private bool effectorsActive = false;

        protected override void Initialize()
        {
            TryToInitializeUntillItSucceds();
        }

        private void TryToInitializeUntillItSucceds()
        {
            bool initializationFinished = false;
            int tries = 0;
            do
            {
                if (++tries > MAX_TRIES_TO_INITIALIZE_BEFORE_ERROR)
                {
                    Logger.Log(this, "extentionCardCommunicator could not be initialized - max tries no hes been exceeded", 3);
                    this.overallState = DeviceOverallState.Error;
                    initializationFinished = true; //it failed, but its end of trying
                }
                try
                {
                    //Analog outputs
                    instantAoCtrl.SelectedDevice = new DeviceInformation(USB4702_DEVICE_DESCRIPTION_STRING); // AO0

                    //Digital output
                    instantDoCtrl.SelectedDevice = new DeviceInformation(USB4702_DEVICE_DESCRIPTION_STRING);
                        
                    instantDiCtrl.SelectedDevice = new DeviceInformation(USB4702_DEVICE_DESCRIPTION_STRING);

                    //Counter
                    eventSpeedCounterCtrl.SelectedDevice = new DeviceInformation(USB4702_DEVICE_DESCRIPTION_STRING);

                    eventSpeedCounterCtrl.Channel = 0;
                    eventSpeedCounterCtrl.Enabled = true;

                    setPortDO(BRAKE_ENABLE_PORT_NO, BRAKE_ENABLE_ON_PORT_LEVEL); //enabling brake engine

                    initializationFinished = true; //no exceptions and get here - everything is ok!
                    this.overallState = DeviceOverallState.OK;
                }
                catch (Exception e)
                {
                    Logger.Log(this, String.Format("cannot initialize connection for extentionCardCommunicator, tries left: {0}", MAX_TRIES_TO_INITIALIZE_BEFORE_ERROR - tries), 2);
                    Logger.Log(this, String.Format("Exception received: {0}", e.Message), 2);
                }
            } while (!initializationFinished);
        }

        protected override void StartSensors()
        {
            //no sensors in here
        }

        protected override void StartEffectors()
        {
            effectorsActive = true;

            EnableSteeringWheelSteering();
            TurnOnEngine();
        }

        protected override void PauseEffectors()
        {
            SetSteeringWheel(0.0);
            DisableSteeringWheelSteering();
            TurnOffEngine();

            effectorsActive = false;
        }

        protected override void EmergencyStop()
        {
            SetSteeringWheel(0.0); //do not move steering wheel

            DisableSteeringWheelSteering();
            TurnOffEngine();

            effectorsActive = false;
        }
        
        /// <summary>
        /// sets "value" voltage on channel no "channel"
        /// </summary>
        /// <param name="channel">channel no</param>
        /// <param name="value">0-5V (will be checked anyway - throws if bad)</param>
        private void setPortAO(int channel, double value)
        {
            if (effectorsActive)
            {
                if (value > 5 || value < 0)
                    throw new ArgumentException("value is not in range", "value");

                try
                {
                    instantAoCtrl.Write(channel, value);
                }
                catch (InvalidCastException)
                {
                    Logger.Log(this, "msg couldn't been send via extentionCardCommunicator - probably because of no connection", 2);
                }
            }
            else
            {
                Logger.Log(this, "analog port was not set, because effectors are no active");
            }
        }

        /// <summary>
        /// sets 0/1 "level" on port "port"
        /// </summary>
        /// <param name="port">port no</param>
        /// <param name="level">0/1</param>
        private void setPortDO(int port, byte level)
        {
            if (effectorsActive)
            {
                try
                {
                    if (level == 1)
                    {
                        buffer |= (1 << port);
                        instantDoCtrl.Write(0, (byte)buffer);
                    }
                    else if (level == 0)
                    {
                        buffer &= ~(1 << port);
                        instantDoCtrl.Write(0, (byte)buffer);
                    }
                    else
                    {
                        throw new ArgumentException("wrong level value - it should be 0/1", "level");
                    }

                }
                catch (Exception)
                {
                    Logger.Log(this, "msg couldn't been send via extentionCardCommunicator - probably because of no connection", 2);
                }
            }
            else
            {
                Logger.Log(this, "analog port was not set, because effectors are no active");
            }
        }

        //working
        public void RestartSpeedCounter()
        {
            eventSpeedCounterCtrl.Enabled = false;
            eventSpeedCounterCtrl.Enabled = true;
        }

        public int getSpeedCounterStatus()
        {
            if (!eventSpeedCounterCtrl.Initialized)
            {
                Logger.Log(this, "Speed counter is not initialized and cannot provide any data", 2);
                return 0;
            }
            return eventSpeedCounterCtrl.Value;
            
        }

        private void TurnOnEngine()
        {
            Logger.Log(this, "Starting ignition", 2);
            setPortDO(IGNITION_PORT_NO, IGNITION_ON_PORT_LEVEL);
            Thread.Sleep(IGNITION_ON_TIME_IN_MS);

            Logger.Log(this, "Starting engine starter (rozrusznik)", 2);
            setPortDO(STARTER_PORT_NO, STARTER_ON_PORT_LEVEL);
            Thread.Sleep(STARTER_ON_TIME_IN_MS);

            Logger.Log(this, "Stopping engine starter (rozrusznik)", 2);
            setPortDO(STARTER_PORT_NO, STARTER_OFF_PORT_LEVEL);
        }

        private void TurnOffEngine()
        {
            Logger.Log(this, "Stopping ignition (zaplon)", 2);
            setPortDO(IGNITION_PORT_NO, IGNITION_OFF_PORT_LEVEL);
            Thread.Sleep(IGNTITION_OFF_TIME_IN_MS);

            Logger.Log(this, "Starting ignition (zaplon)", 2);
            setPortDO(IGNITION_PORT_NO, IGNITION_ON_PORT_LEVEL);
        }

        private void DisableSteeringWheelSteering()
        {
            //setPortDO(STEERING_WHEEL_ENABLER_PORT_NO, STEERING_WHEEL_ENABLER_OFF_PORT_LEVEL);
        }

        private void EnableSteeringWheelSteering()
        {
            setPortDO(STEERING_WHEEL_ENABLER_PORT_NO, STEERING_WHEEL_ENABLER_ON_PORT_LEVEL);
        }

        /// <summary>
        /// sets power send to steering wheel in percents of power 
        /// (minus values are counterclockwise, plus values are clockwise)
        /// </summary>
        /// <param name="strength">
        /// -100 = max counterclockwise [in percents]
        /// +100 = max clockwise [in percents]
        /// </param>
        public void SetSteeringWheel(double strength)
        {
            EnableSteeringWheelSteering(); //ITS BUG FIX - DO NOT REMOVE! w/o this we will acquire steering wheel error
            if (strength < -100 || strength > 100)
            {
                Logger.Log(this, "steering wheel strength is not in range", 2);
                throw new ArgumentException("strenght is not in range");
            }

            Helpers.ReScaller.ReScale(ref strength, -100, 100, STEERING_WHEEL_MIN_SET_VALUE_IN_VOLTS, STEERING_WHEEL_MAX_SET_VALUE_IN_VOLTS);

            setPortAO(STEERING_WHEEL_SET_PORT, strength);
        }

        public List<bool> ReadDigitalInputs()
        {
            List<bool> output = new List<bool>();
            byte data;
            
            ErrorCode code = instantDiCtrl.Read(DIGITAL_INTPUT_PORT_NO, out data);

            if (code != ErrorCode.Success)
            {
                Logger.Log(this, String.Format("Reading digital input failed! Error code: {0}", code.ToString()), 3);
            }

            for (int i = 0; i < 8; i++)
            {
                bool currBit = data % 2 == 1;
                output.Add(currBit);
                data >>= 1;
            }

            return output;
        }

        const int GEARBOX_STEP_ENGINE_STEP_PORT_NO = 7;
        const byte GEARBOX_STEP_ENGINE_STEP_HIGH_LEVEL = 1;
        const byte GEARBOX_STEP_ENGINE_STEP_LOW_LEVEL = 0;

        const int GEARBOX_STEP_ENGINE_DIRECTION_PORT_NO = 6;
        const byte GEARBOX_STEP_ENGINE_DIRECTION_FRONT_LEVEL = 0;
        const byte GEARBOX_STEP_ENGINE_DIRECTION_REAR_LEVEL = 1;
        public void MoveGearbox(bool frontDirection, int highTimeInMs, int lowTimeInMs)
        {
            if (frontDirection)
            {
                setPortDO(GEARBOX_STEP_ENGINE_DIRECTION_PORT_NO, GEARBOX_STEP_ENGINE_DIRECTION_FRONT_LEVEL);
            }
            else
            {
                setPortDO(GEARBOX_STEP_ENGINE_DIRECTION_PORT_NO, GEARBOX_STEP_ENGINE_DIRECTION_REAR_LEVEL);
            }

            setPortDO(GEARBOX_STEP_ENGINE_STEP_PORT_NO, GEARBOX_STEP_ENGINE_STEP_HIGH_LEVEL);
            Thread.Sleep(highTimeInMs);
            setPortDO(GEARBOX_STEP_ENGINE_STEP_PORT_NO, GEARBOX_STEP_ENGINE_STEP_LOW_LEVEL);
            Thread.Sleep(lowTimeInMs);
        }

        /// <summary>
        /// sets power send to engine steering brake pedal
        /// </summary>
        /// <param name="strength">
        /// -100 = max pulling brake pedal
        /// +100 = max pushing brake pedal
        /// </param>
        public void SetBrake(double strength)
        {
            Logger.Log(this, String.Format("Brake steering value is being set to: {0}%", strength));

            if (strength < -100 || strength > 100)
            {
                Logger.Log(this, "strength is not in range", 2);
                throw new ArgumentException("strenght is not in range");
            }

            if (strength < 0) //backward
            {
                strength *= -1;
                setPortDO(BRAKE_DIRECTION_PORT_NO, BRAKE_BACKWARD_PORT_LEVEL);
            }
            else //forward
            {
                setPortDO(BRAKE_DIRECTION_PORT_NO, BRAKE_FORWARD_PORT_LEVEL);
            }

            //to dont waste engine - when steering value is low - we are stopping engine 
            if (strength < MIN_STRENGTH_FOR_BRAKE_TO_REACT_IN_PERCENTS) //strength should be always > 0 in here
            {
                setPortAO(BRAKE_STRENGTH_SET_PORT, BRAKE_MIN_SET_VALUE_IN_VOLTS);
                setPortDO(BRAKE_STOP_PORT_NO, BRAKE_STOP_ON_PORT_LEVEL);
            }
            else
            {
                Helpers.ReScaller.ReScale(ref strength, 0, 100, BRAKE_MIN_SET_VALUE_IN_VOLTS, BRAKE_MAX_SET_VALUE_IN_VOLST);

                setPortDO(BRAKE_STOP_PORT_NO, BRAKE_STOP_OFF_PORT_LEVEL);
                setPortAO(BRAKE_STRENGTH_SET_PORT, strength);
            }
        }

    }

}

