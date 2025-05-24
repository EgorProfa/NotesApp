using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesApp;
using System;
using System.Data;
using System.Diagnostics;


namespace Test_Cases
{
    [TestClass]
    public class UnitTest1
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
            dbService.Dispose();
        }

        [DataTestMethod]
        [DataRow("test1", "TestPassword123!", true, DisplayName = "TC_1_1 - Валидный логин/пароль")]
        [DataRow("test3", "Iwoudnt1234t!!!", false, DisplayName = "TC_1_2 - Невалидный логин/пароль")]
        public void TestAuthentication(string username, string password, bool expectedSuccess)
        {
            string result = dbService.AuthenticateUser(username, password);

            if (expectedSuccess)
            {
                Assert.IsNotNull(result, "Аутентификация прошла успешно");
                Assert.AreEqual(username, result, "Возвращаемое имя пользователя должно соответствовать введенному.");
            }
            else
            {
                Assert.IsNull(result, "Аутентификация должна завершиться неудачей");
            }
        }


        [DataTestMethod]
        [DataRow("test37", "Iwoudnt1234t!!!", true, DisplayName = "TC_2_1 - Валидные логин/пароль")]
        [DataRow("test", "Iwoudnt", false, DisplayName = "TC_2_2 - Невалидный логин/пароль")]
        [DataRow("test1", "TestPassword123!", false, DisplayName = "TC_2_3 - Уже зарегистрированный пользователь")]
        public void TestRegistration(string username, string password, bool expectedSuccess)
        {

            var (success, message) = dbService.RegisterUser(username, password);

            Assert.AreEqual(expectedSuccess, success, message);
        }

        [DataTestMethod]
        [DataRow(1, "Новая заметка", "новый текст", true, DisplayName = "TC_3_1 - Создать заметку")]
        public void TestNoteCreation(int authorId, string title, string content, bool expectedSuccess)
        {
            var (success, message, noteId) = dbService.CreateNote(authorId, title, content);

            Assert.AreEqual(expectedSuccess, success, message);
            if (expectedSuccess)
            {
                Assert.IsTrue(noteId.HasValue, "Идентификатор должен быть возвращен.");
                Assert.IsTrue(noteId.Value > 0, "Идентификатор не отрицательный");
            }
        }

        [DataTestMethod]
        [DataRow(1, 1, "Новая заметка изменённая", "новый текст но изменённый", true, DisplayName = "TC_3_2 - Обновление заметки")]
        public void TestNoteUpdate(int noteId, int userId, string title, string content, bool expectedSuccess)
        {
            var (createSuccess, _, createdNoteId) = dbService.CreateNote(userId, "Новая заметка", "новый текст");
            if (!createSuccess) Assert.Fail("Не удалось создать заметку");
            var (success, message) = dbService.UpdateNote(createdNoteId ?? noteId, userId, title, content);
            Assert.AreEqual(expectedSuccess, success, message);
        }

        [DataTestMethod]
        [DataRow(1, 1, true, DisplayName = "TC_3_3 - Удаление своей заметки")]
        [DataRow(3, 1, false, DisplayName = "TC_3_4 - Удаление чужой заметки")]
        public void TestNoteDeletion(int noteId, int userId, bool expectedSuccess)
        {
            if (expectedSuccess)
            {
                var (createSuccess, _, createdNoteId) = dbService.CreateNote(userId, "Новая заметка", "новый текст");
                if (!createSuccess) Assert.Fail("Не удалось создать заметку");
                noteId = createdNoteId ?? noteId;
            }
            var (success, message) = dbService.DeleteNote(noteId, userId);
            Assert.AreEqual(expectedSuccess, success, message);
        }

        [TestMethod]
        [DataRow("test1", "TestPassword123!", "Новая заметка ", "новый текст", DisplayName = "TС_4_1 - Создание записи о создании заметки")]
        public void TC_4_1_AuditCreateNote(string login, string pass, string label, string text)
        {
            var (success, _, _) = dbService.CreateNote(DatabaseService.GetUserID(login), label, text);
            Assert.IsTrue(success, "Заметка должна быть создана");

            DataRow auditRecord = dbService.GetLastAuditRecord(1);
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("CREATE", auditRecord["action"].ToString(), "Тип действия должен быть CREATE");
        }

        [TestMethod]
        [DataRow("test1", "TestPassword123!", 10, "Новая заметка ", "новый текст", DisplayName = "TС_4_1 - Создание записи о изменении заметки")]
        public void TC_4_2_AuditUpdateNote(string login, string pass, int note_id, string label, string text)
        {
            var (success, _) = dbService.UpdateNote(note_id, DatabaseService.GetUserID(login), label, text);

            DataRow auditRecord = dbService.GetLastAuditRecord(DatabaseService.GetUserID(login));
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("EDIT", auditRecord["action"].ToString(), "Тип действия должен быть EDIT");
        }

        [TestMethod]
        [DataRow("test1", 11, DisplayName = "TС_4_1 - Создание записи о удалении заметки")]
        public void TC_4_3_AuditDeleteNote(string login, int note_id)
        {
            var (success, _) = dbService.DeleteNote(note_id, DatabaseService.GetUserID(login));
            Assert.IsTrue(success, "Заметка должна быть удалена");

            DataRow auditRecord = dbService.GetLastAuditRecord(DatabaseService.GetUserID(login));
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("DELETE", auditRecord["action"].ToString(), "Тип действия должен быть DELETE");
        }
    }
}
