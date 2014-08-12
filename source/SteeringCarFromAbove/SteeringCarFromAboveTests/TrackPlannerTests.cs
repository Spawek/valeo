using System;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteeringCarFromAbove;

namespace SteeringCarFromAboveTests
{
    [TestClass]
    public class TrackPlannerTests
    {
        [TestMethod]
        public void TrackPlannerFindSmallForwardTrackTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 9, angleTolerance: 19.0d,
                positionStep: 10.0d, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0d, _y: 500.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0d, _y: 510.0d, _angle: 90.0d);
            
            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.AreEqual(2, track.Count);

        }

        [TestMethod]
        public void TrackPlannerFindLongForwardTrackTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 9, angleTolerance: 19.0d,
                positionStep: 10.0d, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0d, _y: 500.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0d, _y: 600.0d, _angle: 90.0d);

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.AreEqual(11, track.Count);
        }

        [TestMethod]
        public void TrackPlannerFind90TurnTrackTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 9, angleTolerance: 19.0d,
                positionStep: 10.0d, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);

            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0d, _y: 500.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 600.0d, _y: 600.0d, _angle: 0.0d);

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.IsTrue(track.Count > 10);  // coz it not a bad expectation ;)
        }
        
        [TestMethod]
        public void TrackPlannerFind360TurnTrackTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 9, angleTolerance: 19.0d,
                positionStep: 10.0d, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);
            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0d, _y: 500.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0d, _y: 500.0d, _angle: 90.0d);

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.IsTrue(track.Count > 10); // its really bad assert, but still better than nothing
        }

        [TestMethod]
        public void TrackPlannerLongRunTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 29, angleTolerance: 19.0d,
                positionStep: 30.0d, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);
            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 100.0d, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 900.0d, _y: 900.0d, _angle: 90.0d);

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.IsTrue(track.Count > 10); // its really bad assert, but still better than nothing
        }

        [TestMethod]
        public void TrackPlannerObstacleTest()
        {
            TrackPlanner planner = new TrackPlanner(
                locationTolerance: 39, angleTolerance: 19.0d,
                positionStep: 40.0, angleStep: 20.0d,
                mapSizeX: 1000.0d, mapSizeY: 1000.0d);
            Map map = new Map(1000, 1000);

            map.car = new PositionAndOrientation(_x: 500.0, _y: 100.0d, _angle: 90.0d);
            map.parking = new PositionAndOrientation(_x: 500.0, _y: 900.0, _angle: 90.0d);
            map.obstacles.Add(new Rectangle(350, 500, 300, 50));

            List<PositionAndOrientation> track = planner.PlanTrack(map);
            Assert.IsTrue(track.Count > 20); 
        }



    }
}
