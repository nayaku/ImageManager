namespace ImageManager
{
    partial class NumberInputDialog
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.skinNumericUpDown = new CCWin.SkinControl.SkinNumericUpDown();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cancelSkinButton = new CCWin.SkinControl.SkinButton();
            this.okSkinButton = new CCWin.SkinControl.SkinButton();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skinNumericUpDown)).BeginInit();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.skinNumericUpDown);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(163, 30);
            this.panel1.TabIndex = 0;
            // 
            // skinNumericUpDown
            // 
            this.skinNumericUpDown.Location = new System.Drawing.Point(23, 4);
            this.skinNumericUpDown.Name = "skinNumericUpDown";
            this.skinNumericUpDown.Size = new System.Drawing.Size(120, 21);
            this.skinNumericUpDown.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.cancelSkinButton);
            this.panel2.Controls.Add(this.okSkinButton);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 37);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(163, 29);
            this.panel2.TabIndex = 1;
            // 
            // cancelSkinButton
            // 
            this.cancelSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.cancelSkinButton.DownBack = null;
            this.cancelSkinButton.Location = new System.Drawing.Point(87, 4);
            this.cancelSkinButton.MouseBack = null;
            this.cancelSkinButton.Name = "cancelSkinButton";
            this.cancelSkinButton.NormlBack = null;
            this.cancelSkinButton.Size = new System.Drawing.Size(62, 23);
            this.cancelSkinButton.TabIndex = 1;
            this.cancelSkinButton.Text = "取消";
            this.cancelSkinButton.UseVisualStyleBackColor = false;
            this.cancelSkinButton.Click += new System.EventHandler(this.CancelSkinButton_Click);
            // 
            // okSkinButton
            // 
            this.okSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.okSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.okSkinButton.DownBack = null;
            this.okSkinButton.Location = new System.Drawing.Point(16, 4);
            this.okSkinButton.MouseBack = null;
            this.okSkinButton.Name = "okSkinButton";
            this.okSkinButton.NormlBack = null;
            this.okSkinButton.Size = new System.Drawing.Size(58, 23);
            this.okSkinButton.TabIndex = 0;
            this.okSkinButton.Text = "确认";
            this.okSkinButton.UseVisualStyleBackColor = false;
            this.okSkinButton.Click += new System.EventHandler(this.OkSkinButton_Click);
            // 
            // NumberInputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(163, 66);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NumberInputDialog";
            this.ShowIcon = false;
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.skinNumericUpDown)).EndInit();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private CCWin.SkinControl.SkinButton cancelSkinButton;
        private CCWin.SkinControl.SkinButton okSkinButton;
        public CCWin.SkinControl.SkinNumericUpDown skinNumericUpDown;
    }
}