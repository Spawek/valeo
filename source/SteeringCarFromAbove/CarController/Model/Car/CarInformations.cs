using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarController
{
    public class CarInformations
    {
        //speed
        public double CurrentSpeed { get; set; }
        public double TargetSpeed { get; set; }
        public double SpeedSteering { get; set; }
        
        //brake
        public double CurrentBrake { get; set; }
        public double TargetBrake { get; set; }
        public double BrakeSteering { get; set; }

        //wheel angle
        public double CurrentWheelAngle { get; set; }
        public double TargetWheelAngle { get; set; }
        public double WheelAngleSteering{ get; set; }

        //alert brake
        public bool AlertBrakeActive { get; set; }

        //gear
        public Gear CurrentGear { get; set; }

        public CarInformations()
        {
            CurrentSpeed = 0.0d;
            TargetSpeed = 0.0d;
            SpeedSteering = 0.0d;

            CurrentBrake = 0.0d;
            TargetBrake = 0.0d;
            BrakeSteering = 0.0d;

            CurrentWheelAngle = 0.0d;
            TargetSpeed = 0.0d;
            WheelAngleSteering = 0.0d;

            AlertBrakeActive = false;
        }

    }
}
