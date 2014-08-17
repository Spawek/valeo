using CarController;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace car_communicator
{
    public class GearboxController : Helpers.Device
    {
        private const int GEARBOX_INPUT_START_BIT_NO = 0;
        private const int GEARBOX_INPUT_BIT_COUNT = 5;
        private const bool GEARBOX_INPUT_ACTIVE_VALUE = false;

        private const int GEARBOX_UPDATING_INTERVAL_IN_MS = 50;
        private const int GEARBOX_POSITION_SETTER_SLEEP_IN_MS = 100;

        private const int GEARBOX_HIGH_STEP_IN_MS = 4;
        private const int GEARBOX_LOW_STEP_IN_MS = 4;

        public GearboxController(USB4702 _extentionCardCommunicator, RealCarCommunicator comunicator)
        {
            extentionCardCommunicator = _extentionCardCommunicator;
            realCarCommunicator = comunicator;
        }
    
        protected override void Initialize()
        {
            gearboxPositionUpdater = new Thread(new ThreadStart(GearboxPositionUpdating));
            gearboxPositionSetter = new Thread(new ThreadStart(GearboxPositionSetting));
        }

        private void GearboxPositionSetting()
        {
            while (true)
            {
                if (lastSeenGear == targetGear)
                {
                    Thread.Sleep(GEARBOX_POSITION_SETTER_SLEEP_IN_MS);
                }
                else
                {
                    if (ConvertGearToRawPosition(lastSeenGear) < ConvertGearToRawPosition(targetGear))
                    {
                        extentionCardCommunicator.MoveGearbox(true, GEARBOX_HIGH_STEP_IN_MS, GEARBOX_LOW_STEP_IN_MS);
                    }
                    else
                    {
                        extentionCardCommunicator.MoveGearbox(false, GEARBOX_HIGH_STEP_IN_MS, GEARBOX_LOW_STEP_IN_MS);
                    }
                }
            }
        }

        private void GearboxPositionUpdating()
        {
            while (true)
            {
                List<bool> rawDigitalInput = extentionCardCommunicator.ReadDigitalInputs();
                List<bool> gearboxInput = rawDigitalInput.GetRange(GEARBOX_INPUT_START_BIT_NO, GEARBOX_INPUT_BIT_COUNT);

                if (gearboxInput.Count(x => x == GEARBOX_INPUT_ACTIVE_VALUE) > 1)
                {
                    Logger.Log(this, String.Format("Wrong number of active inputs from gearbox!"), 3);
                }
                else if (gearboxInput.Count(x => x == GEARBOX_INPUT_ACTIVE_VALUE) == 0)
                {
                    Logger.Log(this, String.Format("No data from gearbox reveived"));
                }
                else
                {
                    int activeGear = gearboxInput.FindIndex(x => x == GEARBOX_INPUT_ACTIVE_VALUE);
                    Gear newFoundGear = ConvertRawPositionToGear(activeGear); 
                    if (lastSeenGear != newFoundGear)
                    {
                        lastSeenGear = newFoundGear;

                        try
                        {
                            //realCarCommunicator.GearboxPositionAcquired(lastSeenGear);
                        }
                        catch (Exception e)
                        {
                            Logger.Log(this, String.Format("Exception occured on new gear acquiring: {1}", e.Message), 3);
                        }
                    }
                    
                }

                Thread.Sleep(GEARBOX_UPDATING_INTERVAL_IN_MS);
            }
        }


        protected override void StartSensors()
        {
            gearboxPositionUpdater.Start();
        }

        protected override void StartEffectors()
        {
            gearboxPositionSetter.Start();
            SetGear(Gear.neutral);
        }

        protected override void PauseEffectors()
        {
            gearboxPositionSetter.Suspend();
        }

        protected override void EmergencyStop()
        {
            gearboxPositionSetter.Suspend();
        }

        public void SetGear(Gear gear)
        {
            targetGear = gear;
        }

        private int ConvertGearToRawPosition(Gear gear)
        {
            switch (gear)
            {
                case Gear.parking:
                    return 0;
                    break;

                case Gear.reverse:
                    return 1;
                    break;

                case Gear.neutral:
                    return 2;
                    break;

                case Gear.drive:
                    return 3;
                    break;

                case Gear.two:
                    return 4;
                    break;

                default:
                    throw new ApplicationException("Unhandled gear!");
                    break;
            }
        }

        private Gear ConvertRawPositionToGear(int rawPosition)
        {
            switch (rawPosition)
            {
                case 0:
                    return Gear.parking;
                    break;

                case 1:
                    return Gear.reverse;
                    break;

                case 2:
                    return Gear.neutral;
                    break;

                case 3:
                    return Gear.drive;
                    break;

                case 4:
                    return Gear.two;
                    break;

                default:
                    throw new ApplicationException("Unhandled gear!");
                    break;
            }
        }

        private USB4702 extentionCardCommunicator;
        RealCarCommunicator realCarCommunicator;
        private Thread gearboxPositionUpdater;
        private Thread gearboxPositionSetter;
        private Gear lastSeenGear;
        private Gear targetGear;
    }
}
