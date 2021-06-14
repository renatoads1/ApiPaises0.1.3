using ApiPaises013.Domain.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using NUnit.Compatibility;

namespace ApiPaisesTeste
{
    [TestFixture]
    public class RegionTeste
    {
        [Test]
        public void TestRegionTeste() {
            var estados = new Region("a","a","a");
            var getall = estados.GetAllData();
            var res = false;
            if (!String.IsNullOrEmpty(getall)) {
                res = true;
            }
            Assert.IsTrue(res);
        }

    }
}
