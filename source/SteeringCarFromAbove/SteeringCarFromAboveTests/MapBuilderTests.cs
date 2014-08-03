using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using SteeringCarFromAbove;
using System.Collections.Generic;

namespace SteeringCarFromAboveTests
{
    [TestClass]
    public class MapBuilderTests
    {
        [TestMethod]
        public void BuildMapTest()
        {
            Rhino.Mocks.MockRepository mocksReposiory = new MockRepository();
            MarkerFinder markerFinderMock = mocksReposiory.StrictMock<MarkerFinder>();
            ObstaclesFinder obstaclesFinderMock = mocksReposiory.StrictMock<ObstaclesFinder>();

            Image image = new Image(1000, 1000);

            Marker carMarker = new Marker();
            Marker parkingMarker = new Marker();
            List<Marker> stableMarkers = new List<Marker>(){new Marker()};

            PositionAndOrientation carPosition = new PositionAndOrientation(5.0f, 4.0f, 90.0f);
            PositionAndOrientation parkingPosition = new PositionAndOrientation(1.0f, 1.0f, 1.0f);
            List<PositionAndOrientation> stableMarkersPosition =
                new List<PositionAndOrientation>() { new PositionAndOrientation(3.0f, 1.0f, 0.0f) };

            ObjectsToTrace objectsToTrace = new ObjectsToTrace(stableMarkers, carMarker, parkingMarker);
            MapBuilder mapBuilder = new MapBuilder(markerFinderMock, obstaclesFinderMock, objectsToTrace);

            using (mocksReposiory.Record())
            {
                markerFinderMock.Expect(x => x.FindMarker(image, carMarker))
                    .Return(carPosition);
                markerFinderMock.Expect(x => x.FindMarker(image, parkingMarker))
                    .Return(parkingPosition);
                markerFinderMock.Expect(x => x.FindMarker(image, stableMarkers[0]))
                    .Return(stableMarkersPosition[0]);
            }
            using (mocksReposiory.Playback())
            {
                Map map = mapBuilder.BuildMap(image);

                Assert.AreEqual(carPosition, map.car);
                Assert.AreEqual(parkingPosition, map.parking);
                Assert.AreEqual(stableMarkersPosition[0], map.markers[stableMarkers[0]]);
            }
        }

        [TestMethod]
        public void RebuildMapTest()
        {
            Rhino.Mocks.MockRepository mocksReposiory = new MockRepository();
            MarkerFinder markerFinderMock = mocksReposiory.StrictMock<MarkerFinder>();
            ObstaclesFinder obstaclesFinderMock = mocksReposiory.StrictMock<ObstaclesFinder>();

            Image image = new Image(0, 0);

            Marker carMarker = new Marker();
            Marker parkingMarker = new Marker();
            List<Marker> stableMarkers = new List<Marker>() { new Marker() };

            PositionAndOrientation carPosition = new PositionAndOrientation(5.0f, 4.0f, 0.0f);
            PositionAndOrientation parkingPosition = new PositionAndOrientation(1.0f, 1.0f, 1.0f);
            List<PositionAndOrientation> stableMarkersPosition =
                new List<PositionAndOrientation>() { new PositionAndOrientation(3.0f, 5.0f, 90.0f) };

            ObjectsToTrace objectsToTrace = new ObjectsToTrace(stableMarkers, carMarker, parkingMarker);
            MapBuilder mapBuilder = new MapBuilder(markerFinderMock, obstaclesFinderMock, objectsToTrace);

            using (mocksReposiory.Record())
            {
                markerFinderMock.Expect(x => x.FindMarker(image, carMarker))
                    .Return(carPosition);
                markerFinderMock.Expect(x => x.FindMarker(image, parkingMarker))
                    .Return(parkingPosition);
                markerFinderMock.Expect(x => x.FindMarker(image, stableMarkers[0]))
                    .Return(stableMarkersPosition[0]);
            }
            Map map;
            using (mocksReposiory.Playback())
            {
                map = mapBuilder.BuildMap(image);
            }

            PositionAndOrientation newCarPosition = new PositionAndOrientation(-4.0f, 5.0f, 90.0f);
            List<PositionAndOrientation> newStableMarkersPosition =
                new List<PositionAndOrientation>() { new PositionAndOrientation(-3.0f, 1.0f, 180.0f) };

            Image newImage = new Image(0, 0);
            using (mocksReposiory.Record())
            {
                markerFinderMock.Expect(x => x.FindMarker(newImage, carMarker))
                    .Return(newCarPosition);
                markerFinderMock.Expect(x => x.FindMarker(newImage, stableMarkers[0]))
                    .Return(newStableMarkersPosition[0]);
            }
            using (mocksReposiory.Playback())
            {
                mapBuilder.UpdateCarPosition(map, newImage);

                double delta = 0.0001f;
                Assert.AreEqual(7.0f, map.car.x, delta);
                Assert.AreEqual(6.0f, map.car.y, delta);
                Assert.AreEqual(0.0f, map.car.angle, delta);
            }
        }

    }
}
