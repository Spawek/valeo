using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteeringCarFromAbove
{
    /// <summary>
    /// CarDriver class calculates car steering basing on current
    /// map and on previous informations (it keeps them inside its object)
    /// 
    /// It uses PID regulator
    /// </summary>
    public class CarDriver
    {
        private Map baseMap;
        private LinkedList<PositionAndOrientation> track;
        private PIDRegulator distanceRegulator;
        private PIDRegulator angleRegulator;

        private LinkedListNode<PositionAndOrientation> currentPosition;
        private enum Mode { ON_TRACK, REACHED_DESTINATION }
        private Mode currentMode = Mode.ON_TRACK;

        private const double DEFAULT_SPEED_IN_MS = 3.0d;
        private const double BREAKING_POWER_ON_DESTINATION = 0.8;

        private PIDSettings angleRegulatorSettings = new PIDSettings()
        {
            P_FACTOR_MULTIPLER = 0.0, //DISABLED

            I_FACTOR_MULTIPLER = 0.0d,
            I_FACTOR_SUM_MAX_VALUE = 0.0d,
            I_FACTOR_SUM_MIN_VALUE = 0.0d,
            I_FACTOR_SUM_SUPPRESSION_PER_SEC = 0.0d,

            D_FACTOR_MULTIPLER = 0.0d,
            D_FACTOR_SUPPRESSION_PER_SEC = 0.0d,
            D_FACTOR_SUM_MIN_VALUE = 0.0d,
            D_FACTOR_SUM_MAX_VALUE = 0.0d,

            MAX_FACTOR_CONST = 30.0d,
            MIN_FACTOR_CONST = -30.0d
        };

        private PIDSettings distanceRegulatorSettings = new PIDSettings()
        {
            P_FACTOR_MULTIPLER = 0.1,

            I_FACTOR_MULTIPLER = 0.0d,
            I_FACTOR_SUM_MAX_VALUE = 0.0d,
            I_FACTOR_SUM_MIN_VALUE = 0.0d,
            I_FACTOR_SUM_SUPPRESSION_PER_SEC = 0.0d,

            D_FACTOR_MULTIPLER = 0.0d,
            D_FACTOR_SUPPRESSION_PER_SEC = 0.0d,
            D_FACTOR_SUM_MIN_VALUE = 0.0d,
            D_FACTOR_SUM_MAX_VALUE = 0.0d,

            MAX_FACTOR_CONST = 30.0d,
            MIN_FACTOR_CONST = -30.0d
        };

        public CarDriver(Map _baseMap, List<PositionAndOrientation> _track)
        {
            if (_track.Count < 2)
                throw new ArgumentException();
            if (_baseMap.car == null)
                throw new ArgumentException();

            baseMap = _baseMap;
            track = new LinkedList<PositionAndOrientation>(_track);
            distanceRegulator = new PIDRegulator(distanceRegulatorSettings, "Car Driver distance regulator");
            distanceRegulator.targetValue = 0.0d;
            angleRegulator = new PIDRegulator(angleRegulatorSettings, "Car Driver ANGLE regulator");
            angleRegulator.targetValue = 0.0d;

            currentPosition = track.First;
        }

        private double CalculateDistance(PositionAndOrientation a, PositionAndOrientation b)
        {
            return Math.Sqrt(Math.Pow(a.x - b.x, 2.0d) + Math.Pow(a.y - b.y, 2.0d));
        }

        private double CalculateDistanceAndSideBetweenLineAndPoint(PositionAndOrientation line, PositionAndOrientation point)
        {
            double lineA = Math.Cos(line.angle / 180.0 * Math.PI);
            double lineB = Math.Sin(line.angle / 180.0 * Math.PI);
            double lineC = -1 * (line.x * lineA + line.y * lineB);

            return lineA * point.x + lineB * point.y + lineC; //  dividing by sqrt(A^2 + B^2) is not needed as these values are taken from sin/cos
        }

        private int checkPointCounter = 1;
        private LinkedListNode<PositionAndOrientation> GetNewCurrentPosition(Map map,
            LinkedListNode<PositionAndOrientation> currentPosition)
        {
            if (currentPosition.Next == null)
                return currentPosition;

            if (CalculateDistance(map.car, currentPosition.Value) <
                CalculateDistance(map.car, currentPosition.Next.Value))
            {
                return currentPosition;
            }
            else
            {
                Console.WriteLine("Reached checkpoint: {0}!", checkPointCounter++);
                return GetNewCurrentPosition(map, currentPosition.Next);
            }
        }

        /// <summary>
        /// state chart:
        /// * ----> ON_TRACK ----> REACHED_DESTINATION
        /// </summary>
        /// <param name="currentPosition"></param>
        /// <returns></returns>
        private Mode GetNewMode(LinkedListNode<PositionAndOrientation> currentPosition)
        {
            if (currentMode == Mode.REACHED_DESTINATION)
                return Mode.REACHED_DESTINATION;

            if (currentPosition == track.Last)
            {
                Console.WriteLine("Destination reached!");
                return Mode.REACHED_DESTINATION;
            }

            return Mode.ON_TRACK;
        }

        public CarSteering CalculateCarSteering(Map currentMap)
        {
            CarSteering steering = new CarSteering();

            currentPosition = GetNewCurrentPosition(currentMap, currentPosition);
            currentMode = GetNewMode(currentPosition);
            
            if (currentMode == Mode.ON_TRACK)
            {
                steering.speed = DEFAULT_SPEED_IN_MS;
                steering.brake = 0.0d;
            }
            else if (currentMode == Mode.REACHED_DESTINATION)
            {
                steering.speed = 0.0d;
                steering.brake = BREAKING_POWER_ON_DESTINATION;
            }
            else
            {
                throw new ApplicationException("unknown mode!");
            }

            double distanceAndSide = CalculateDistanceAndSideBetweenLineAndPoint(currentPosition.Value, currentMap.car);
            double distanceRegulatorValue = distanceRegulator.ProvideObjectCurrentValueToRegulator(distanceAndSide);
            double angleRegulatorValue = angleRegulator.ProvideObjectCurrentValueToRegulator(currentPosition.Value.angle);
            steering.angle = distanceRegulatorValue + angleRegulatorValue;

            return steering;
        }

    }
}
