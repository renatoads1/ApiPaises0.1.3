using NUnit.Framework;
using ApiPaises013.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiPaisesTeste
{
    [TestFixture]
    class PaisesTeste
    {
        [Test]
        public void TestePais() {
            var obj = new Paises("a", "a", "a");
            var objresp = obj.PaisesTeste();
            var ret = false;
            if (!String.IsNullOrEmpty(objresp)) {
                ret = true;
            }

            Assert.IsTrue(ret);
        }

    }
}
