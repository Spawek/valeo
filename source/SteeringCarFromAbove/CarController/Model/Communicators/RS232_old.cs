using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Helpers;
using CarController;

namespace CarController_old
{
    public class RS232Controller : Device
    {
        // Create the serial port with basic settings 
        private SerialPort port = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One); //TODO: add choosing COM no from form

        //messages
        char[] giveMeSteeringWheelAngleMsg = new char[] { '1', 'P', (char)13 }; //TODO: try changing it to byte[] //not necessery, but char[] probably wont work for values > 127...
        char[] giveMeBrakeAngleMsg = new char[] { '2', 'P', (char)13 };
        char[] giveMeSteeringWheelDiagnosisMsg = new char[] { '1', 'D', (char)13 };
        char[] giveMeBrakeDiagnosisMsg = new char[] { '2', 'D', (char)13 };

        //consts
        private TimeSpan SLEEP_PER_READ_LOOP = new TimeSpan(0, 0, 0, 0, 10); //10ms
        private const int LOOPS_BETWEEN_DIAGNOSIS = 100; //mby go more
        private const int READ_TIMEOUT_IN_MS = 100;
        private const int WRITE_TIMEOUT_IN_MS = 100;
        private const int SLEEP_ON_RS232_DESYNC_IN_MS = 10;
        private const int IN_BUFFER_SIZE = 100;
        private const int SLEEP_WHILE_WAITING_FOR_READ_IN_MS = 2;
        private const int SLEEP_ON_FAILED_PORT_OPPENING_BEFORE_NEXT_TRY_AT_APP_INIT_IN_MS = 10; //to dont spam so many messages when it fails anyway
        private const int SLEEP_ON_FAILED_PORT_OPPENING_BEFORE_NEXT_TRY_AT_APP_WORKING_IN_MS = 0; //needed ASAP
        private const int MAX_CONNECTION_TRIES = 3000;
        private const int SLEEP_BETWEEN_2_READS_IN_MS = 10;

        private const bool DIAGNOSIS_ENABLED = false;

        private const int WHEEL_OUTPUT_WHEN_MAX_RIGHT = 15487;
        private const double WHEEL_ANGLE_ON_MAX_RIGHT = 30.0; //IMPORTANT: TODO: check it in documentation

        private const int WHEEL_OUTPUT_WHEN_MAX_LEFT = 1347;
        private const double WHEEL_ANGLE_ON_MAX_LEFT = -30.0; //IMPORTANT: TODO: check it in documentation

        //brake
        private const int BRAKE_OUTPUT_MAX_PUSHED = 13433; //was checked to be 11170 when max but decreased to improve steering
        private const double BRAKE_POWER_WHEN_MAX_PUSHED = 100;

        private const int BRAKE_OUTPUT_MAX_PULLED = 12817; //ze sprezynami jest 10709; //was checked to be 10729 when min but increased to improve steering
        private const double BRAKE_POWER_WHEN_MAX_PULLED = 0;

        //read values
        /// <summary>
        /// setting this value will send converted info to car that new value is received
        /// </summary>
        int SteeringWheelRead
        {
            get
            {
                return __steeringWheelRead__;
            }
            set
            {
                __steeringWheelRead__ = value;
                communicator.WheelAngleAcquired(ConvertReceivedSteeringWheelAngleToRealWheelAngle(value));
            }
        }
        int __steeringWheelRead__;

        private double ConvertReceivedSteeringWheelAngleToRealWheelAngle(int value)
        {
            double val = Convert.ToDouble(value);
            return ReScaller.ReScale(ref val, WHEEL_OUTPUT_WHEN_MAX_LEFT, WHEEL_OUTPUT_WHEN_MAX_RIGHT, WHEEL_ANGLE_ON_MAX_LEFT, WHEEL_ANGLE_ON_MAX_RIGHT);
        }

