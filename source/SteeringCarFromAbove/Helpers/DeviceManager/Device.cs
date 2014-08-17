using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/*
 * State flow (INNER ARROW ACTIONS ARE ALSO STATES!)
 * 
 *  Not Initialized         
 *        |
 *   Initializing           
 *        |
 *        \/                
 *    Initialized           
 *        |
 *     StartingSensors      
 *        |
 *        \/
 *    SensorsStarted        
 *        |
 
 *        |
 *        \/                EmergencyStopping
 *    EffectorsStarted      ----------------->
 *      |          /\
 *      |     StartingEffectors                 EmergencyStop
 * PausingEffecors  |
 *      \/          |
 *    EffectorsPaused       ----------------->
 *                          EmergencyStopping
 *    
 * 
 *  OverallState flow (triangle ;))
 *  
 *           OK
 *         /\   /\
 *         /     \  
 *        /       \
 *      \/        \/
 *      Error<--->Warning
 */

namespace Helpers
{
    public delegate void DeviceStateHasChangedEventHandler(object sender, DeviceStateHasChangedEventArgs args);
    public class DeviceStateHasChangedEventArgs : EventArgs
    {
        private DeviceOverallState deviceState;

        public DeviceStateHasChangedEventArgs(DeviceOverallState state)
        {
            deviceState = state;
        }

        public DeviceOverallState GetDeviceState()
        {
            return deviceState;
        }
    }

    public enum DeviceOverallState
    {
        OK, Warrning, Error
    }

    public enum DeviceInitializationState
    {
        NotInitialized,
        Initializing,
        Initialized,
        StartingSensors,
        SensorsStarted,
        StartingEffectors,
        EffectorsStarted,
        PausingEffectors,
        EffectorsPaused,
        EmergencyStopping,
        EmergencyStopped
    }

    public abstract class Device
    {
        public event DeviceStateHasChangedEventHandler evDeviceStateHasChanged;

        /// <summary>
        /// changing this state will also invoke event 'evDeviceStateHasChanged'
        /// (and do some stuff in DeviceManager which will be subscribed for this event)
        /// 
        /// default value is DeviceState.OK
        /// </summary>
        public DeviceOverallState overallState
        {
            get { return __STATE__; }
            protected set
            {
                if (__STATE__ != value)
                {
                    __STATE__ = value;
                    DeviceStateHasChangedEventHandler temp = evDeviceStateHasChanged;
                    if (temp != null)
                    {
                        temp(this, new DeviceStateHasChangedEventArgs(__STATE__));
                    }
                }
            }
        }
        private DeviceOverallState __STATE__ = DeviceOverallState.OK;

        public DeviceInitializationState initializationState = DeviceInitializationState.NotInitialized;

        public void InitializeWithPreAndPostWork()
        {
            InitializePreWork();
            Initialize();
            InitializePostWork();
        }
        protected abstract void Initialize();
        private void InitializePreWork()
        {
            if (initializationState == DeviceInitializationState.NotInitialized)
            {
                Logger.Log(this, "Initialization started", 1);
            }
            else
            {
#if DEBUG
                //throw new ApplicationException("Invalid state transition");
#endif
            }

            initializationState = DeviceInitializationState.Initializing;
        }
        private void InitializePostWork()
        {
            Logger.Log(this, "Initialization completed", 1);
            initializationState = DeviceInitializationState.Initialized;
        }

        public void StartSensorsWithPreAndPostWork()
        {
            StartSensorsPreWork();
            StartSensors();
            StartSensorsPostWork();
        }
        protected abstract void StartSensors();
        private void StartSensorsPreWork()
        {
            if (initializationState == DeviceInitializationState.Initialized)
            {
                Logger.Log(this, "Starting sensors started", 1);
            }
            else
            {
#if DEBUG
                //throw new ApplicationException("Invalid state transition");
#endif
            }
            initializationState = DeviceInitializationState.StartingSensors;
        }
        private void StartSensorsPostWork()
        {
            Logger.Log(this, "Starting sensors completed", 1);
            initializationState = DeviceInitializationState.SensorsStarted;
        }

        public void StartEffectorsWithPreAndPostWork()
        {
            StartEffectorsPreWork();
            StartEffectors();
            StartEffectorsPostWork();
        }
        protected abstract void StartEffectors();
        private void StartEffectorsPreWork()
        {
            if (initializationState == DeviceInitializationState.SensorsStarted || initializationState == DeviceInitializationState.EffectorsPaused)
            {
                Logger.Log(this, "Starting effectors started", 1);
            }
            else
            {
#if DEBUG
                //throw new ApplicationException("Invalid state transition");
#endif
            }
            initializationState = DeviceInitializationState.StartingEffectors;
        }
        private void StartEffectorsPostWork()
        {
            Logger.Log(this, "Starting effectors completed", 1);
            initializationState = DeviceInitializationState.EffectorsStarted;
        }

        public void PauseEffectorsWithPreAndPostWork()
        {
            PauseEffectorsPreWork();
            PauseEffectors();
            PauseEffectorsPostWork();
        }
        protected abstract void PauseEffectors();
        private void PauseEffectorsPreWork()
        {
            Logger.Log(this, "Pausing effectors started", 1);
            initializationState = DeviceInitializationState.PausingEffectors;
        }
        private void PauseEffectorsPostWork()
        {
            Logger.Log(this, "Pausing effectors completed", 1);
            initializationState = DeviceInitializationState.EffectorsPaused;
        }

        public void EmergencyStopWithPreAndPostWork()
        {
            EmergencyStopPreWork();
            EmergencyStop();
            EmergencyStopPostWork();
        }
        protected abstract void EmergencyStop();
        private void EmergencyStopPreWork()
        {
            //no state checking here - its emergency!
            Logger.Log(this, "Emergency stopping started", 1);
            initializationState = DeviceInitializationState.EmergencyStopping;
        }
        private void EmergencyStopPostWork()
        {
            Logger.Log(this, "Emergency stopping completed", 1);
            initializationState = DeviceInitializationState.EmergencyStopped;
        }
    }
}
