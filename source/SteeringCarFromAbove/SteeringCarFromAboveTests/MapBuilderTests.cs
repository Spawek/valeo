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

            Marker carMarker = new Marker();
            Marker parkingPlaceMarker = new Marker();
            List<Marker> stableMarkers = new List<Marker>(){new Marker(), new Marker()};

            ObjectsToTrace objectsToTrace = new ObjectsToTrace(stableMarkers, carMarker, parkingPlaceMarker);

            MapBuilder mapBuilder = new MapBuilder(markerFinderMock, obstaclesFinderMock, objectsToTrace);
        }
    }
}
