namespace ImageManager
{
    partial class ChooseLabelDialog
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
            this.skinComboBox = new CCWin.SkinControl.SkinComboBox();
            this.okSkinButton = new CCWin.SkinControl.SkinButton();
            this.cancelSkinButton = new CCWin.SkinControl.SkinButton();
            this.skinLabel1 = new CCWin.SkinControl.SkinLabel();
            this.addImageLabelSkinButton = new CCWin.SkinControl.SkinButton();
            this.SuspendLayout();
            // 
            // skinComboBox
            // 
            this.skinComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.skinComboBox.FormattingEnabled = true;
            this.skinComboBox.Location = new System.Drawing.Point(12, 26);
            this.skinComboBox.Name = "skinComboBox";
            this.skinComboBox.Size = new System.Drawing.Size(175, 22);
            this.skinComboBox.TabIndex = 0;
            this.skinComboBox.WaterText = "";
            this.skinComboBox.SelectionChangeCommitted += new System.EventHandler(this.skinComboBox_SelectionChangeCommitted);
            this.skinComboBox.TextUpdate += new System.EventHandler(this.SkinComboBox_TextUpdate);
            // 
            // okSkinButton
            // 
            this.okSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.okSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.okSkinButton.DownBack = null;
            this.okSkinButton.Location = new System.Drawing.Point(15, 54);
            this.okSkinButton.MouseBack = null;
            this.okSkinButton.Name = "okSkinButton";
            this.okSkinButton.NormlBack = null;
            this.okSkinButton.Size = new System.Drawing.Size(75, 23);
            this.okSkinButton.TabIndex = 1;
            this.okSkinButton.Text = "确定";
            this.okSkinButton.UseVisualStyleBackColor = false;
            this.okSkinButton.Click += new System.EventHandler(this.OkSkinButton_Click);
            // 
            // cancelSkinButton
            // 
            this.cancelSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.cancelSkinButton.DownBack = null;
            this.cancelSkinButton.Location = new System.Drawing.Point(131, 54);
            this.cancelSkinButton.MouseBack = null;
            this.cancelSkinButton.Name = "cancelSkinButton";
            this.cancelSkinButton.NormlBack = null;
            this.cancelSkinButton.Size = new System.Drawing.Size(75, 23);
            this.cancelSkinButton.TabIndex = 2;
            this.cancelSkinButton.Text = "取消";
            this.cancelSkinButton.UseVisualStyleBackColor = false;
            this.cancelSkinButton.Click += new System.EventHandler(this.CancelSkinButton_Click);
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(12, 6);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(56, 17);
            this.skinLabel1.TabIndex = 3;
            this.skinLabel1.Text = "标签名：";
            // 
            // addImageLabelSkinButton
            // 
            this.addImageLabelSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.addImageLabelSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.addImageLabelSkinButton.DownBack = null;
            this.addImageLabelSkinButton.Location = new System.Drawing.Point(193, 26);
            this.addImageLabelSkinButton.MouseBack = null;
            this.addImageLabelSkinButton.Name = "addImageLabelSkinButton";
            this.addImageLabelSkinButton.NormlBack = null;
            this.addImageLabelSkinButton.Size = new System.Drawing.Size(20, 23);
            this.addImageLabelSkinButton.TabIndex = 4;
            this.addImageLabelSkinButton.Text = "✚";
            this.addImageLabelSkinButton.UseVisualStyleBackColor = false;
            this.addImageLabelSkinButton.Click += new System.EventHandler(this.AddImageLabelSkinButton_Click);
            // 
            // ChooseLabelDialog
            // 
            this.AcceptButton = this.okSkinButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(218, 89);
            this.Controls.Add(this.addImageLabelSkinButton);
            this.Controls.Add(this.skinLabel1);
            this.Controls.Add(this.cancelSkinButton);
            this.Controls.Add(this.okSkinButton);
            this.Controls.Add(this.skinComboBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseLabelDialog";
            this.ShowIcon = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CCWin.SkinControl.SkinComboBox skinComboBox;
        private CCWin.SkinControl.SkinButton okSkinButton;
        private CCWin.SkinControl.SkinButton cancelSkinButton;
        private CCWin.SkinControl.SkinLabel skinLabel1;
        private CCWin.SkinControl.SkinButton addImageLabelSkinButton;
    }
}