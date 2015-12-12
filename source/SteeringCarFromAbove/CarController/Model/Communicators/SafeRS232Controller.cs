using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Helpers;

namespace CarController.Model.Communicators
{
    /// <summary>
    /// class which controlls RS232 communication - it has 2 tasks:
    ///     1) receive wheels angle (by measuring steering wheel angle)
    ///     2) receive brakes state (by measuring brakes pedal angle)
    /// </summary>
    public class SafeRS232Controller : Device
    {
        public override string ToString()
        {
            return "RS232 controller";
        } 

        private SafeRS232Communicator RS232; 
        private RealCarCommunicator communicator;
        private Thread deviceQueringThread;

        private char[] giveMeSteeringWheelAngleMsg = new char[] { '1', 'P', (char)13 }; //TODO: try changing it to byte[] //not necessery, but char[] probably wont work for values > 127...
        private char[] giveMeBrakeAngleMsg = new char[] { '2', 'P', (char)13 };
        private char[] giveMeSteeringWheelDiagnosisMsg = new char[] { '1', 'D', (char)13 };
        private char[] giveMeBrakeDiagnosisMsg = new char[] { '2', 'D', (char)13 };

        private const int SLEEP_BETWEEN_2_READS_IN_MS = 10;

        private const int WHEEL_OUTPUT_WHEN_MAX_RIGHT = 15000; //increased by 100
        private const double WHEEL_ANGLE_ON_MAX_RIGHT = 30.0; //IMPORTANT: TODO: check it in documentation

        private const int WHEEL_OUTPUT_WHEN_MAX_LEFT = 2066; //decreased by 100
        private const double WHEEL_ANGLE_ON_MAX_LEFT = -30.0; //IMPORTANT: TODO: check it in documentation

        private const int BRAKE_OUTPUT_MAX_PUSHED = 11070; //was checked to be 11170 when max but decreased to improve steering
        private const double BRAKE_POWER_WHEN_MAX_PUSHED = 100;

        private const int BRAKE_OUTPUT_MAX_PULLED = 10740; //was checked to be 10729 when min but increased to improve steering
        private const double BRAKE_POWER_WHEN_MAX_PULLED = 0;

        /// <summary>
        /// setting this value will send converted info to car that new value is received
        /// </summary>
        private int SteeringWheelRead
        {
            get { return __steeringWheelRead__; }
            set
            {
                __steeringWheelRead__ = value;
                communicator.WheelAngleAcquired(ConvertReceivedSteeringWheelAngleToRealWheelAngle(value));
            }
        }
        private int __steeringWheelRead__;

        private double ConvertReceivedSteeringWheelAngleToRealWheelAngle(int value)
        {
            double val = Convert.ToDouble(value);
            if (Limiter.LimitAndReturnTrueIfLimitted(ref val, WHEEL_ANGLE_ON_MAX_LEFT, WHEEL_ANGLE_ON_MAX_RIGHT))
            {
                Logger.Log(this, String.Format(
                        "Steering wheel received value - {0} - is out of range [{1},{2}]",
                        val,
                        WHEEL_OUTPUT_WHEN_MAX_LEFT,
                        WHEEL_OUTPUT_WHEN_MAX_RIGHT),
                    1);
            }
            return ReScaller.ReScale(ref val, WHEEL_OUTPUT_WHEN_MAX_LEFT, WHEEL_OUTPUT_WHEN_MAX_RIGHT, WHEEL_ANGLE_ON_MAX_LEFT, WHEEL_ANGLE_ON_MAX_RIGHT);
        }

        /// <summary>
        /// setting this value will send converted info to car that new value is received
        /// </summary>
        private int BrakeRead
        {
            get { return __brakeRead__; }
            set
            {
                __brakeRead__ = value;
                communicator.BrakePositionsAcquired(ConvertReceivedBrakeAngleToRealBrakePosition(value));
            }
        }
        private int __brakeRead__;

        private double ConvertReceivedBrakeAngleToRealBrakePosition(int value)
        {
            double val = Convert.ToDouble(value);
            if (Limiter.LimitAndReturnTrueIfLimitted(ref val, BRAKE_OUTPUT_MAX_PULLED, BRAKE_OUTPUT_MAX_PUSHED))
            {
                Logger.Log(this, String.Format(
                        "brake received value - {0} - is out of range [{1},{2}]", 
                        val, 
                        BRAKE_OUTPUT_MAX_PULLED, 
                        BRAKE_OUTPUT_MAX_PUSHED), 
                    1);
            }
            return ReScaller.ReScale(ref val, BRAKE_OUTPUT_MAX_PULLED, BRAKE_OUTPUT_MAX_PUSHED, BRAKE_POWER_WHEN_MAX_PULLED, BRAKE_POWER_WHEN_MAX_PUSHED);
        }

        public SafeRS232Controller(RealCarCommunicator comm, SafeRS232Communicator safeRS232Communicator) //TODO: IT SHOULD NOT RELY ON RealCarCommunicator but on ICarCommunicator //its untestable now
        {
            communicator = comm;
            RS232 = safeRS232Communicator;
        }


        protected override void Initialize()
        {
            try
            {
                RS232.Initialize();
                DiagnoseBrakeSensors();
                DiagnoseSteeringWheelSensors();
            }
            catch (MaxTriesToConnectRS232ExceededException)
            {
                overallState = DeviceOverallState.Error;
            }
        }

        protected override void StartSensors()
        {
            deviceQueringThread = new Thread(new ThreadStart(StartTransmission));
            deviceQueringThread.Start();
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
            //no effectors in here - no danger - sensors still can work
        }

