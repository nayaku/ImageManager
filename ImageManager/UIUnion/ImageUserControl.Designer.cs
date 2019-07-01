namespace ImageManager
{
    partial class ImageUserControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWithOtherToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyImageContentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyImagePathToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addImgLabelToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.removeImageToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureCheckBox = new System.Windows.Forms.CheckBox();
            this.imageTitleLabel = new System.Windows.Forms.Label();
            this.imgLabelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox.ContextMenuStrip = this.contextMenuStrip;
            this.pictureBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox.ErrorImage = null;
            this.pictureBox.Location = new System.Drawing.Point(0, 0);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(142, 44);
            this.pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox.TabIndex = 0;
            this.pictureBox.TabStop = false;
            this.pictureBox.Click += new System.EventHandler(this.PictureBox_Click);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.openWithOtherToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyImageContentToolStripMenuItem,
            this.copyImagePathToolStripMenuItem1,
            this.toolStripSeparator2,
            this.addImgLabelToolStripMenuItem1,
            this.removeImageToolStripMenuItem1});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(181, 170);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "贴片式打开";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.OpenToolStripMenuItem_Click);
            // 
            // openWithOtherToolStripMenuItem
            // 
            this.openWithOtherToolStripMenuItem.Name = "openWithOtherToolStripMenuItem";
            this.openWithOtherToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openWithOtherToolStripMenuItem.Text = "用其他程序打开";
            this.openWithOtherToolStripMenuItem.Click += new System.EventHandler(this.OpenWithOtherToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // copyImageContentToolStripMenuItem
            // 
            this.copyImageContentToolStripMenuItem.Name = "copyImageContentToolStripMenuItem";
            this.copyImageContentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.copyImageContentToolStripMenuItem.Text = "拷贝图片内容";
            this.copyImageContentToolStripMenuItem.Click += new System.EventHandler(this.CopyImageContentToolStripMenuItem_Click);
            // 
            // copyImagePathToolStripMenuItem1
            // 
            this.copyImagePathToolStripMenuItem1.Name = "copyImagePathToolStripMenuItem1";
            this.copyImagePathToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.copyImagePathToolStripMenuItem1.Text = "拷贝图片路径";
            this.copyImagePathToolStripMenuItem1.Click += new System.EventHandler(this.CopyImagePathToolStripMenuItem1_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // addImgLabelToolStripMenuItem1
            // 
            this.addImgLabelToolStripMenuItem1.Name = "addImgLabelToolStripMenuItem1";
            this.addImgLabelToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.addImgLabelToolStripMenuItem1.Text = "添加图片标签";
            this.addImgLabelToolStripMenuItem1.Click += new System.EventHandler(this.AddImgLabelToolStripMenuItem1_Click);
            // 
            // removeImageToolStripMenuItem1
            // 
            this.removeImageToolStripMenuItem1.Name = "removeImageToolStripMenuItem1";
            this.removeImageToolStripMenuItem1.Size = new System.Drawing.Size(180, 22);
            this.removeImageToolStripMenuItem1.Text = "删除图片";
            this.removeImageToolStripMenuItem1.Click += new System.EventHandler(this.RemoveImageToolStripMenuItem1_Click);
            // 
            // pictureCheckBox
            // 
            this.pictureCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureCheckBox.AutoSize = true;
            this.pictureCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.pictureCheckBox.Location = new System.Drawing.Point(127, 3);
            this.pictureCheckBox.Name = "pictureCheckBox";
            this.pictureCheckBox.Size = new System.Drawing.Size(15, 14);
            this.pictureCheckBox.TabIndex = 1;
            this.pictureCheckBox.UseVisualStyleBackColor = false;
            // 
            // imageTitleLabel
            // 
            this.imageTitleLabel.AutoSize = true;
            this.imageTitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.imageTitleLabel.ContextMenuStrip = this.contextMenuStrip;
            this.imageTitleLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageTitleLabel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.imageTitleLabel.Location = new System.Drawing.Point(0, 0);
            this.imageTitleLabel.Name = "imageTitleLabel";
            this.imageTitleLabel.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.imageTitleLabel.Size = new System.Drawing.Size(96, 42);
            this.imageTitleLabel.TabIndex = 2;
            this.imageTitleLabel.Text = "ImageTitle";
            this.imageTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // imgLabelFlowLayoutPanel
            // 
            this.imgLabelFlowLayoutPanel.AutoSize = true;
            this.imgLabelFlowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.imgLabelFlowLayoutPanel.ContextMenuStrip = this.contextMenuStrip;
            this.imgLabelFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.imgLabelFlowLayoutPanel.Location = new System.Drawing.Point(0, 226);
            this.imgLabelFlowLayoutPanel.Name = "imgLabelFlowLayoutPanel";
            this.imgLabelFlowLayoutPanel.Size = new System.Drawing.Size(142, 0);
            this.imgLabelFlowLayoutPanel.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.imageTitleLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 44);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(142, 182);
            this.panel1.TabIndex = 4;
            // 
            // ImageUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ContextMenuStrip = this.contextMenuStrip;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.imgLabelFlowLayoutPanel);
            this.Controls.Add(this.pictureCheckBox);
            this.Controls.Add(this.pictureBox);
            this.Name = "ImageUserControl";
            this.Size = new System.Drawing.Size(142, 226);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.CheckBox pictureCheckBox;
        private System.Windows.Forms.Label imageTitleLabel;
        private System.Windows.Forms.FlowLayoutPanel imgLabelFlowLayoutPanel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWithOtherToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyImageContentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyImagePathToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem addImgLabelToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem removeImageToolStripMenuItem1;
        private System.Windows.Forms.Panel panel1;
    }
}