        /// <summary>
        /// setting this value will send converted info to car that new value is received
        /// </summary>
        int BrakeRead
        {
            get
            {
                return __brakeRead__;
            }

            set
            {
                __brakeRead__ = value;
                communicator.BrakePositionsAcquired(ConvertReceivedBrakeAngleToRealBrakePosition(value));
            }
        }
        int __brakeRead__;

        private double ConvertReceivedBrakeAngleToRealBrakePosition(int value)
        {
            double val = Convert.ToDouble(value);
            if (Limiter.LimitAndReturnTrueIfLimitted(ref val, BRAKE_OUTPUT_MAX_PULLED, BRAKE_OUTPUT_MAX_PUSHED))
            {
                Logger.Log(this, "brake received value is out of range", 1);
            }
            return ReScaller.ReScale(ref val, BRAKE_OUTPUT_MAX_PULLED, BRAKE_OUTPUT_MAX_PUSHED, BRAKE_POWER_WHEN_MAX_PULLED, BRAKE_POWER_WHEN_MAX_PUSHED);
        }


        //transmission thread
        System.Threading.Thread transsmissionThread;

        RealCarCommunicator communicator;

        public RS232Controller(RealCarCommunicator comm)
        {
            communicator = comm;
        }


        protected override void Initialize()
        {
            // Attach a method to be called when there
            // is data waiting in the port's buffer
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.ReadTimeout = READ_TIMEOUT_IN_MS;
            port.WriteTimeout = WRITE_TIMEOUT_IN_MS;
            port.Encoding = Encoding.UTF8; //it probably fixes bug with "i cant receive/send values bigger than 127 - i get 63 instead of it"

            //thread start
            transsmissionThread = new System.Threading.Thread(new System.Threading.ThreadStart(startDataTransmission));
        }

        /// <summary>
        /// note: must work as a new thread
        /// Datasheet in repo: "autonomiczny_samochod\docs\Dane techniczne - czujnik k¹ta.pdf"
        /// in case of any problems contact "Korad Zawada" and Electronics group
        /// </summary>
        private void startDataTransmission()
        {
            TryOppeningPortUntilItSucceds(SLEEP_ON_FAILED_PORT_OPPENING_BEFORE_NEXT_TRY_AT_APP_INIT_IN_MS);

            int loopsFromLastDiagnosis = LOOPS_BETWEEN_DIAGNOSIS; //so it will make diagnose at start

            while (true)
            {
                try
                {
                    if (loopsFromLastDiagnosis++ < LOOPS_BETWEEN_DIAGNOSIS)
                    {
                        SensorsRead();
                    }
                    else
                    {
                        if (DIAGNOSIS_ENABLED)
                        {
                            DiagnoseSensors();
                        }
                        loopsFromLastDiagnosis = 0;
                    }
                }
                catch (TimeoutException)
                {
                    Logger.Log(this, "RS232 timout has occured", 1);
                    //TODO: some timout handling???
                }
                catch (Exception e)
                {
                    Logger.Log(this, String.Format("RS232 exception has occured: {0}", e.Message), 2);
                    Logger.Log(this, String.Format("RS232 exception stack trace: {0}", e.StackTrace), 1);
                }
            }
        }

        private void SensorsRead()
        {
            try
            {
                if (!port.IsOpen)
                {
                    Logger.Log(this, "RS232 port is closed - trying to reinitialize");
                    TryOppeningPortUntilItSucceds(SLEEP_ON_FAILED_PORT_OPPENING_BEFORE_NEXT_TRY_AT_APP_WORKING_IN_MS);
                }

                ReadSteeringWheelSensor(); //there 2 methods should remain 2 methods, because they can be customed in some way in future
                Thread.Sleep(SLEEP_BETWEEN_2_READS_IN_MS);
                ReadBrakesSensors();
                Thread.Sleep(SLEEP_BETWEEN_2_READS_IN_MS);
            }
            catch (Exception e)
            {
                Logger.Log(this, String.Format("RS232 communication error: \nMsg:{0} \nStackTrace:{1}", e.Message, e.StackTrace), 2);
                //TODO: errors handling
            }
        }

