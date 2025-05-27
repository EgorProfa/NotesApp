using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesApp;
using Npgsql;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Test_Cases
{
    [TestClass]
    public class Module_Reg
    {
        private DatabaseService dbService;
        private string _lastRegisteredUser = null;

        [TestInitialize]
        public void TestInitialize()
        {
            dbService = new DatabaseService();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_lastRegisteredUser != null)
            {
                var (success, message) = dbService.DeleteUser(_lastRegisteredUser);
                Trace.WriteLine(success
                    ? $"Удален тестовый пользователь: {_lastRegisteredUser}"
                    : $"Ошибка удаления: {message}");
            }
        }

        [DataTestMethod]
        [DataRow("test37", "test", false, DisplayName = "Валидный логин, невалидный пароль")]
        [DataRow("test", "Iwoudnt1234t!!!", false, DisplayName = "Невалидный логин, валидный пароль")]
        [DataRow("test37", "Iwoudnt1234t!!!", true, DisplayName = "Валидные логин и пароль")]
        [DataRow("", "", false, DisplayName = "Пустые логин и пароль")]
        [DataRow("test1", "TestPassword123!", false, DisplayName = "Уже зарегистрированный пользователь")]
        public void TC_2_1_TestRegistration(string username, string password, bool expectedSuccess)
        {
            var (success, message) = dbService.RegisterUser(username, password);

            if (success)
            {
                _lastRegisteredUser = username;
            }

            Assert.AreEqual(expectedSuccess, success, message);
            Trace.WriteLine(message);
        }

        [DataTestMethod]
        [DataRow("hashTestUser", "Test1234!Password")]
        public void TC_2_2_TestPasswordHashing(string username, string password)
        {
            var (success, message) = dbService.RegisterUser(username, password);
            Assert.IsTrue(success, $"Регистрация не удалась: {message}");
            _lastRegisteredUser = username;

            string storedHash = GetPasswordHash(username);

            Assert.IsNotNull(storedHash, "Хеш не найден в базе");
            Assert.AreNotEqual(password, storedHash, "Пароль не должен храниться в открытом виде");
            Trace.WriteLine("Пароль успешно хэшируется");
        }

        public string GetPasswordHash(string username)
        {
            using (var cmd = new NpgsqlCommand("SELECT password_hash FROM users WHERE username = @username", dbService.GetConnection()))
            {
                cmd.Parameters.AddWithValue("@username", username);
                return cmd.ExecuteScalar()?.ToString();
            }
        }
    }
}
