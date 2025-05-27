using Npgsql;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace NotesApp
{
    /// <summary>
    /// Класс для работы с формой авторизации
    /// </summary>
    public partial class LoginForm : Form
    {
        /// <summary>
        /// Инициализация формы авторизации
        /// </summary>
        public LoginForm()
        {
            InitializeComponent();
            tbLogin.KeyDown += TextBox_KeyDown;
            tbPassword.KeyDown += TextBox_KeyDown;
        }

        /// <summary>
        /// Обработчик нажатия кнопки входа в систему
        /// Проверяет учетные данные пользователя и авторизует его при успешной проверке
        /// </summary>
        private void ButtonLogin_Click(object sender, EventArgs e)
        {
            string username = tbLogin.Text.Trim();
            string password = tbPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка ввода",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (DatabaseService dbService = new DatabaseService())
                {
                    // Аутентификация пользователя
                    var authenticatedUsername = dbService.AuthenticateUser(username, password);

                    if (authenticatedUsername.Success)
                    {
                        // Успешный вход
                        this.Hide();

                        // Получаем ID пользователя (может пригодиться для NotesForm)
                        int userId = DatabaseService.GetUserID(username);

                        NotesForm notesForm = new NotesForm(username);
                        notesForm.FormClosed += (s, args) => this.Close();
                        notesForm.Show();
                    }
                    else
                    {
                        // Неудачная аутентификация
                        MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                        tbPassword.Clear();
                        tbLogin.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при аутентификации: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик события нажатия клавиши в текстовом поле
        /// Выполняет вход при нажатии клавиши Enter
        /// </summary>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ButtonLogin_Click(null, null);
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Обработчик события наведения курсора на метку "Нет аккаунта"
        /// Изменяет внешний вид метки при наведении
        /// </summary>
        private void regLabel_MouseEnter(object sender, EventArgs e)
        {
            regLabel.ForeColor = Color.DarkCyan;
            regLabel.Font = new Font(regLabel.Font, FontStyle.Underline);
        }

        /// <summary>
        /// Обработчик события выхода курсора с метки "Нет аккаунта"
        /// Восстанавливает стандартный вид метки
        /// </summary>
        private void regLabel_MouseLeave(object sender, EventArgs e)
        {
            regLabel.ForeColor = SystemColors.ControlText;
            regLabel.Font = new Font(regLabel.Font, FontStyle.Regular);
        }

        /// <summary>
        /// Обработчик клика по метке "Нет аккаунта"
        /// Открывает форму регистрации и скрывает текущую форму
        /// </summary>
        private void regLabel_Click(object sender, EventArgs e)
        {
            RegistrationForm _registrationForm = new RegistrationForm(this);
            this.Hide();
            _registrationForm.Show();
        }
    }
}
