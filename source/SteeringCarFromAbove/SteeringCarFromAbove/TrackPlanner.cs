using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuadTreeLib;

namespace SteeringCarFromAbove
{
    //TODO: Track can be found with divide and conquer method to decrease complexity
    // at first find track with big step and tolerance, then find half track with smaller step and tolerance

    //TODO: CHANGE ALL CODE TO RADIANS!!!
    //TODO: add obstacles
    
    //NOTE: target can be not found becase of being close to target before but not exactly there what would block the track
    //TODO: increase target finding tolerance if that problem occurs
    public class TrackPlanner
    {
        private class BFSNode : IHasRect
        {
            public BFSNode(PositionAndOrientation _position, BFSNode _predecessor)
            {
                position = _position;
                predecessor = _predecessor;
                Rectangle = new System.Drawing.RectangleF((float)position.x, (float)position.y, 0.001f, 0.001f);
            }
            public PositionAndOrientation position;
            public BFSNode predecessor;

            public System.Drawing.RectangleF Rectangle { get; private set; }
        }

        public TrackPlanner(double locationTolerance, double angleTolerance, double positionStep, double angleStep,
            double mapSizeX, double mapSizeY)
        {
            locationToleranceSquared_ = locationTolerance * locationTolerance;
            locationTolerance_ = locationTolerance;
            angleTolerance_ = angleTolerance;
            positionStep_ = positionStep;
            angleStep_ = angleStep;
            mapSizeX_ = mapSizeX;
            mapSizeY_ = mapSizeY;

            seenTree_ = new QuadTree<BFSNode>(new System.Drawing.RectangleF(0.0f, 0.0f, (float)mapSizeX, (float)mapSizeY));
        }

        public List<PositionAndOrientation> PlanTrack(Map map)
        {
            Queue<BFSNode> frontier = new Queue<BFSNode>();
            BFSNode startPoint = new BFSNode(map.car, null);
            frontier.Enqueue(startPoint);

            PositionAndOrientation target = map.parking;

            while (frontier.Count != 0)
            {
                BFSNode curr = frontier.Dequeue();

                List<BFSNode> succesors = generateSuccessors(curr);

                foreach (BFSNode s in succesors)
                {
                    if (ArePointsSame(s.position, target))
                        return GetPathBetweenPoints(startPoint, s);
                }

                succesors.ForEach(x => frontier.Enqueue(x));
                succesors.ForEach(x => seenTree_.Insert(x));
            }

            return new List<PositionAndOrientation>();  // couldn't find result - returning empty list
        }

        private List<PositionAndOrientation> GetPathBetweenPoints(BFSNode start, BFSNode target)
        {
            List<PositionAndOrientation> path = new List<PositionAndOrientation>();

            BFSNode curr = target;
            while (curr.predecessor != null)
            {
                path.Add(curr.position);
                curr = curr.predecessor;
            }

            path.Reverse();

            return path;
        }

        private List<BFSNode> generateSuccessors(BFSNode predecessor)
        {
            List<BFSNode> successors = new List<BFSNode>();

            double oldAngleInRadians = predecessor.position.angle / 180.0f * Math.PI;
            double angleStepInRadians = angleStep_ / 180.0f * Math.PI;
            foreach (int i in new List<int>(){0, -1, 1})  // HARDCODE!
            {
                double newAngleInRadians = oldAngleInRadians + angleStepInRadians * i;
                double newX = predecessor.position.x + Math.Cos(newAngleInRadians) * positionStep_;
                double newY = predecessor.position.y + Math.Sin(newAngleInRadians) * positionStep_;

                if (newX < 0 || newX > mapSizeX_ || newY < 0 || newY > mapSizeY_)
                    continue;

                double newAngle = predecessor.position.angle + angleStep_ * i;
                PositionAndOrientation newPosition = new PositionAndOrientation(newX, newY, newAngle);

                if (!IsPositionSeen(newPosition))
                {
                    successors.Add(new BFSNode(newPosition, predecessor));
                    addedCount++;
                }
                else
                {
                    rejectedCount++;
                }
            }

            return successors;
        }


        int addedCount = 0;
        int rejectedCount = 0;

        //TODO: create static map with each possible square??? O(n) = c
        //TODO: change to octree for optimization (O(n) = n for search? ;/
        //TODO: can be even simpler algorithm on sorted list x and then sorted list y and then sorted list angle
        //TODO: use that: http://www.codeproject.com/Articles/30535/A-Simple-QuadTree-Implementation-in-C
        List<PositionAndOrientation> seen = new List<PositionAndOrientation>();
        private bool IsPositionSeen(PositionAndOrientation point)
        {
            //return seen.Any(x => ArePointsSame(x, point));
            List<BFSNode> matchingPositionNodes =
                seenTree_.Query(new System.Drawing.RectangleF((float)point.x - (float)locationTolerance_ / 2,
                    (float)point.y - (float)locationTolerance_ / 2, (float)locationTolerance_, (float)locationTolerance_));

            return matchingPositionNodes.Any(
                x => MathTools.AnglesEqual(x.position.angle, point.angle, angleTolerance_));
        }

        private bool ArePointsSame(PositionAndOrientation a, PositionAndOrientation b)
        {
            return Math.Pow(a.x - b.x, 2.0d) + Math.Pow(a.y - b.y, 2.0d) <= locationToleranceSquared_
                &&
                 MathTools.AnglesEqual(a.angle, b.angle, angleTolerance_);
        }

        private double locationToleranceSquared_; // squared here for optimization
        private double locationTolerance_;
        private double angleTolerance_;
        private double positionStep_;
        private double angleStep_;
        private double mapSizeX_;
        private double mapSizeY_;
        private QuadTree<BFSNode> seenTree_;
    }
}
