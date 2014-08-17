using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Helpers;

namespace CarController
{
    public enum Gear
    {
        parking,
        reverse,
        neutral,
        drive,
        two
    }
    //evSpeedInfoReceived
    public delegate void SpeedInfoReceivedEventHander(object sender, SpeedInfoReceivedEventArgs args);
    public class SpeedInfoReceivedEventArgs : EventArgs
    {
        private double speedInfo;

        public SpeedInfoReceivedEventArgs(double speed)
        {
            speedInfo = speed;
        }

        public double GetSpeedInfo()
        {
            return speedInfo;
        }
    }

    //evSteeringWheelAngleInfoReceived
    public delegate void SteeringWheelAngleInfoReceivedEventHandler(object sender, SteeringWheelAngleInfoReceivedEventArgs args);
    public class SteeringWheelAngleInfoReceivedEventArgs : EventArgs
    {
        private double wheelAngleInfo;

        public SteeringWheelAngleInfoReceivedEventArgs(double angle)
        {
            wheelAngleInfo = angle;
        }

        public double GetAngle()
        {
            return wheelAngleInfo;
        }
    }
    
    //evBrakePositionInfoReceived
    public delegate void BrakePositionReceivedEventHandler(object sender, BrakePositionReceivedEventArgs args);
    public class BrakePositionReceivedEventArgs : EventArgs
    {
        //from 0 to 100[%];
        private double brakePosition;

        public BrakePositionReceivedEventArgs(double position)
        {
            brakePosition = position;

            if (position < 0 || position > 100)
            {
                Logger.Log(this, "received brake position is not in range [0, 100]", 2);
            }
        }

        public double GetPosition()
        {
            return brakePosition;
        }
    }
    
    public delegate void GearPositionReceivedEventHandler(object sender, GearPositionReceivedEventArgs args);
    public class GearPositionReceivedEventArgs : EventArgs
    {
        private Gear gear;

        public GearPositionReceivedEventArgs(Gear _gear)
        {
            gear = _gear;
        }

        public Gear GetGear()
        {
            return gear;
        }
    }

    public interface ICarCommunicator
    {
        event SpeedInfoReceivedEventHander evSpeedInfoReceived;
        event SteeringWheelAngleInfoReceivedEventHandler evSteeringWheelAngleInfoReceived;
        event BrakePositionReceivedEventHandler evBrakePositionReceived;
        event GearPositionReceivedEventHandler evGearPositionReceived;

        ICar ICar
        {
            get;
        }

        void InitRegulatorsEventsHandling();

        void SetGear(Gear gear);
    }
}