        private void ReadBrakesSensors()
        {
            if (!safeWriteToRS232(giveMeBrakeAngleMsg, 0, giveMeBrakeAngleMsg.Length))
            {
                List<int> readMsg = readWordFromRS232();
                if (readMsg.Count != 3)
                {
                    Logger.Log(this, String.Format("wrong received message length: {0}", readMsg.Count), 1);
                }
                else
                {
                    if (readMsg[0] == 'A')
                    {
                        BrakeRead = readMsg[1] * 64 + readMsg[2];
                        Logger.Log(this, String.Format("Brake possition received from RS232: {0}", BrakeRead));
                        Logger.Log(this, String.Format("buff[0]: {0}, buff[1]: {1}, buff[2]: {2}", readMsg[0], readMsg[1], readMsg[2]));
                    }
                    else
                    {
                        if (readMsg[0] == 'E') //check that condition
                        {
                            Logger.Log(this, "RS232 received an errror from brakes", 2);
                            //TODO: some handling?
                        }

                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        Logger.Log(this, String.Format("BRAKE - RS232 is desonchronised! Read is not done. Msg received: {0} {1} {2}", readMsg[0], readMsg[1], readMsg[2]), 1);
                    }
                }
                System.Threading.Thread.Sleep(SLEEP_PER_READ_LOOP);
            }
        }

        private void ReadSteeringWheelSensor()
        {
            if (!safeWriteToRS232(giveMeSteeringWheelAngleMsg, 0, giveMeSteeringWheelAngleMsg.Length))
            {
                List<int> readMsg = readWordFromRS232();
                if (readMsg.Count != 3)
                {
                    Logger.Log(this, String.Format("wrong received message length: {0}", readMsg.Count), 1);
                }
                else
                {
                    if (readMsg[0] == 'A')
                    {
                        SteeringWheelRead = readMsg[1] * 64 + readMsg[2];
                        Logger.Log(this, String.Format("Steering wheel possition received from RS232: {0}", SteeringWheelRead));
                        Logger.Log(this, String.Format("buff[0]: {0}, buff[1]: {1}, buff[2]: {2}", readMsg[0], readMsg[1], readMsg[2]));
                    }
                    else
                    {
                        if (readMsg[0] == 'E') //check that condition
                        {
                            Logger.Log(this, "RS232 received an errror from steering wheel", 2);
                            //TODO: some handling?
                        }

                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        Logger.Log(this, String.Format("STEERING WHEEL - RS232 is desonchronised! Read is not done. Msg received: {0} {1} {2}", readMsg[0], readMsg[1], readMsg[2]), 1);
                        System.Threading.Thread.Sleep(SLEEP_ON_RS232_DESYNC_IN_MS);
                    }
                }
            }
        }

        private void DiagnoseSensors()
        {
            try
            {
                DiagnoseSteeringWheel();
                DiagnoseBrakes();
            }
            catch (Exception e)
            {
                Logger.Log(this, String.Format("RS232 communication error: \nMsg:{0} \nStackTrace:{1}", e.Message, e.StackTrace), 2);
                //TODO: errors handling
            }
        }

        private void DiagnoseSteeringWheel()
        {
            if (!safeWriteToRS232(giveMeSteeringWheelDiagnosisMsg, 0, giveMeSteeringWheelDiagnosisMsg.Length))
            {
                List<int> readMsg = readWordFromRS232();
                if (readMsg.Count != 2)
                {
                    Logger.Log(this, String.Format("wrong received message length: {0}", readMsg, 1));
                }
                else
                {
                    if (readMsg[1] % 2 == 0)
                    {
                        Logger.Log(this, "RS232 sterring wheel diagnosis bit 0 error", 1);
                    }
                    if ((readMsg[1] / 2) % 2 == 1)
                    {
                        Logger.Log(this, "RS232 sterring wheel diagnosis bit 1 error", 1);
                    }
                    if ((readMsg[1] / 4) % 2 == 1)
                    {
                        Logger.Log(this, "RS232 sterring wheel diagnosis bit 2 error - magnet is too strong or too close", 1);
                    }
                    if ((readMsg[1] / 8) % 2 == 1)
                    {
                        Logger.Log(this, "RS232 sterring wheel diagnosis bit 3 error - magnet is too weak or too far", 1);
                    }
                }
                Logger.Log(this, "RS232 Diagnosis done!");
            }
        }

