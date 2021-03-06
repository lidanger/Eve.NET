﻿using NUnit.Framework;

namespace Eve.Tests
{
    [TestFixture]
    class BasicAuthenticator
    {
        [Test] 
        public void BasicAuthenticatorDefaults() { 
            var ba = new Authenticators.BasicAuthenticator("user", "pw");
            Assert.AreEqual(ba.UserName, "user"); 
            Assert.AreEqual(ba.Password, "pw");
        }
    }
}
