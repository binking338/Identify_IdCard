using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var idcardno = DotNETX.Identify.IDCard.IdCardNo.Parse("330881198701290034");
            Assert.AreEqual("浙江省江山市", idcardno.County);
            Assert.AreEqual(new DateTime(1987, 1, 29), idcardno.Birthday);
        }
    }
}
