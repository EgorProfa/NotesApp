using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotesApp;
using System;
using System.Diagnostics;

namespace Test_Cases
{
    [TestClass]
    public class Module_NoteManage
    {
        private DatabaseService dbService;
        private int? lastNoteId;
        private string lastAuthorId;

        [TestInitialize]
        public void TestInitialize()
        {
            dbService = new DatabaseService();
            lastNoteId = null;
            lastAuthorId = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (lastNoteId.HasValue)
            {
                var (success, message) = dbService.DeleteNote(lastNoteId.Value, DatabaseService.GetUserID(lastAuthorId));
                Trace.WriteLine(success
                    ? $"Удалена тестовая заметка: ID {lastNoteId}"
                    : $"Ошибка удаления заметки: {message}");
            }
        }

        [DataTestMethod]
        [DataRow("test1", "Новая заметка", "новый текст", true, DisplayName = "Создание непустой заметки")]
        [DataRow("test1", "", "", false, DisplayName = "Создание пустой заметки")]
        public void TC_3_1_TestNoteCreation(string user, string title, string content, bool expectedSuccess)
        {
            var (success, message, noteId) = dbService.CreateNote(DatabaseService.GetUserID(user), title, content);

            if (success)
            {
                lastNoteId = noteId;
                lastAuthorId = user;
            } 

            Assert.AreEqual(expectedSuccess, success, message);
            Trace.WriteLine(message);
        }

        [DataTestMethod]
        [DataRow("test1", "Изменяемая заметка", "Текст", "Обновлённый заголовок", "Обновлённый текст")]
        public void TC_3_2_TestUpdateOwnNote(string user, string title, string content, string newTitle, string newContent)
        {
            var (created, msg, noteId) = dbService.CreateNote(DatabaseService.GetUserID(user), title, content);
            Assert.IsTrue(created, msg);

            var (success, message) = dbService.UpdateNote(noteId.Value, DatabaseService.GetUserID(user), newTitle, newContent);
            Assert.IsTrue(success, message);
            Trace.WriteLine(message);

            lastNoteId = noteId;
            lastAuthorId = user;
        }

        [DataTestMethod]
        [DataRow("test1", "test2", "Изменяемая заметка", "Текст", "Обновлённый заголовок", "Обновлённый текст")]
        public void TC_3_3_TestUpdateForeignNote(string author, string user, string title, string content, string newTitle, string newContent)
        {
            var (created, msg, noteId) = dbService.CreateNote(DatabaseService.GetUserID(author), title, content);
            Assert.IsTrue(created, msg);

            var (success, message) = dbService.UpdateNote(noteId.Value, DatabaseService.GetUserID(user), newTitle, newContent);
            Assert.IsTrue(success, message);
            Trace.WriteLine(message);

            lastNoteId = noteId;
            lastAuthorId = author;
        }

        [DataTestMethod]
        [DataRow("test1", "Удаляемая заметка", "Текст")]
        public void TC_3_4_TestDeleteOwnNote(string user, string title, string content)
        {
            var (created, msg, noteId) = dbService.CreateNote(DatabaseService.GetUserID(user), title, content);
            Assert.IsTrue(created, msg);

            var (success, message) = dbService.DeleteNote(noteId.Value, DatabaseService.GetUserID(user));
            Assert.IsTrue(success, message);
            Trace.WriteLine(message);

            lastNoteId = noteId;
            lastAuthorId = user;
        }

        [DataTestMethod]
        [DataRow("test1", "test2", "Удаляемая заметка", "Текст")]
        public void TC_3_5_TestDeleteForeignNote(string author, string user, string title, string content)
        {
            var (created, msg, noteId) = dbService.CreateNote(DatabaseService.GetUserID(author), title, content);
            Assert.IsTrue(created, msg);

            var (success, message) = dbService.DeleteNote(noteId.Value, DatabaseService.GetUserID(user));
            Assert.IsFalse(success, message);
            Trace.WriteLine(message);

            lastNoteId = noteId;
            lastAuthorId = author;
        }
    }
}
