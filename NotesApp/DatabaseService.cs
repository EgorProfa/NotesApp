using Npgsql;
using System;
using System.Windows.Forms;

namespace NotesApp
{
    /// <summary>
    /// Класс для работы с базой данных PostgreSQL
    /// </summary>
    public class DatabaseService : IDisposable
    {
        private static NpgsqlConnection databaseConnection;

        /// <summary>
        /// Конструктор класса DatabaseService
        /// Устанавливает соединение с базой данных
        /// </summary>
        public DatabaseService()
        {
            databaseConnection = new NpgsqlConnection("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=6Gwyw4e7");
            databaseConnection.Open();
        }

        /// <summary>
        /// Получает активное соединение с базой данных
        /// </summary>
        /// <returns>Активное соединение NpgsqlConnection</returns>
        public NpgsqlConnection GetConnection()
        {
            return databaseConnection;
        }

        /// <summary>
        /// Получает имя пользователя по его идентификатору
        /// </summary>
        /// <param name="_userId">Идентификатор пользователя</param>
        /// <returns>Имя пользователя или null, если пользователь не найден</returns>
        public static int GetUserID(string _userName)
        {
            try
            {
                using (NpgsqlCommand _usernameCommand = new NpgsqlCommand("SELECT user_id FROM users WHERE username = @username", databaseConnection))
                {
                    _usernameCommand.Parameters.AddWithValue("@username", _userName);
                    return Convert.ToInt32(_usernameCommand.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }

        /// <summary>
        /// Создает новую заметку
        /// </summary>
        /// <param name="_authorId">Идентификатор автора</param>
        /// <param name="_title">Заголовок заметки</param>
        /// <param name="_content">Содержимое заметки</param>
        /// <returns>Кортеж с результатом операции (Success, Message, NoteId)</returns>
        public (bool Success, string Message, int? NoteId) CreateNote(int _authorId, string _title, string _content)
        {
            try
            {
                using (NpgsqlCommand _createNoteCommand = new NpgsqlCommand(
                    "INSERT INTO notes (author_id, title, content) " +
                    "VALUES (@author_id, @title, @content) RETURNING note_id", databaseConnection))
                {
                    _createNoteCommand.Parameters.AddWithValue("@author_id", _authorId);
                    _createNoteCommand.Parameters.AddWithValue("@title", _title);
                    _createNoteCommand.Parameters.AddWithValue("@content", _content);

                    int _newNoteId = (int)_createNoteCommand.ExecuteScalar();
                    return (true, "Заметка успешно создана", _newNoteId);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при создании заметки: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Обновляет существующую заметку
        /// </summary>
        /// <param name="_noteId">Идентификатор заметки</param>
        /// <param name="_userId">Идентификатор пользователя</param>
        /// <param name="_title">Новый заголовок</param>
        /// <param name="_content">Новое содержимое</param>
        /// <returns>Кортеж с результатом операции (Success, Message)</returns>
        public (bool Success, string Message) UpdateNote(int _noteId, int _userId, string _title, string _content)
        {
            try
            {
                using (NpgsqlCommand _updateNoteCommand = new NpgsqlCommand(
                    "UPDATE notes SET title = @title, content = @content, " +
                    "last_changed_at = CURRENT_TIMESTAMP " +
                    "WHERE note_id = @note_id AND author_id = @author_id", databaseConnection))
                {
                    _updateNoteCommand.Parameters.AddWithValue("@note_id", _noteId);
                    _updateNoteCommand.Parameters.AddWithValue("@author_id", _userId);
                    _updateNoteCommand.Parameters.AddWithValue("@title", _title);
                    _updateNoteCommand.Parameters.AddWithValue("@content", _content);

                    int _affectedRows = _updateNoteCommand.ExecuteNonQuery();
                    return _affectedRows > 0 ? (true, "Заметка успешно обновлена") : (false, "Заметка не найдена");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при обновлении заметки: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаляет заметку
        /// </summary>
        /// <param name="_noteId">Идентификатор заметки</param>
        /// <param name="_userId">Идентификатор пользователя</param>
        /// <returns>Кортеж с результатом операции (Success, Message)</returns>
        public (bool Success, string Message) DeleteNote(int _noteId, int _userId)
        {
            try
            {
                using (NpgsqlCommand _deleteNoteCommand = new NpgsqlCommand(
                    "DELETE FROM notes WHERE note_id = @note_id AND author_id = @author_id", databaseConnection))
                {
                    _deleteNoteCommand.Parameters.AddWithValue("@note_id", _noteId);
                    _deleteNoteCommand.Parameters.AddWithValue("@author_id", _userId);

                    int _affectedRows = _deleteNoteCommand.ExecuteNonQuery();
                    return _affectedRows > 0 ? (true, "Заметка успешно удалена") : (false, "Заметка не найдена");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при удалении заметки: {ex.Message}");
            }
        }

        /// <summary>
        /// Реализация интерфейса IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed = false;
        /// <summary>
        /// Защищенный метод для освобождения ресурсов
        /// </summary>
        /// <param name="_disposing">Флаг, указывающий на необходимость освобождения управляемых ресурсов</param>
        protected virtual void Dispose(bool _disposing)
        {
            if (!isDisposed)
            {
                if (_disposing)
                {
                    databaseConnection?.Close();
                    databaseConnection?.Dispose();
                }
                isDisposed = true;
            }
        }
    }
}
