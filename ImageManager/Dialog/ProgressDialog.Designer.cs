namespace ImageManager
{
    partial class ProgressDialog
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
            WorkingTokenSource.Dispose();
            _workTask.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.Panel panel1;
            this.cancelSkinButton = new CCWin.SkinControl.SkinButton();
            this.messageRichTextBox = new System.Windows.Forms.RichTextBox();
            this.skinProgressBar = new CCWin.SkinControl.SkinProgressBar();
            this.groupBox = new System.Windows.Forms.GroupBox();
            panel1 = new System.Windows.Forms.Panel();
            panel1.SuspendLayout();
            this.groupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(this.cancelSkinButton);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 127);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(311, 22);
            panel1.TabIndex = 3;
            // 
            // cancelSkinButton
            // 
            this.cancelSkinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelSkinButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.cancelSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.cancelSkinButton.DownBack = null;
            this.cancelSkinButton.Location = new System.Drawing.Point(124, 1);
            this.cancelSkinButton.MouseBack = null;
            this.cancelSkinButton.Name = "cancelSkinButton";
            this.cancelSkinButton.NormlBack = null;
            this.cancelSkinButton.Size = new System.Drawing.Size(75, 21);
            this.cancelSkinButton.TabIndex = 0;
            this.cancelSkinButton.Text = "取消";
            this.cancelSkinButton.UseVisualStyleBackColor = false;
            this.cancelSkinButton.Click += new System.EventHandler(this.CancelSkinButton_Click);
            // 
            // messageRichTextBox
            // 
            this.messageRichTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.messageRichTextBox.Location = new System.Drawing.Point(3, 17);
            this.messageRichTextBox.Name = "messageRichTextBox";
            this.messageRichTextBox.ReadOnly = true;
            this.messageRichTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.messageRichTextBox.Size = new System.Drawing.Size(305, 81);
            this.messageRichTextBox.TabIndex = 0;
            this.messageRichTextBox.Text = "";
            // 
            // skinProgressBar
            // 
            this.skinProgressBar.Back = null;
            this.skinProgressBar.BackColor = System.Drawing.Color.Transparent;
            this.skinProgressBar.BarBack = null;
            this.skinProgressBar.BarRadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.skinProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.skinProgressBar.ForeColor = System.Drawing.Color.Red;
            this.skinProgressBar.Location = new System.Drawing.Point(0, 0);
            this.skinProgressBar.Name = "skinProgressBar";
            this.skinProgressBar.RadiusStyle = CCWin.SkinClass.RoundStyle.All;
            this.skinProgressBar.Size = new System.Drawing.Size(311, 23);
            this.skinProgressBar.TabIndex = 0;
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.messageRichTextBox);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 23);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(311, 126);
            this.groupBox.TabIndex = 2;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "消息";
            // 
            // ProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 149);
            this.Controls.Add(panel1);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.skinProgressBar);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ProgressDialog_FormClosing);
            panel1.ResumeLayout(false);
            this.groupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CCWin.SkinControl.SkinProgressBar skinProgressBar;
        private System.Windows.Forms.GroupBox groupBox;
        private CCWin.SkinControl.SkinButton cancelSkinButton;
        private System.Windows.Forms.RichTextBox messageRichTextBox;
    }
}