        private void DiagnoseBrakes()
        {
            if (!safeWriteToRS232(giveMeBrakeDiagnosisMsg, 0, giveMeBrakeDiagnosisMsg.Length))
            {
                List<int> readMsg = readWordFromRS232();
                if (readMsg.Count != 4)
                {
                    Logger.Log(this, String.Format("wrong received message length: {0}", readMsg, 1));
                }
                else
                {
                    if (readMsg[0] == 0)
                    {
                        Logger.Log(this, "RS232 brake diagnosis bit 0 error", 1);
                    }
                    if (readMsg[1] == 1)
                    {
                        Logger.Log(this, "RS232 brake diagnosis bit 1 error", 1);
                    }
                    if (readMsg[2] == 1)
                    {
                        Logger.Log(this, "RS232 brake diagnosis bit 2 error - magnet is too strong or too close", 1);
                    }
                    if (readMsg[3] == 1)
                    {
                        Logger.Log(this, "RS232 brake diagnosis bit 3 error - magnet is too weak or too far", 1);
                    }
                }
            }
        }

        /// <summary>
        /// tries to open RS232 port, retries after "waitBeforeNextTryInMs" if failed
        /// </summary>
        /// <param name="waitBeforeNextTryInMs"></param>
        private void TryOppeningPortUntilItSucceds(int waitBeforeNextTryInMs)
        {
            bool done = false;
            int tries = 0;

            while (tries++ < MAX_CONNECTION_TRIES && done == false)
            {
                try
                {
                    port.Open();
                    done = true;
                }
                catch (Exception)
                {
                    Logger.Log(this, String.Format("RS232 port oppening failed, waiting {0}ms before next try", waitBeforeNextTryInMs), 2);
                    Thread.Sleep(waitBeforeNextTryInMs);
                }
            }

            if (done)
            {
                overallState = DeviceOverallState.OK;
            }
            else
            {
                overallState = DeviceOverallState.Error;
            }
        }


        //TODO: REMOVE BELOW CODE AFTER TESTING THAT CURRENT CODE WORKS
        /* it has been rewriten below due to reciving values > 127 problems //and that it was piece shit written in garage

       private volatile string inBuffer = string.Empty;
       private volatile string receivedWord = string.Empty;

       /// <summary>
       /// its NOT THREAD-SAFE - cant work on different threads
       /// uses volatile vars "inBuffer" and "receivedWord"
       /// </summary>
       /// <returns></returns>
       private string readWordFromRS232()
       {
           while (receivedWord == string.Empty)
               System.Threading.Thread.Sleep(SLEEP_WHILE_WAITING_FOR_READ_IN_MS);

           string temp = receivedWord;
           receivedWord = string.Empty;

           return temp;
       }

        
       //http://stackoverflow.com/questions/5848907/received-byte-never-over-127-in-serial-port <--- use just read();
       private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
       {
           string temp = port.ReadExisting(); //<-------- to nie dziala -> powyzej 128 daje 63
           //int temp = port.ReadChar();
           Console.WriteLine(temp);
           //inBuffer = inBuffer + Convert.ToChar(temp);
           inBuffer = inBuffer + temp;
           if (inBuffer != string.Empty)
           {
               if (inBuffer[inBuffer.Length - 1] == 13)
               //if(temp == 13)
               {
                   if (receivedWord != string.Empty)
                   {
                       Logger.Log(this, String.Format("message from RS232 was not read: {0}", receivedWord), 1);
                   }
                   receivedWord = inBuffer;
                   inBuffer = string.Empty;
               }
           }
       }
       */


