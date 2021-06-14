using NUnit.Framework;
using ApiPaises013.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiPaisesTeste
{
    [TestFixture]
    class CityTeste
    {
        [Test]
        public void TestCity() {
            var teste = new City("a","a","a","a","a","a");
            var restest = teste.TestCity("a", "a", "a", "a", "a", "a");
            var re = false;
            if (!String.IsNullOrEmpty(restest)) {
                re = true;
            }
            Assert.IsTrue(re);
        }
    }
}
