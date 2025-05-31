using DatabaseLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Diagnostics;

namespace Test_Cases
{
    [TestClass]
    public class Module_Audit
    {
        private DatabaseService dbService;
        private int _testNoteId;

        [TestInitialize]
        public void TestInitialize()
        {
            dbService = new DatabaseService();
            _testNoteId = -1;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_testNoteId > 0)
            {
                dbService.DeleteNote(_testNoteId, DatabaseService.GetUserID("test1"));
            }
        }

        [TestMethod]
        [DataRow("test1", "Новая заметка", "новый текст")]
        public void TC_4_1_AuditCreateNote(string login, string title, string content)
        {
            var (success, message, noteId) = dbService.CreateNote(DatabaseService.GetUserID(login), title, content);
            _testNoteId = noteId ?? -1;

            Assert.IsTrue(success, "Заметка должна быть создана");
            DataRow auditRecord = dbService.GetLastAuditRecord(DatabaseService.GetUserID(login));
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("CREATE", auditRecord["action"].ToString(), "Тип действия должен быть CREATE");
            Trace.WriteLine(message);
        }

        [TestMethod]
        [DataRow("test1", "Тестовая заметка", "Изначальный текст", "Обновленный заголовок", "Обновленный текст")]
        public void TC_4_2_AuditUpdateNote(string login, string initialTitle, string initialContent, string updatedTitle, string updatedContent)
        {
            var (createSuccess, _, noteId) = dbService.CreateNote(DatabaseService.GetUserID(login), initialTitle, initialContent);
            Assert.IsTrue(createSuccess, "Не удалось создать тестовую заметку");
            _testNoteId = noteId ?? -1;

            var (updateSuccess, message) = dbService.UpdateNote(_testNoteId, DatabaseService.GetUserID(login), updatedTitle, updatedContent);

            Assert.IsTrue(updateSuccess, "Заметка должна быть обновлена");
            DataRow auditRecord = dbService.GetLastAuditRecord(DatabaseService.GetUserID(login));
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("EDIT", auditRecord["action"].ToString(), "Тип действия должен быть EDIT");
            Trace.WriteLine(message);
        }

        [TestMethod]
        [DataRow("test1", "Заметка для удаления", "Текст заметки")]
        public void TC_4_3_AuditDeleteNote(string login, string title, string content)
        {
            var (createSuccess, _, noteId) = dbService.CreateNote(DatabaseService.GetUserID(login), title, content);
            Assert.IsTrue(createSuccess, "Не удалось создать тестовую заметку");
            _testNoteId = noteId ?? -1;

            var (deleteSuccess, message) = dbService.DeleteNote(_testNoteId, DatabaseService.GetUserID(login));
            _testNoteId = -1;

            Assert.IsTrue(deleteSuccess, "Заметка должна быть удалена");
            DataRow auditRecord = dbService.GetLastAuditRecord(DatabaseService.GetUserID(login));
            Assert.IsNotNull(auditRecord, "Должна существовать запись аудита");
            Assert.AreEqual("DELETE", auditRecord["action"].ToString(), "Тип действия должен быть DELETE");
            Trace.WriteLine(message);
        }
    }
}
