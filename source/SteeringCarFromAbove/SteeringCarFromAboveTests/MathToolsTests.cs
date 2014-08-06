using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteeringCarFromAbove;

namespace SteeringCarFromAboveTests
{
    [TestClass]
    public class MathToolsTests
    {
        [TestMethod]
        public void ModTest()
        {
            Assert.AreEqual(0.0d, MathTools.Mod(-3.0d, 3.0d));
            Assert.AreEqual(1.0d, MathTools.Mod(-2.0d, 3.0d));
            Assert.AreEqual(2.0d, MathTools.Mod(-1.0d, 3.0d));
            Assert.AreEqual(0.0d, MathTools.Mod(0.0d, 3.0d));
            Assert.AreEqual(1.0d, MathTools.Mod(1.0d, 3.0d));
            Assert.AreEqual(2.0d, MathTools.Mod(2.0d, 3.0d));
            Assert.AreEqual(0.0d, MathTools.Mod(3.0d, 3.0d));
        }

        [TestMethod]
        public void AngleEqualTest()
        {
            Assert.IsTrue(MathTools.AnglesEqual(10.0d, 20.0d, 15.0d));
            Assert.IsTrue(MathTools.AnglesEqual(355.0d, 5.0d, 15.0d));

            Assert.IsFalse(MathTools.AnglesEqual(355.0d, 5.0d, 5.0d));
            Assert.IsTrue(MathTools.AnglesEqual(10.0d, 20.0d, 5.0d));
        }
    }
}
