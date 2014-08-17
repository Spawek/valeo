using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Helpers
{
    public class DeviceManager
    {
        private static DeviceManager __GLOBAL_DEVICE_MANAGER__ = null;
        public static DeviceManager GlobalDeviceManager
        {
            get
            {
                if (__GLOBAL_DEVICE_MANAGER__ == null)
                {
                    __GLOBAL_DEVICE_MANAGER__ = new DeviceManager();
                }
                return __GLOBAL_DEVICE_MANAGER__;
            }
            private set { __GLOBAL_DEVICE_MANAGER__ = value; }
        }

        /// <summary>
        /// public is needed by form - DO NOT USE //TODO: try to do something with it
        /// </summary>
        public List<Device> devicesList = new List<Device>();


        public event DeviceStateHasChangedEventHandler evDeviceManagerOverallStateHasChanged;

        /// <summary>
        /// changing this state will also invoke event 'evDeviceStateHasChanged'
        /// 
        /// default value is DeviceState.OK
        /// </summary>
        public DeviceOverallState overallState
        {
            get { return __OVERALLSTATE__; }
            private set
            {
                if (__OVERALLSTATE__ != value)
                {
                    __OVERALLSTATE__ = value;
                    DeviceStateHasChangedEventHandler temp = evDeviceManagerOverallStateHasChanged;
                    if (temp != null)
                    {
                        temp(this, new DeviceStateHasChangedEventArgs(__OVERALLSTATE__));
                    }
                }
            }
        }
        private DeviceOverallState __OVERALLSTATE__ = DeviceOverallState.OK;

        private DeviceManagerForm deviceManagerForm;
        private Thread formThread;
        private Thread actionThread = null; //e.g. starting sensors or pausing effectors
        public string currentActionName = "Idle";

        public DeviceManager(bool doYouWantDeviceManagerWindow = true)
        {
            if (doYouWantDeviceManagerWindow)
            {
                formThread = new Thread(new ThreadStart(CreateForm));
                formThread.Start();
            }
        }

        private void CreateForm()
        {
            deviceManagerForm = new DeviceManagerForm(this);
            deviceManagerForm.Activate();
            deviceManagerForm.Show();

            System.Windows.Forms.Application.Run();
        }

        public void RegisterDevice(Device dev)
        {
            devicesList.Add(dev);
            dev.evDeviceStateHasChanged += dev_evDeviceStateHasChanged;
            Logger.Log(this, String.Format("Device has been registred: {0}", dev.ToString()), 1);
        }

        private void dev_evDeviceStateHasChanged(object sender, DeviceStateHasChangedEventArgs args)
        {
            Logger.Log(this, 
                String.Format(
                    "device {0} state has changed to: {1}", 
                    sender.ToString(), 
                    args.ToString()), 
                1);

            switch (args.GetDeviceState())
            {
                case DeviceOverallState.Error:
                    overallState = DeviceOverallState.Error;
                    EmergencyStop();
                    break;

                case DeviceOverallState.Warrning:
                    if (overallState == DeviceOverallState.OK)
                    {
                        PauseEffectors();
                        overallState = DeviceOverallState.Warrning;
                    }
                    break;

                case DeviceOverallState.OK:
                    int devsInWarrningState = 0;
                    int devsInErrorState = 0;
                    foreach (Device dev in devicesList)
                    {
                        if (dev.overallState == DeviceOverallState.Warrning) devsInWarrningState++;
                        else if (dev.overallState == DeviceOverallState.Error) devsInErrorState++;

                        if (devsInErrorState > 0) overallState = DeviceOverallState.Error;
                        else if (devsInWarrningState > 0) overallState = DeviceOverallState.Warrning;
                        else overallState = DeviceOverallState.OK;
                    }
                    break;

                default:
                    throw new ApplicationException("Unknown device state");
            }

        }

        public void Initialize()
        {
            if (actionThread == null)
            {
                currentActionName = "Initialization";
                actionThread = new Thread(new ThreadStart(ParallelInitialization));
                actionThread.Start();
            }
            else
            {
                Logger.Log(this, "action couldn't be started, because other action was executing!", 3);
            }
        }

        private void ParallelInitialization()
        {
            Parallel.ForEach(devicesList, dev =>
            {
                Logger.Log(this, String.Format("Device initialization: {0}", dev.ToString()), 1);
                dev.InitializeWithPreAndPostWork();
            });
            currentActionName = "Idle";
            actionThread = null;
        }

        public void StartSensors()
        {
            if (actionThread == null)
            {
                currentActionName = "Starting Sensors";
                actionThread = new Thread(new ThreadStart(ParallelStartingSensors));
                actionThread.Start();
            }
            else
            {
                Logger.Log(this, "action couldn't be started, because other action was executing!", 3);
            }

        }

        private void ParallelStartingSensors()
        {
            Parallel.ForEach(devicesList, dev =>
            {
                Logger.Log(this, String.Format("Device sensors starting: {0}", dev.ToString()), 1);
                dev.StartSensorsWithPreAndPostWork();
            });
            currentActionName = "Idle";
            actionThread = null;
        }

        public void StartEffectors()
        {
            if (actionThread == null)
            {
                currentActionName = "Starting Effectors";
                actionThread = new Thread(new ThreadStart(ParallelStartingEffectors));
                actionThread.Start();
            }
            else
            {
                Logger.Log(this, "action couldn't be started, because other action was executing!", 3);
            }
        }

        private void ParallelStartingEffectors()
        {
            Parallel.ForEach(devicesList, dev =>
            {
                Logger.Log(this, String.Format("Device effectors starting: {0}", dev.ToString()), 1);
                dev.StartEffectorsWithPreAndPostWork();
            });
            currentActionName = "Idle";
            actionThread = null;
        }

        public void PauseEffectors()
        {
            if (actionThread == null)
            {
                currentActionName = "Pausing Effectors";
                actionThread = new Thread(new ThreadStart(ParallelPausingEffectors));
                actionThread.Start();
            }
            else
            {
                Logger.Log(this, "action couldn't be started, because other action was executing!", 3);
            }
        }

        private void ParallelPausingEffectors()
        {
            Parallel.ForEach(devicesList, dev =>
            {
                Logger.Log(this, String.Format("Device effectors pausing: {0}", dev.ToString()), 2);
                dev.PauseEffectorsWithPreAndPostWork();
            });
            currentActionName = "Idle";
            actionThread = null;
        }


        public void EmergencyStop()
        {
            if (actionThread == null)
            {
                currentActionName = "Emergency Stopping!";
                actionThread = new Thread(new ThreadStart(ParallelEmergencyStop));
                actionThread.Start();
            }
            else
            {
                Logger.Log(this, "action couldn't be started, because other action was executing!", 3);
            }
        }

        private void ParallelEmergencyStop()
        {
            Parallel.ForEach(devicesList, dev =>
            {
                Logger.Log(this, String.Format("Device emergency stop: {0}", dev.ToString()), 2);
                dev.EmergencyStopWithPreAndPostWork();
            });
            currentActionName = "Idle";
            actionThread = null;
        }
    }
}
