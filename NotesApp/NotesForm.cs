using System;
using System.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace NotesApp
{
    /// <summary>
    /// Форма для работы с заметками
    /// </summary>
    public partial class NotesForm : Form
    {
        private string userName;
        private int userId;
        private string sessionId;

        /// <summary>
        /// Инициализация формы работы с заметками
        /// </summary>
        /// <param name="_userName">Имя пользователя, который работает с заметками</param>
        public NotesForm(string _userName)
        {
            InitializeComponent();

            this.userName = _userName;
            this.Text = $"Работа с заметками (пользователь: {userName})";

            try
            {
                using (DatabaseService dbService = new DatabaseService())
                {
                    // Получаем ID пользователя
                    userId = DatabaseService.GetUserID(userName);
                    if (userId == -1)
                    {
                        MessageBox.Show("Ошибка получения данных пользователя", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                        return;
                    }

                    // Создаем новую сессию
                    sessionId = Guid.NewGuid().ToString();
                    var (sessionSuccess, sessionMessage) = dbService.CreateSession(sessionId, userId);

                    if (!sessionSuccess)
                    {
                        MessageBox.Show(sessionMessage, "Ошибка сессии",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                        return;
                    }

                    // Загрузка заметок
                    LoadNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        /// <summary>
        /// Обработчик события закрытия формы
        /// </summary>
        private void NotesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                using (DatabaseService dbService = new DatabaseService())
                {
                    var (success, message) = dbService.DeleteSession(sessionId);

                    if (!success)
                    {
                        Debug.WriteLine($"Ошибка при закрытии сессии: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при закрытии формы: {ex.Message}");
            }
        }

        private DataTable notesData;
        private int? currentNoteId = null;
        private int currentIndex = -1;

        /// <summary>
        /// Загружает заметки из базы данных с возможностью поиска
        /// </summary>
        /// <param name="_searchTerm">Термин для поиска заметок</param>
        /// <summary>
        /// Загружает заметки из базы данных с возможностью поиска
        /// </summary>
        /// <param name="searchTerm">Термин для поиска заметок</param>
        private void LoadNotes(string searchTerm = null)
        {
            try
            {
                // Очистка таблицы перед загрузкой новых данных
                notesTable.DataSource = null;
                notesTable.Columns.Clear();

                using (DatabaseService dbService = new DatabaseService())
                {
                    // Получение данных через DatabaseService
                    notesData = dbService.GetNotes(searchTerm);

                    if (notesData == null)
                    {
                        throw new Exception("Не удалось получить данные из базы");
                    }

                    // Настройка DataGridView
                    ConfigureDataGridView();

                    // Установка текущей заметки
                    if (notesData.Rows.Count > 0)
                    {
                        currentIndex = 0;
                        ShowCurrentNote();
                    }
                    else
                    {
                        ClearNoteFields();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке заметок: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ClearNoteFields();
            }
        }

        /// <summary>
        /// Настраивает DataGridView для отображения заметок
        /// </summary>
        private void ConfigureDataGridView()
        {
            AddColumn("username", "Автор", 150);
            AddColumn("title", "Заголовок", 200);
            AddColumn("created_at", "Дата создания", 145, "dd.MM.yyyy HH:mm");
            AddColumn("last_changed_at", "Дата изменения", 150, "dd.MM.yyyy HH:mm");

            AddHiddenColumn("note_id");
            AddHiddenColumn("author_id");
            AddHiddenColumn("content");

            notesTable.DataSource = notesData;
        }

        /// <summary>
        /// Добавляет видимую колонку в DataGridView
        /// </summary>
        /// <param name="_dataProperty">Имя свойства данных</param>
        /// <param name="_header">Заголовок колонки</param>
        /// <param name="_width">Ширина колонки</param>
        /// <param name="_format">Формат отображения данных</param>
        private void AddColumn(string _dataProperty, string _header, int _width, string _format = null)
        {
            var col = new DataGridViewTextBoxColumn
            {
                DataPropertyName = _dataProperty,
                HeaderText = _header,
                Name = "col" + _dataProperty,
                Width = _width
            };

            if (_format != null)
            {
                col.DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = _format,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };
            }

            notesTable.Columns.Add(col);
        }

        /// <summary>
        /// Добавляет скрытую колонку в DataGridView
        /// </summary>
        /// <param name="_dataProperty">Имя свойства данных</param>
        private void AddHiddenColumn(string _dataProperty)
        {
            notesTable.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = _dataProperty,
                Name = "col" + _dataProperty,
                Visible = false
            });
        }

        /// <summary>
        /// Выделяет текущую строку в DataGridView
        /// </summary>
        private void SelectCurrentRow()
        {
            if (currentIndex >= 0 && currentIndex < notesTable.Rows.Count)
            {
                // Снимаем текущее выделение
                notesTable.ClearSelection();

                // Устанавливаем текущую ячейку (столбец "username")
                notesTable.CurrentCell = notesTable.Rows[currentIndex].Cells["colusername"];
            }
        }

        /// <summary>
        /// Отображает текущую выбранную заметку
        /// </summary>
        private void ShowCurrentNote()
        {
            if (currentIndex >= 0 && currentIndex < notesData.Rows.Count)
            {
                var _row = notesData.Rows[currentIndex];
                currentNoteId = (int)_row["note_id"];
                tbNoteTitle.Text = _row["title"].ToString();
                tbNoteText.Text = _row["content"].ToString().Replace("\n", "\r\n");
                UpdateButtonStates();
                UpdateNavButtons();
            }
        }

        /// <summary>
        /// Очищает поля ввода заметки
        /// </summary>
        private void ClearNoteFields()
        {
            tbNoteTitle.Clear();
            tbNoteText.Clear();
            currentNoteId = null;
            currentIndex = -1;
            UpdateButtonStates();
            UpdateNavButtons();
        }

        /// <summary>
        /// Обновляет состояние кнопок в зависимости от текущего выбора
        /// </summary>
        private void UpdateButtonStates()
        {
            bool _isNoteSelected = currentIndex >= 0 && currentIndex < notesData?.Rows.Count;
            bool _isAuthor = false;

            if (_isNoteSelected)
            {
                var _row = notesData.Rows[currentIndex];
                int _authorId = (int)_row["author_id"];
                _isAuthor = _authorId == userId;
            }

            btnDelete.Enabled = _isNoteSelected && _isAuthor;
            btnSave.Enabled = _isNoteSelected || currentNoteId == null;
        }

        /// <summary>
        /// Обновляет состояние кнопок навигации
        /// </summary>
        private void UpdateNavButtons()
        {
            bool _hasNotes = notesData != null && notesData.Rows.Count > 0;
            btnPrevious.Enabled = _hasNotes && currentIndex > 0;
            btnNext.Enabled = _hasNotes && currentIndex < notesData.Rows.Count - 1;
        }

        /// <summary>
        /// Обработчик клика по кнопке "Следующая заметка"
        /// </summary>
        private void ButtonNext_Click(object sender, EventArgs e)
        {
            if (notesData == null || notesData.Rows.Count == 0) return;

            if (currentIndex < notesData.Rows.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
            }
            SelectCurrentRow();
            ShowCurrentNote();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Предыдущая заметка"
        /// </summary>
        private void ButtonPrevious_Click(object sender, EventArgs e)
        {
            if (notesData == null || notesData.Rows.Count == 0) return;

            if (currentIndex > 0)
            {
                currentIndex--;
            }
            else
            {
                currentIndex = notesData.Rows.Count - 1;
            }
            SelectCurrentRow();
            ShowCurrentNote();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Поиск"
        /// </summary>
        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            LoadNotes(tbSearch.Text.Trim());
        }

        /// <summary>
        /// Обработчик клика по кнопке "Сброс"
        /// </summary>
        private void ButtonReset_Click(object sender, EventArgs e)
        {
            tbSearch.Clear();
            LoadNotes();
            ClearNoteFields();
        }

        /// <summary>
        /// Обработчик двойного клика по строке таблицы заметок
        /// </summary>
        private void notesTable_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            currentIndex = e.RowIndex;
            ShowCurrentNote();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Создать"
        /// </summary>
        private void ButtonCreate_Click(object sender, EventArgs e)
        {
            tbNoteTitle.Clear();
            tbNoteText.Clear();
            currentNoteId = null;
            currentIndex = -1;
            UpdateButtonStates();
            UpdateNavButtons();
        }

        /// <summary>
        /// Обработчик клика по кнопке "Сохранить"
        /// </summary>
        private void ButtonSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNoteTitle.Text))
            {
                MessageBox.Show("Введите заголовок заметки", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (DatabaseService _databaseService = new DatabaseService())
            {
                string _content = tbNoteText.Text.Replace("\r\n", "\n");

                if (currentNoteId == null)
                {
                    var _result = _databaseService.CreateNote(userId, tbNoteTitle.Text, _content);
                    if (_result.Success)
                    {
                        MessageBox.Show("Заметка успешно создана", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        currentNoteId = _result.NoteId;
                        LoadNotes(tbSearch.Text.Trim());
                    }
                    else
                    {
                        MessageBox.Show(_result.Message, "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var _result = _databaseService.UpdateNote(currentNoteId.Value, userId, tbNoteTitle.Text, _content);
                    MessageBox.Show(_result.Message, _result.Success ? "Успех" : "Ошибка", MessageBoxButtons.OK, _result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    LoadNotes(tbSearch.Text.Trim());
                }
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке "Удалить"
        /// </summary>
        private void ButtonDelete_Click(object sender, EventArgs e)
        {
            if (currentNoteId == null || currentIndex < 0) return;

            DialogResult _confirmResult = MessageBox.Show("Вы уверены, что хотите удалить эту заметку?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (_confirmResult == DialogResult.Yes)
            {
                using (DatabaseService _databaseService = new DatabaseService())
                {
                    var _result = _databaseService.DeleteNote(currentNoteId.Value, userId);
                    MessageBox.Show(_result.Message, _result.Success ? "Успех" : "Ошибка", MessageBoxButtons.OK, _result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Error);

                    if (_result.Success)
                    {
                        LoadNotes(tbSearch.Text.Trim());
                    }
                }
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке "Закрыть"
        /// </summary>
        private void ButtonClose_Click(object sender, EventArgs e)
        {
            try
            {
                using (DatabaseService dbService = new DatabaseService())
                {
                    var (success, message) = dbService.DeleteSession(sessionId);

                    if (!success)
                    {
                        Debug.WriteLine($"Ошибка при закрытии сессии: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при закрытии формы: {ex.Message}");
            }

            LoginForm _loginForm = new LoginForm();
            _loginForm.FormClosed += (s, args) => this.Close();
            _loginForm.Show();
            this.Hide();
        }

        /// <summary>
        /// Загрузка заметки при нажатии клавиши Enter
        /// </summary>
        private void notesTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                currentIndex = notesTable.CurrentRow.Index;
                ShowCurrentNote();
            }
        }
    }
}