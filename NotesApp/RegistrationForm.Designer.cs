namespace NotesApp
{
    partial class RegistrationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lbBackToLogin = new System.Windows.Forms.Label();
            this.tbRegPassword = new System.Windows.Forms.TextBox();
            this.tbRegLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnReg = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbBackToLogin
            // 
            this.lbBackToLogin.AutoSize = true;
            this.lbBackToLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbBackToLogin.Location = new System.Drawing.Point(62, 272);
            this.lbBackToLogin.Name = "lbBackToLogin";
            this.lbBackToLogin.Size = new System.Drawing.Size(180, 18);
            this.lbBackToLogin.TabIndex = 22;
            this.lbBackToLogin.Text = "У меня уже есть аккаунт";
            this.lbBackToLogin.Click += new System.EventHandler(this.lbBackToLogin_Click);
            this.lbBackToLogin.MouseEnter += new System.EventHandler(this.loginLabel_MouseEnter);
            this.lbBackToLogin.MouseLeave += new System.EventHandler(this.loginLabel_MouseLeave);
            // 
            // tbRegPassword
            // 
            this.tbRegPassword.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.tbRegPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRegPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbRegPassword.Location = new System.Drawing.Point(59, 177);
            this.tbRegPassword.Name = "tbRegPassword";
            this.tbRegPassword.PasswordChar = '*';
            this.tbRegPassword.Size = new System.Drawing.Size(183, 26);
            this.tbRegPassword.TabIndex = 21;
            // 
            // tbRegLogin
            // 
            this.tbRegLogin.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.tbRegLogin.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbRegLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbRegLogin.Location = new System.Drawing.Point(59, 86);
            this.tbRegLogin.Name = "tbRegLogin";
            this.tbRegLogin.Size = new System.Drawing.Size(183, 26);
            this.tbRegLogin.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(55, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 20);
            this.label2.TabIndex = 19;
            this.label2.Text = "ПАРОЛЬ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(55, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 20);
            this.label1.TabIndex = 18;
            this.label1.Text = "ЛОГИН";
            // 
            // btnReg
            // 
            this.btnReg.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.btnReg.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnReg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReg.Location = new System.Drawing.Point(59, 222);
            this.btnReg.Name = "btnReg";
            this.btnReg.Size = new System.Drawing.Size(183, 36);
            this.btnReg.TabIndex = 17;
            this.btnReg.Text = "ЗАРЕГИСТРИРОВАТЬСЯ";
            this.btnReg.UseVisualStyleBackColor = false;
            this.btnReg.Click += new System.EventHandler(this.ButtonReg_Click);
            // 
            // RegistrationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightYellow;
            this.ClientSize = new System.Drawing.Size(304, 358);
            this.Controls.Add(this.lbBackToLogin);
            this.Controls.Add(this.tbRegPassword);
            this.Controls.Add(this.tbRegLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnReg);
            this.MaximizeBox = false;
            this.Name = "RegistrationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Регистрация";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RegistrationForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbBackToLogin;
        private System.Windows.Forms.TextBox tbRegPassword;
        private System.Windows.Forms.TextBox tbRegLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnReg;
    }
}