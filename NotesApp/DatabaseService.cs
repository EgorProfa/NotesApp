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
                    "WHERE note_id = @note_id", databaseConnection))
                {
                    _updateNoteCommand.Parameters.AddWithValue("@note_id", _noteId);
                    //_updateNoteCommand.Parameters.AddWithValue("@author_id", _userId);
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
        /// Проверяет, соответствует ли имя пользователя требованиям
        /// </summary>
        /// <param name="_username">Имя пользователя для проверки</param>
        /// <returns>True если имя пользователя валидно, иначе False</returns>
        public bool ValidateUsername(string _username)
        {
            if (string.IsNullOrWhiteSpace(_username) || _username.Length < 5 || _username.Length > 20)
                return false;

            foreach (char _character in _username)
            {
                if (!(char.IsLetterOrDigit(_character) && IsLatinLetter(_character)))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Проверяет, является ли символ латинской буквой
        /// </summary>
        /// <param name="_character">Символ для проверки</param>
        /// <returns>True если символ латинский, иначе False</returns>
        private static bool IsLatinLetter(char _character)
        {
            return (_character >= 'A' && _character <= 'Z') || (_character >= 'a' && _character <= 'z');
        }

        /// <summary>
        /// Проверяет валидность пароля по правилам базы данных
        /// </summary>
        /// <param name="password">Пароль для проверки</param>
        /// <returns>True если пароль валиден, иначе False</returns>
        public bool ValidatePassword(string password)
        {
            using (NpgsqlCommand command = new NpgsqlCommand(
                "SELECT validate_password(@password)", databaseConnection))
            {
                command.Parameters.AddWithValue("@password", password);
                return (bool)command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Проверяет, существует ли пользователь с указанным логином
        /// </summary>
        /// <param name="username">Логин для проверки</param>
        /// <returns>True если пользователь существует, иначе False</returns>
        public bool UserExists(string username)
        {
            using (NpgsqlCommand command = new NpgsqlCommand(
                "SELECT COUNT(*) FROM users WHERE username = @username", databaseConnection))
            {
                command.Parameters.AddWithValue("@username", username);
                return (long)command.ExecuteScalar() > 0;
            }
        }

        /// <summary>
        /// Регистрирует нового пользователя
        /// </summary>
        /// <param name="username">Логин пользователя</param>
        /// <param name="password">Пароль пользователя</param>
        /// <returns>Кортеж с результатом операции (Success, Message)</returns>
        public (bool Success, string Message) RegisterUser(string username, string password)
        {
            try
            {
                using (NpgsqlCommand command = new NpgsqlCommand(
                    "INSERT INTO users (username, password_hash) VALUES (@username, crypt(@password, gen_salt('bf')))",
                    databaseConnection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var transaction = databaseConnection.BeginTransaction())
                    {
                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            transaction.Commit();

                            return rowsAffected > 0
                                ? (true, "Вы успешно зарегистрировались!")
                                : (false, "Не удалось зарегистрировать пользователя");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            return (false, $"Ошибка при регистрации: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        /// <summary>
        /// Проверяет учетные данные пользователя
        /// </summary>
        /// <param name="username">Имя пользователя</param>
        /// <param name="password">Пароль</param>
        /// <returns>Имя пользователя при успешной аутентификации, иначе null</returns>
        public string AuthenticateUser(string username, string password)
        {
            using (NpgsqlCommand command = new NpgsqlCommand(
                "SELECT username FROM users WHERE username = @username AND password_hash = crypt(@password, password_hash)",
                databaseConnection))
            {
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                var result = command.ExecuteScalar();
                return result != null ? Convert.ToString(result) : null;
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
