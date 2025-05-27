using DatabaseLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace Test_Cases
{
    [TestClass]
    public class Module_Auth
    {
        private DatabaseService dbService;

        [TestInitialize]
        public void TestInitialize()
        {
            dbService = new DatabaseService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            dbService.ClearSession();
        }

        [DataTestMethod]
        [DataRow("test1", "TestPassword123!", true, DisplayName = "Валидный логин/пароль")]
        [DataRow("test3", "Iwoudnt1234t!!!", false, DisplayName = "Невалидный логин/пароль")]
        [DataRow("test1", "' OR '1'='1", false, DisplayName = "SQL-инъекция")]
        [DataRow("", "", false, DisplayName = "Пустые логин/пароль")]
        public void TC_1_1_TestAuthentication(string username, string password, bool expectedSuccess)
        {
            var result = dbService.AuthenticateUserWithSession(username, password);

            Assert.AreEqual(expectedSuccess, result.Success,
                $"{result.Message}");
            Trace.WriteLine(result.Message);
            if (expectedSuccess)
                Assert.IsNotNull(result.UserId, $"Должен вернуться ID пользователя");
        }

        [DataTestMethod]
        [DataRow("test1", "TestPassword123!", false)]
        public void TC_1_2_TestAlreadyLogedIn(string username, string password, bool expectedSuccess)
        {
            var firstLogin = dbService.AuthenticateUserWithSession(username, password);
            var secondLogin = dbService.AuthenticateUserWithSession(username, password);

            Assert.AreEqual(expectedSuccess, secondLogin.Success,
                $"{secondLogin.Message}");
            Trace.WriteLine(secondLogin.Message);
        }
    }
}
