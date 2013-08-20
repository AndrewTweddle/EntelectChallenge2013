using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace AndrewTweddle.BattleCity.Core.UnitTests
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var fix = new TwoValuedCircularBufferTestFixture();
            fix.TestInsertions();
        }
    }
}
