using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            userName = _userName;
            userId = DatabaseService.GetUserID(userName);
            sessionId = Guid.NewGuid().ToString();
            this.Text = $"Работа с заметками (пользователь: {_userName})";

            // Создаем новую сессию для пользователя
            if (!NewSession())
            {
                MessageBox.Show("Вы уже вошли в систему с другого устройства", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            // Загрузка заметок в dataGridView
            LoadNotes();
        }

        /// <summary>
        /// Обработчик события закрытия формы
        /// </summary>
        private void NotesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterSession();
        }

        /// <summary>
        /// Создает новую сессию пользователя
        /// </summary>
        /// <returns>True если сессия создана успешно, иначе False</returns>
        private bool NewSession()
        {
            try
            {
                using (DatabaseService _databaseService = new DatabaseService())
                {
                    using (NpgsqlCommand _сmd = new NpgsqlCommand(
                        "INSERT INTO active_sessions (session_id, user_id, login_time) " +
                        "VALUES (@session_id, @user_id, CURRENT_TIMESTAMP)",
                        _databaseService.GetConnection()))
                    {
                        _сmd.Parameters.AddWithValue("@session_id", sessionId);
                        _сmd.Parameters.AddWithValue("@user_id", userId);
                        _сmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации сессии: {ex.Message}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Удаляет сессию пользователя
        /// </summary>
        /// <returns>True если сессия удалена успешно, иначе False</returns>
        private bool UnregisterSession()
        {
            try
            {
                using (DatabaseService _databaseService = new DatabaseService())
                {
                    using (NpgsqlCommand _cmd = new NpgsqlCommand(
                        "DELETE FROM active_sessions WHERE session_id = @session_id",
                        _databaseService.GetConnection()))
                    {
                        _cmd.Parameters.AddWithValue("@session_id", sessionId);
                        int _affectedRows = _cmd.ExecuteNonQuery();
                        return _affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка удаления сессии: {ex.Message}");
                return false;
            }
        }

        private DataTable notesData;
        private int? currentNoteId = null;
        private int currentIndex = -1;

        /// <summary>
        /// Загружает заметки из базы данных с возможностью поиска
        /// </summary>
        /// <param name="_searchTerm">Термин для поиска заметок</param>
        private void LoadNotes(string _searchTerm = null)
        {
            try
            {
                notesTable.DataSource = null;
                notesTable.Columns.Clear();

                using (DatabaseService _databaseService = new DatabaseService())
                {
                    notesData = new DataTable();
                    using (var _cmd = new NpgsqlCommand())
                    {
                        _cmd.Connection = _databaseService.GetConnection();
                        _cmd.CommandText = @"SELECT n.note_id, n.title, n.content, n.created_at, n.last_changed_at, u.username, n.author_id FROM notes n JOIN users u ON n.author_id = u.user_id";

                        if (!string.IsNullOrWhiteSpace(_searchTerm))
                        {
                            _cmd.CommandText += @" WHERE LOWER(n.title) LIKE @search 
                                               OR LOWER(u.username) LIKE @search";
                            _cmd.Parameters.AddWithValue("@search", $"%{_searchTerm.ToLower()}%");
                        }

                        _cmd.CommandText += " ORDER BY n.note_id";

                        using (NpgsqlDataAdapter _adapter = new NpgsqlDataAdapter(_cmd))
                        {
                            _adapter.Fill(notesData);
                        }
                    }

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
                UnregisterSession();

                LoginForm _loginForm = new LoginForm();
                _loginForm.FormClosed += (s, args) => this.Close();
                _loginForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе из системы: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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