        /// <summary>
        /// this is rewriten function from above
        /// IMPORTANT: TODO: TEST IT!!!!!
        /// </summary>
        volatile LinkedList<int> RS232SignalsInputList = new LinkedList<int>();
        volatile bool RS232dataIsBeingRead = false; //to dont allow data reading start twice

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            /* 
             * due to: http://msdn.microsoft.com/en-us/library/y2sxhat8.aspx 
             * changing encoding to "UTF8Encoding" fixes bug with getting "63" instead of values bigger than 127
             * 
             * due to StackOverflow:
             * only Read() should be used (returns byte, not char - chars bugs the code!)
             */
            if (!RS232dataIsBeingRead)
            { //TODO: think about doing it in new thread
                RS232dataIsBeingRead = true;
                int byteRead;
                try
                {
                    while ((byteRead = port.ReadByte()) != -1) //-1 is when there is end of stream
                    {
                        RS232SignalsInputList.AddLast(byteRead);
                    }
                    RS232dataIsBeingRead = false;
                }
                catch (System.TimeoutException)
                {
                    RS232SignalsInputList.Clear(); //clearing list on timeout should prevent desync
                    Logger.Log(this, "RS232 timeout has occured", 2);
                }
                catch (Exception)
                {
                    // Logger.Log(this, String.Format("RS232 error: {0}, {1}", e.Message, e.StackTrace)); //TODO: //IMPORTANT: tempporary
                }
                finally
                {
                    RS232dataIsBeingRead = false;
                }
            }
            //else just end 
        }

        /// <summary>
        /// returns read word (every word ends with "13") - it cuts "13" and dont return that
        /// </summary>
        /// <returns></returns>
        private List<int> readWordFromRS232()
        {
            LinkedListNode<int> node;

            int timeoutCounter = 0;
            while ((node = RS232SignalsInputList.Find(13)) == null)
            {
                if (timeoutCounter++ > READ_TIMEOUT_IN_MS / SLEEP_WHILE_WAITING_FOR_READ_IN_MS)
                {
                    RS232SignalsInputList.Clear(); //clearing list on timeout should prevent desync
                    throw new TimeoutException("RS232 read custom timeout has occured");
                }

                Thread.Sleep(SLEEP_WHILE_WAITING_FOR_READ_IN_MS);
            }

            if (node == RS232SignalsInputList.First) //if 1st == 13 == \n 
            {
                RS232SignalsInputList.RemoveFirst(); //remove it
                return readWordFromRS232(); //and try again
            }

            //if some "\n" == 13 == end of line was found cut and send left part of list from 1st node with 13
            List<int> output = new List<int>();

            int temp;
            while ((temp = RS232SignalsInputList.First.Value) != 13)
            {
                output.Add(temp);
                RS232SignalsInputList.RemoveFirst();
            }
            RS232SignalsInputList.RemoveFirst(); //remove also \n == 13 value

            return output;
        }

        /// <summary>
        /// line standard write, but catches exceptions from inside
        /// returns:
        ///     false if OK
        ///     true if ERROR
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private bool safeWriteToRS232(char[] buffer, int offset, int count)
        {
            try
            {
                port.Write(buffer, offset, count);
                return false;
            }
            catch (TimeoutException)
            {
                RS232SignalsInputList.Clear(); //clearing list on timeout should prevent desync
                Logger.Log(this, "timeout on write has occured, message was not sent", 1); //TODO: real exceptions handling
            }
            catch (Exception)
            {
                Logger.Log(this, "some error has occured on writing msg to RS232", 1);
            }

            return true;
        }


        protected override void StartSensors()
        {
            transsmissionThread.Start();
        }

        protected override void StartEffectors()
        {
            //do nothing
        }

        protected override void PauseEffectors()
        {
            //do nothing
        }

        protected override void EmergencyStop()
        {
            //do nothing 
        }
    }
}
