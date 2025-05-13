using Npgsql;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NotesApp
{
    /// <summary>
    /// Форма регистрации
    /// </summary>
    public partial class RegistrationForm : Form
    {
        private LoginForm loginForm;

        /// <summary>
        /// Инициализация формы регистрации
        /// </summary>
        /// <param name="_loginForm">Форма авторизации</param>
        public RegistrationForm(LoginForm _loginForm)
        {
            InitializeComponent();
            this.loginForm = _loginForm;
            tbRegLogin.KeyDown += TextBox_KeyDown;
            tbRegPassword.KeyDown += TextBox_KeyDown;
        }

        /// <summary>
        /// Возвращение к форме авторизации
        /// </summary>
        private void RegistrationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            loginForm.Show();
        }

        /// <summary>
        /// Обработчик клика по метке "У меня уже есть аккаунт"
        /// Открывает форму авторизации и скрывает текущую форму
        /// </summary>
        private void lbBackToLogin_Click(object sender, EventArgs e)
        {
            loginForm.Show();
            this.Close();
        }

        /// <summary>
        /// Обработчик события наведения курсора на метку "У меня уже есть аккаунт"
        /// Изменяет внешний вид метки при наведении
        /// </summary>
        private void loginLabel_MouseEnter(object sender, EventArgs e)
        {
            lbBackToLogin.ForeColor = Color.DarkCyan;
            lbBackToLogin.Font = new Font(lbBackToLogin.Font, FontStyle.Underline);
        }

        /// <summary>
        /// Обработчик события выхода курсора с метки "У меня уже есть аккаунт"
        /// Восстанавливает стандартный вид метки
        /// </summary>
        private void loginLabel_MouseLeave(object sender, EventArgs e)
        {
            lbBackToLogin.ForeColor = SystemColors.ControlText;
            lbBackToLogin.Font = new Font(lbBackToLogin.Font, FontStyle.Regular);
        }

        /// <summary>
        /// Обработчик события нажатия клавиши в текстовом поле
        /// Выполняет регистрацию при нажатии клавиши Enter
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonReg_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Проверяет, соответствует ли имя пользователя требованиям
        /// </summary>
        /// <param name="_username">Имя пользователя для проверки</param>
        /// <returns>True если имя пользователя валидно, иначе False</returns>
        public static bool ValidateUsername(string _username)
        {
            if (string.IsNullOrWhiteSpace(_username))
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
        /// Обработчик нажатия кнопки регистрации
        /// </summary>
        private void ButtonReg_Click(object sender, EventArgs e)
        {
            string _login = tbRegLogin.Text.Trim();
            string _password = tbRegPassword.Text;

            if (string.IsNullOrWhiteSpace(_login) || _login.Length < 5 || _login.Length > 20 || ValidateUsername(_login))
            {
                MessageBox.Show("Логин должен содержать от 5 до 20 символов латинского алфавита и цифр", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (DatabaseService _databaseService = new DatabaseService())
                {
                    // 1. Проверка валидности пароля
                    using (NpgsqlCommand _checkPasswordCommand = new NpgsqlCommand(
                        "SELECT validate_password(@password)", _databaseService.GetConnection()))
                    {
                        _checkPasswordCommand.Parameters.AddWithValue("@password", _password);
                        bool _isPasswordValid = (bool)_checkPasswordCommand.ExecuteScalar();

                        if (!_isPasswordValid)
                        {
                            MessageBox.Show("Пароль не соответствует требованиям.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // 2. Проверка уникальности логина
                    using (NpgsqlCommand _checkUsernameCommand = new NpgsqlCommand(
                        "SELECT COUNT(*) FROM users WHERE username = @username", _databaseService.GetConnection()))
                    {
                        _checkUsernameCommand.Parameters.AddWithValue("@username", _login);
                        long _userCount = (long)_checkUsernameCommand.ExecuteScalar();

                        if (_userCount > 0)
                        {
                            MessageBox.Show("Пользователь с таким логином уже существует", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // 3. Регистрация пользователя
                    using (NpgsqlCommand _insertCommand = new NpgsqlCommand(
                        "INSERT INTO users (username, password_hash) VALUES (@username, crypt(@password, gen_salt('bf')))",
                        _databaseService.GetConnection()))
                    {
                        _insertCommand.Parameters.AddWithValue("@username", _login);
                        _insertCommand.Parameters.AddWithValue("@password", _password);

                        using (var _transaction = _databaseService.GetConnection().BeginTransaction())
                        {
                            try
                            {
                                int _rowsAffected = _insertCommand.ExecuteNonQuery();
                                _transaction.Commit();

                                if (_rowsAffected > 0)
                                {
                                    loginForm.Show();
                                    this.Close();
                                    MessageBox.Show("Вы успешно зарегистрировались!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            catch
                            {
                                _transaction.Rollback();
                                throw;
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