        private void DiagnoseSteeringWheelSensors()
        {
            List<int> readMsg = RS232.Query(giveMeSteeringWheelDiagnosisMsg);
            bool errorFound = false;

            if (readMsg.Count != 2)
            {
                Logger.Log(this, String.Format("wrong received message length: {0}", readMsg, 1));
            }
            else
            {
                if (readMsg[1] % 2 == 0)
                {
                    Logger.Log(this, "RS232 sterring wheel diagnosis bit 0 error", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 2) % 2 == 1)
                {
                    Logger.Log(this, "RS232 sterring wheel diagnosis bit 1 error", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 4) % 2 == 1)
                {
                    Logger.Log(this, "RS232 sterring wheel diagnosis bit 2 error - magnet is too strong or too close", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 8) % 2 == 1)
                {
                    Logger.Log(this, "RS232 sterring wheel diagnosis bit 3 error - magnet is too weak or too far", 1);
                    errorFound = true;
                }

                if (errorFound == false)
                {
                    Logger.Log(this, "RS232 steering wheel diagnosis - all test PASSED!");
                }

                Logger.Log(this, "RS232 steering wheel diagnosis done!");
            }
        }

        private void DiagnoseBrakeSensors()
        {
            List<int> readMsg = RS232.Query(giveMeBrakeDiagnosisMsg);
            bool errorFound = false;

            if (readMsg.Count != 2)
            {
                Logger.Log(this, String.Format("wrong received message length: {0}", readMsg, 1));
            }
            else
            {
                if (readMsg[1] % 2 == 0)
                {
                    Logger.Log(this, "RS232 brake diagnosis bit 0 error", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 2) % 2 == 1)
                {
                    Logger.Log(this, "RS232 brake diagnosis bit 1 error", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 4) % 2 == 1)
                {
                    Logger.Log(this, "RS232 brake diagnosis bit 2 error - magnet is too strong or too close", 1);
                    errorFound = true;
                }
                if ((readMsg[1] / 8) % 2 == 1)
                {
                    Logger.Log(this, "RS232 brake diagnosis bit 3 error - magnet is too weak or too far", 1);
                    errorFound = true;
                }

                if (errorFound == false)
                {
                    Logger.Log(this, "RS232 brake diagnosis - all test PASSED!");
                }

                Logger.Log(this, "RS232 brake diagnosis done!");
            }
        }

        private void StartTransmission()
        {
            while (true)
            {
                try
                {
                    ReadSteeringWheelSensor();
                    Thread.Sleep(SLEEP_BETWEEN_2_READS_IN_MS);

                    ReadBrakesSensors();
                    Thread.Sleep(SLEEP_BETWEEN_2_READS_IN_MS);
                }
                catch (MaxTriesToConnectRS232ExceededException) //TODO: to nie lapiue tego wyjatku
                {
                    overallState = DeviceOverallState.Error;
                    Logger.Log(this, "RS232 exceeded max tries to connect - fatal error", 4);
                }
                catch (Exception e)
                {
                    overallState = DeviceOverallState.Warrning;
                    Logger.Log(this, String.Format("RS232 exception message: {0}", e.Message), 3);
                    Logger.Log(this, String.Format("RS232 exception stack: {0}", e.StackTrace), 2);
                }
            }

        }

        private void ReadBrakesSensors()
        {
            List<int> readMsg = RS232.Query(giveMeBrakeAngleMsg);
            if (readMsg.Count == 3)
            {
                if (readMsg[0] == 'A')
                {
                    BrakeRead = readMsg[1] * 64 + readMsg[2];
                    Logger.Log(this, String.Format("Brake possition received from RS232: {0}", BrakeRead));
                    Logger.Log(this, String.Format("buff[0]: {0}, buff[1]: {1}, buff[2]: {2}", readMsg[0], readMsg[1], readMsg[2]));
                }
                else
                {
                    Logger.Log(this, String.Format("Brake - RS232 is desonchronised! Read is not done. Msg received: {0} {1} {2}", readMsg[0], readMsg[1], readMsg[2]), 1);
                }
            }
            else
            {
                if (readMsg[0] == 'E')
                {
                    Logger.Log(this, "RS232 received an error ('E' message) from brakes", 2);
                }
                else
                {
                    Logger.Log(this, String.Format("RS232, brakes, wrong received message length: {0}", readMsg.Count), 1);
                }
            }
        }

        private void ReadSteeringWheelSensor()
        {
            List<int> readMsg = RS232.Query(giveMeSteeringWheelAngleMsg);
            if (readMsg.Count == 3)
            {
                if (readMsg[0] == 'A')
                {
                    SteeringWheelRead = readMsg[1] * 64 + readMsg[2];
                    Logger.Log(this, String.Format("Steering wheel possition received from RS232: {0}", SteeringWheelRead));
                    Logger.Log(this, String.Format("buff[0]: {0}, buff[1]: {1}, buff[2]: {2}", readMsg[0], readMsg[1], readMsg[2]));
                }
                else
                {
                    Logger.Log(this, String.Format("Steering wheel - RS232 is desonchronised! Read is not done. Msg received: {0} {1} {2}", readMsg[0], readMsg[1], readMsg[2]), 1);
                }
            }
            else
            {
                if (readMsg[0] == 'E')
                {
                    Logger.Log(this, "RS232 received an error ('E' message) from steering wheel", 2);
                }
                else
                {
                    Logger.Log(this, String.Format("RS232, steering wheel, wrong received message length: {0}", readMsg.Count), 1);
                }
            }
        }

    }
}
