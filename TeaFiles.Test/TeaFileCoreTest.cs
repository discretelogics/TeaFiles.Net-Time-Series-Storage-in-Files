// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class TeaFileCoreTest
    {
        [TestInitialize]
        public void Init()
        {
            // Ensure english error messages on non english systems.
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        }

        [TestMethod]
        public void ExoticCoverage()
        {
            Stream stream = null;
            var tfc = new TeaFileCore(stream, true);
            tfc.Description = null;
            Executing.This(() => tfc.IsAccessibleWith(null, ItemDescriptionElements.All)).Should().NotThrow();
            Executing.This(() => tfc.ItemDescriptionExists()).Should().Throw<InvalidOperationException>();
        }
    }
}
