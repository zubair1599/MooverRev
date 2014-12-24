using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moovers.Tests
{
    [TestClass]
    public class JsonSerialize
    {
        // MOOCRM-66 -- ensure all dates are sent with local timezone.
        [TestMethod]
        public void TestJsDates()
        {
            var utcDateObj = new DateTime(2011, 1, 1);
            var localDateObj = new DateTime(2011, 1, 1, 0, 0, 0, DateTimeKind.Local);
            Assert.AreEqual(utcDateObj.SerializeToJson(), localDateObj.SerializeToJson());
        }
    }
}
