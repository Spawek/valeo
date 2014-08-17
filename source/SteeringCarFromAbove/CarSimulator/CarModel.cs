using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarSimulator
{
    public class EnginePointStats
    {
        public double RPM; // 1/s
        public double power; // W //engine output power (on wheel - when there are no transmission)
        public double torque; // N*m //ending output torque (on wheel - when there are no transmission)

        public EnginePointStats(double _RPM, double _power, double _torque)
        {
            RPM = _RPM;
            power = _power;
            torque = _torque;
        }
    }
    public abstract class CarModel
    {
        // Gravitational Acceleration at the Earths surface. 
        public const double EARTH_GRAV_CONST = 9.8067;

        public abstract List<EnginePointStats> EngineStats { get; }
        public abstract double StaticEngineResistanceForces { get; } //N*m
        public abstract double DynamicEngineResistancePerRPM { get; } //N*m/RPM
        public abstract double EngineMomentum { get; } //kg * m^2
        public abstract double Torque { get; }
        public abstract double Power { get; }
        public abstract double WheelRadius { get; }
        public abstract double DifferentialRatio { get; }
        public abstract double MaxEngineRPM { get; }
        public abstract double Mass { get; }
        public abstract double Width { get; }
        public abstract double Height { get; }
        public abstract double[] GearTransmissionRatios { get; }
        public abstract int MaxGear { get; }
        public abstract double WheelMomentum { get; }
        public abstract double WheelMass { get; }
        public abstract double WheelsNo { get; }
        public double ExternalResistanceForces { get; set; }
        public double RPM { get; set; }
        public double BrakingLevel { get; set; } // in range [0,1]
        public abstract double MaxBreakingForcePerWheel { get; }
        public abstract int BrakingWheelsNo { get; }
        public double DistanceDoneInMeters { get; set; }
        public abstract int AcceleratingWheelsNo { get; }
        public double StaticFrictionFactor { get; set; } //set coz it van vary when evironment changes
        public double KineticFrictionFactor { get; set; } //set coz it van vary when evironment changes

        //automatic gearbox part
        public abstract bool IsGearBoxAutomatic { get; }
        public abstract double RpmToRaiseGearOnAutomaticGearbox { get; }
        public abstract double RpmToLowerGearOnAutomaticGearbox { get; }

        //air resistance part
        public double AirDensity { get { return 1.2; } } //kg/m^2 //source: http://pl.wikipedia.org/wiki/Gęstość_powietrza
        public abstract double CarDragCoeffcient { get; }
        public double CarFrontSurface { get { return Height * Width * 0.85; } } //TODO: pod samochodem jest spora szczelina, ktora trzeba uwzglednic (na razie dalem na pale *0.85)
        public double AerodynemicResistance { get { return CarDragCoeffcient * AirDensity * Math.Pow(SpeedInMetersPerSecond, 2.0) * CarFrontSurface / 2; } }

        //rolling resistance //from: http://www.engineeringtoolbox.com/rolling-friction-resistance-d_1303.html //TODO: make it some better way
        public abstract double TyrePresure { get; }
        public double RollingResistanceCoefficient { get { return 0.005 + 1.0 / TyrePresure * (0.01 + 0.0095 * Math.Pow(SpeedInKilometersPerHour / 100.0, 2.0)); } }
        public double RollingResistance { get { return RollingResistanceCoefficient * Mass * EARTH_GRAV_CONST; } }

        private double __THROTTLE_OPPENING_LEVEL__ = 0.0;
        public double ThrottleOppeningLevel
        {
            get { return __THROTTLE_OPPENING_LEVEL__; }
            set
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentException("throttle oppening level is out of [0,1] range");

                __THROTTLE_OPPENING_LEVEL__ = value;
            }
        }

        public double RPS { get { return RPM / 60; } } //obroty na sekunde
        public double SpeedInMetersPerSecond { get { return RPS * TransmissionRate * WheelCircuit; } }
        public double SpeedInKilometersPerHour { get { return SpeedInMetersPerSecond * 3.6; } }
        public double ForwardForceOnWheelsFromEngine { get { return Torque / TransmissionRate / WheelRadius * ThrottleOppeningLevel; } }
        public double WheelCircuit { get { return WheelRadius * 2 * Math.PI; } }
        public double TransmissionRate { get { return GearRatio(CurrGear) * DifferentialRatio; } }
        public double DynamicEngineResistanceForces { get { return DynamicEngineResistancePerRPM * RPM; } }
        public double EngineResistanceForces { get { return DynamicEngineResistanceForces + StaticEngineResistanceForces; } }
        public double EngineResistanceForcesOnWheels { get { return EngineResistanceForces / TransmissionRate / WheelRadius; } }
        public double BrakingForce { get { return BrakingLevel * BrakingWheelsNo * MaxBreakingForcePerWheel; } }

        private int __CURR_GEAR__ = 1;
        public int CurrGear
        {
            get { return __CURR_GEAR__; }

            /*
             *   I assume that total momnentum is unchanged:
             *   RPS * 
             *   (M_engine +                                    // engine momentum 
             *   transmission_rate * M_wheel * no_of_wheels +   // wheels momentum
             *   transmission_rate * wheel_circuit * car_mass)  // car mass momentum
             *       = const;
             */
            set
            {
                if (value != __CURR_GEAR__)
                {
                    double oldTransmissionRate = TransmissionRate;
                    __CURR_GEAR__ = value;
                    double newTranmissionRate = TransmissionRate;

                    RPM = RPM *
                        (EngineMomentum +
                        oldTransmissionRate * WheelMomentum * WheelsNo +
                        oldTransmissionRate * WheelCircuit * Mass)
                        /
                        (EngineMomentum +
                        newTranmissionRate * WheelMomentum * WheelsNo +
                        newTranmissionRate * WheelCircuit * Mass);
                }
            }
        }

        public double GearRatio(int gear)
        {
            if (gear > MaxGear || gear < 1)
                throw new ArgumentException("gear is invalid");

            return GearTransmissionRatios[gear - 1];
        }

        public CarModel()
        {
            RPM = 0;
            ExternalResistanceForces = 0;
            ThrottleOppeningLevel = 0;
            CurrGear = 1;
            DistanceDoneInMeters = 0;
        }

        public abstract void Start();

        private static double LinearApprox(double x1, double y1, double x2, double y2, double wantedX)
        {
            double yDiff = y2 - y1;
            double xDiff = x2 - x1;
            double changePerPoint = yDiff / xDiff;
            double wantedXdiffFromX1 = wantedX - x1;

            return y1 + changePerPoint * wantedXdiffFromX1;
        }

        private bool areEngineStatsSortedByRPM = false;
        protected double GetTorque(double RPM)
        {
            if (!areEngineStatsSortedByRPM)
            {
                EngineStats.OrderBy(stat => stat.RPM);
                areEngineStatsSortedByRPM = true;
            }

            double torque;
            var engineStat = EngineStats.Find(x => x.RPM == RPM);
            if (engineStat != null) //RPM is a point on our map
            {
                torque = engineStat.torque;
            }
            else
            {
                //RPM is not a point on our map (and it has top be approximated)
                if (RPM > EngineStats.First().RPM && RPM < EngineStats.Last().RPM) //it is in scale
                {
                    var p1 = EngineStats.Find(x => x.RPM > RPM);
                    var p2 = EngineStats.FindLast(x => x.RPM < RPM); //can be optimized
                    torque = LinearApprox(p1.RPM, p1.torque, 0, 0, RPM);
                }
                else if (RPM < EngineStats.First().RPM) //if its under a scale
                {
                    var p1 = EngineStats[0];
                    torque = LinearApprox(p1.RPM, p1.torque, 0, 0, RPM);
                }
                else //if its over a scale
                {
                    var p1 = EngineStats.Last();
                    torque = LinearApprox(p1.RPM, p1.torque, MaxEngineRPM, 0, RPM);
                }
            }

            return torque;
        }
        /* MY CALCULATIONS ABOUT CAR DYNAMICS (TORQUE BASED):
         * torque_current = E_engine * M_engine + //E = epsilon, M = momentum //engine ineria
         *          + E_wheels * M_wheels +  //wheels inertia
         *          + a_car * m_car / transmission_rate / r_wheel + //a = acceleration, m = mass, r = radius //car dynamics //ERROR - it probably should be multiplied by transmission //TODO: look for mistake
         *          + internal_forces //???
         *          + external_forces / transmission_rate / r_wheel //external forces on wheel
         *
         * transmission_rate = omega_wheel / omega_engine 
         * so:
         * E_wheels = E_engine * transmission_rate  
         * 
         * 
         *  torque_current = torque[curr_rpm] * gas_in_peccents_current
         */
        private DateTime lastTickTime = DateTime.Now;
        public void CalculationsTick()
        {
            TimeSpan timeFromLastTick = DateTime.Now - lastTickTime;
            lastTickTime = DateTime.Now;
            double tyresForceBallance = 0;

            tyresForceBallance += ForwardForceOnWheelsFromEngine;
            tyresForceBallance -= EngineResistanceForcesOnWheels;
            tyresForceBallance -= RollingResistance;
            tyresForceBallance -= BrakingForce * Math.Sign(SpeedInMetersPerSecond); //force opposite to speed vector

            double carForceBallance = 0;

            int workingWheels;
            if (BrakingForce * Math.Sign(SpeedInMetersPerSecond) > ForwardForceOnWheelsFromEngine - EngineResistanceForcesOnWheels - RollingResistance)
            {
                workingWheels = BrakingWheelsNo;
            }
            else
            {
                workingWheels = AcceleratingWheelsNo;
            }

            if (Math.Abs(tyresForceBallance) < Mass * EARTH_GRAV_CONST * StaticFrictionFactor * workingWheels / WheelsNo)
            {
                carForceBallance = tyresForceBallance;
            }
            else
            {
                carForceBallance = Math.Sign(tyresForceBallance) * Mass * EARTH_GRAV_CONST * KineticFrictionFactor * workingWheels / WheelsNo;
                /*
                 * IMPORTANT: NOTE: 
                 * this model just wastes energy of slide
                 * 
                 * in real this energy forces wheels to move wheels and engine faster, but not accelerate the car
                 */

                //TODO: fix it by adding some variables for connections (engine <---> wheels) and (wheels <---> enviroment)
                Console.Write("IM SLIDING ");
            }

            carForceBallance -= AerodynemicResistance;
            carForceBallance -= ExternalResistanceForces;

            double Acceleration = //a = F/m (but we got some additional radial inetrions, so we have to remember about E = M / I)
                carForceBallance /
                    (Mass +
                    EngineMomentum / TransmissionRate / WheelRadius + // engine inertion
                    WheelsNo * WheelMomentum / WheelRadius); // wheels inertion

            double Epsilon_engine = Acceleration / WheelCircuit / TransmissionRate;

            RPM += Epsilon_engine * timeFromLastTick.TotalSeconds * 60.0;

            if (IsGearBoxAutomatic)
            {
                MakeAutoGearChanges();
            }

            if (RPM < 5)
            {
                RPM = 0;
            }

            DistanceDoneInMeters += Math.Abs(SpeedInMetersPerSecond) * timeFromLastTick.TotalSeconds;
        }

        private void MakeAutoGearChanges()
        {
            if (RPM > RpmToRaiseGearOnAutomaticGearbox) 
            {
                if (CurrGear < MaxGear)
                {
                    CurrGear++;
                }
            }

            if (RPM < RpmToLowerGearOnAutomaticGearbox)
            {
                if (CurrGear > 1)
                {
                    CurrGear--;
                }
            }
        }
    }
}
