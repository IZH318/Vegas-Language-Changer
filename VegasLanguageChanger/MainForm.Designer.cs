namespace VegasLanguageChanger
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStripMain = new System.Windows.Forms.MenuStrip();
            this.lblInfo = new System.Windows.Forms.Label();
            this.grpAvailable = new System.Windows.Forms.GroupBox();
            this.lstAvailable = new System.Windows.Forms.ListBox();
            this.grpSelected = new System.Windows.Forms.GroupBox();
            this.lstSelected = new System.Windows.Forms.ListBox();
            this.btnMoveRight = new System.Windows.Forms.Button();
            this.btnMoveAllRight = new System.Windows.Forms.Button();
            this.btnMoveLeft = new System.Windows.Forms.Button();
            this.btnMoveAllLeft = new System.Windows.Forms.Button();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.btnChangeLanguage = new System.Windows.Forms.Button();
            this.grpAvailable.SuspendLayout();
            this.grpSelected.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStripMain
            // 
            this.menuStripMain.Location = new System.Drawing.Point(0, 0);
            this.menuStripMain.Name = "menuStripMain";
            this.menuStripMain.Size = new System.Drawing.Size(584, 24);
            this.menuStripMain.TabIndex = 0;
            this.menuStripMain.Text = "menuStrip1";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(12, 36);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(38, 12);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "label1";
            // 
            // grpAvailable
            // 
            this.grpAvailable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpAvailable.Controls.Add(this.lstAvailable);
            this.grpAvailable.Location = new System.Drawing.Point(12, 63);
            this.grpAvailable.Name = "grpAvailable";
            this.grpAvailable.Size = new System.Drawing.Size(260, 203);
            this.grpAvailable.TabIndex = 2;
            this.grpAvailable.TabStop = false;
            this.grpAvailable.Text = "groupBox1";
            // 
            // lstAvailable
            // 
            this.lstAvailable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstAvailable.FormattingEnabled = true;
            this.lstAvailable.ItemHeight = 12;
            this.lstAvailable.Location = new System.Drawing.Point(3, 17);
            this.lstAvailable.Name = "lstAvailable";
            this.lstAvailable.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstAvailable.Size = new System.Drawing.Size(254, 183);
            this.lstAvailable.TabIndex = 0;
            this.lstAvailable.DoubleClick += new System.EventHandler(this.lstAvailable_DoubleClick);
            // 
            // grpSelected
            // 
            this.grpSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSelected.Controls.Add(this.lstSelected);
            this.grpSelected.Location = new System.Drawing.Point(312, 63);
            this.grpSelected.Name = "grpSelected";
            this.grpSelected.Size = new System.Drawing.Size(260, 203);
            this.grpSelected.TabIndex = 3;
            this.grpSelected.TabStop = false;
            this.grpSelected.Text = "groupBox1";
            // 
            // lstSelected
            // 
            this.lstSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstSelected.FormattingEnabled = true;
            this.lstSelected.ItemHeight = 12;
            this.lstSelected.Location = new System.Drawing.Point(3, 17);
            this.lstSelected.Name = "lstSelected";
            this.lstSelected.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSelected.Size = new System.Drawing.Size(254, 183);
            this.lstSelected.TabIndex = 0;
            this.lstSelected.DoubleClick += new System.EventHandler(this.lstSelected_DoubleClick);
            // 
            // btnMoveRight
            // 
            this.btnMoveRight.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMoveRight.Location = new System.Drawing.Point(275, 106);
            this.btnMoveRight.Name = "btnMoveRight";
            this.btnMoveRight.Size = new System.Drawing.Size(31, 23);
            this.btnMoveRight.TabIndex = 4;
            this.btnMoveRight.Text = ">";
            this.btnMoveRight.UseVisualStyleBackColor = true;
            this.btnMoveRight.Click += new System.EventHandler(this.btnMoveRight_Click);
            // 
            // btnMoveAllRight
            // 
            this.btnMoveAllRight.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMoveAllRight.Location = new System.Drawing.Point(275, 135);
            this.btnMoveAllRight.Name = "btnMoveAllRight";
            this.btnMoveAllRight.Size = new System.Drawing.Size(31, 23);
            this.btnMoveAllRight.TabIndex = 5;
            this.btnMoveAllRight.Text = ">>";
            this.btnMoveAllRight.UseVisualStyleBackColor = true;
            this.btnMoveAllRight.Click += new System.EventHandler(this.btnMoveAllRight_Click);
            // 
            // btnMoveLeft
            // 
            this.btnMoveLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMoveLeft.Location = new System.Drawing.Point(275, 164);
            this.btnMoveLeft.Name = "btnMoveLeft";
            this.btnMoveLeft.Size = new System.Drawing.Size(31, 23);
            this.btnMoveLeft.TabIndex = 6;
            this.btnMoveLeft.Text = "<";
            this.btnMoveLeft.UseVisualStyleBackColor = true;
            this.btnMoveLeft.Click += new System.EventHandler(this.btnMoveLeft_Click);
            // 
            // btnMoveAllLeft
            // 
            this.btnMoveAllLeft.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnMoveAllLeft.Location = new System.Drawing.Point(275, 193);
            this.btnMoveAllLeft.Name = "btnMoveAllLeft";
            this.btnMoveAllLeft.Size = new System.Drawing.Size(31, 23);
            this.btnMoveAllLeft.TabIndex = 7;
            this.btnMoveAllLeft.Text = "<<";
            this.btnMoveAllLeft.UseVisualStyleBackColor = true;
            this.btnMoveAllLeft.Click += new System.EventHandler(this.btnMoveAllLeft_Click);
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLanguage.FormattingEnabled = true;
            this.cmbLanguage.Location = new System.Drawing.Point(12, 281);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(469, 20);
            this.cmbLanguage.TabIndex = 8;
            this.cmbLanguage.SelectionChangeCommitted += new System.EventHandler(this.cmbLanguage_SelectionChangeCommitted);
            // 
            // btnChangeLanguage
            // 
            this.btnChangeLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeLanguage.Location = new System.Drawing.Point(487, 279);
            this.btnChangeLanguage.Name = "btnChangeLanguage";
            this.btnChangeLanguage.Size = new System.Drawing.Size(85, 23);
            this.btnChangeLanguage.TabIndex = 9;
            this.btnChangeLanguage.Text = "button1";
            this.btnChangeLanguage.UseVisualStyleBackColor = true;
            this.btnChangeLanguage.Click += new System.EventHandler(this.btnChangeLanguage_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 321);
            this.Controls.Add(this.btnChangeLanguage);
            this.Controls.Add(this.cmbLanguage);
            this.Controls.Add(this.btnMoveAllLeft);
            this.Controls.Add(this.btnMoveLeft);
            this.Controls.Add(this.btnMoveAllRight);
            this.Controls.Add(this.btnMoveRight);
            this.Controls.Add(this.grpSelected);
            this.Controls.Add(this.grpAvailable);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.menuStripMain);
            this.MainMenuStrip = this.menuStripMain;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 360);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VegasLanguageChanger";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.grpAvailable.ResumeLayout(false);
            this.grpSelected.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStripMain;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.GroupBox grpAvailable;
        private System.Windows.Forms.ListBox lstAvailable;
        private System.Windows.Forms.GroupBox grpSelected;
        private System.Windows.Forms.ListBox lstSelected;
        private System.Windows.Forms.Button btnMoveRight;
        private System.Windows.Forms.Button btnMoveAllRight;
        private System.Windows.Forms.Button btnMoveLeft;
        private System.Windows.Forms.Button btnMoveAllLeft;
        private System.Windows.Forms.ComboBox cmbLanguage;
        private System.Windows.Forms.Button btnChangeLanguage;
    }
}