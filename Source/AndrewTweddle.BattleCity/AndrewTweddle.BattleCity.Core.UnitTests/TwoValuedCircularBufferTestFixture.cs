using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;

namespace AndrewTweddle.BattleCity.Core.UnitTests
{
    [TestFixture]
    public class TwoValuedCircularBufferTestFixture
    {
        [Test]
        public void TestInsertions()
        {
            TwoValuedCircularBuffer<int> buffer = new TwoValuedCircularBuffer<int>(10);
            for (int i = 0; i < 10; i++)
            {
                buffer.Add(i, 1);
            }
            for (int j = 0; j < 5; j++)
            {
                var removed = buffer.Remove();
                Assert.AreEqual(removed.Item1, j);
                Assert.AreEqual(removed.Item2, 1);
            }
            for (int i = 10; i < 15; i++)
            {
                buffer.Add(i, 2);
            }
            for (int j = 5; j < 10; j++)
            {
                var removed = buffer.Remove();
                Assert.AreEqual(removed.Item1, j);
                Assert.AreEqual(removed.Item2, 1);
            }
            for (int j = 10; j < 15; j++)
            {
                var removed = buffer.Remove();
                Assert.AreEqual(removed.Item1, j);
                Assert.AreEqual(removed.Item2, 2);
            }
        }
    }
}
