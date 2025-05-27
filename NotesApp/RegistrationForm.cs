using DatabaseLibrary;
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
        /// Обработчик нажатия кнопки регистрации
        /// </summary>
        private void ButtonReg_Click(object sender, EventArgs e)
        {
            // Получаем данные из полей ввода
            string login = tbRegLogin.Text.Trim();
            string password = tbRegPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (DatabaseService dbService = new DatabaseService())
                {
                    // 1. Валидация логина
                    if (dbService.ValidateUsername(login))
                    {
                        MessageBox.Show("Логин должен содержать от 5 до 20 символов латинского алфавита и цифр",
                                     "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2. Проверка существования пользователя
                    if (dbService.UserExists(login))
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует",
                                     "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 3. Валидация пароля
                    if (!dbService.ValidatePassword(password))
                    {
                        MessageBox.Show("Пароль должен содержать:\n- Минимум 8 символов\n- Заглавные и строчные буквы\n- Цифры\n- Специальные символы",
                                     "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 4. Регистрация пользователя
                    var (success, message) = dbService.RegisterUser(login, password);

                    if (success)
                    {
                        // Успешная регистрация
                        MessageBox.Show(message, "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        loginForm.Show();
                        this.Close();
                    }
                    else
                    {
                        // Ошибка регистрации
                        MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Непредвиденная ошибка: {ex.Message}",
                               "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
