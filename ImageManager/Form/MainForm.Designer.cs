namespace ImageManager
{
    partial class MainForm
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
            System.Windows.Forms.Panel topPanel;
            System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
            this.panel1 = new System.Windows.Forms.Panel();
            this.searchSkinButton = new CCWin.SkinControl.SkinButton();
            this.saerchTextBox = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.orderByTitleSkinRadioButton = new CCWin.SkinControl.SkinRadioButton();
            this.orderByAddTimeSkinRadioButton = new CCWin.SkinControl.SkinRadioButton();
            this.orderByEditTimeSkinRadioButton = new CCWin.SkinControl.SkinRadioButton();
            this.skinLabel1 = new CCWin.SkinControl.SkinLabel();
            this.imageLabelFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.addSearchLabelSkinButton = new CCWin.SkinControl.SkinButton();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.pictureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addImagesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.screenShotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.useGlobalScreenShotHotKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addImageLabelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectCancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInvertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearMemoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imagePanel = new System.Windows.Forms.Panel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.isAscSkinCheckBox = new CCWin.SkinControl.SkinCheckBox();
            topPanel = new System.Windows.Forms.Panel();
            flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            topPanel.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.imageLabelFlowLayoutPanel.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.BackColor = System.Drawing.Color.Transparent;
            topPanel.Controls.Add(flowLayoutPanel2);
            topPanel.Controls.Add(this.imageLabelFlowLayoutPanel);
            topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            topPanel.Location = new System.Drawing.Point(0, 25);
            topPanel.Name = "topPanel";
            topPanel.Size = new System.Drawing.Size(1054, 70);
            topPanel.TabIndex = 1;
            // 
            // flowLayoutPanel2
            // 
            flowLayoutPanel2.BackColor = System.Drawing.Color.Transparent;
            flowLayoutPanel2.Controls.Add(this.panel1);
            flowLayoutPanel2.Controls.Add(this.flowLayoutPanel1);
            flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Right;
            flowLayoutPanel2.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flowLayoutPanel2.Location = new System.Drawing.Point(711, 0);
            flowLayoutPanel2.Name = "flowLayoutPanel2";
            flowLayoutPanel2.Size = new System.Drawing.Size(343, 70);
            flowLayoutPanel2.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.searchSkinButton);
            this.panel1.Controls.Add(this.saerchTextBox);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(337, 28);
            this.panel1.TabIndex = 1;
            // 
            // searchSkinButton
            // 
            this.searchSkinButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.searchSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.searchSkinButton.DownBack = null;
            this.searchSkinButton.Location = new System.Drawing.Point(259, 3);
            this.searchSkinButton.MouseBack = null;
            this.searchSkinButton.Name = "searchSkinButton";
            this.searchSkinButton.NormlBack = null;
            this.searchSkinButton.Size = new System.Drawing.Size(75, 23);
            this.searchSkinButton.TabIndex = 2;
            this.searchSkinButton.Text = "搜索";
            this.searchSkinButton.UseVisualStyleBackColor = false;
            this.searchSkinButton.Click += new System.EventHandler(this.SearchSkinButton_Click);
            // 
            // saerchTextBox
            // 
            this.saerchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.saerchTextBox.Location = new System.Drawing.Point(3, 3);
            this.saerchTextBox.Name = "saerchTextBox";
            this.saerchTextBox.Size = new System.Drawing.Size(250, 21);
            this.saerchTextBox.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.orderByTitleSkinRadioButton);
            this.flowLayoutPanel1.Controls.Add(this.orderByAddTimeSkinRadioButton);
            this.flowLayoutPanel1.Controls.Add(this.orderByEditTimeSkinRadioButton);
            this.flowLayoutPanel1.Controls.Add(this.isAscSkinCheckBox);
            this.flowLayoutPanel1.Controls.Add(this.skinLabel1);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 37);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(337, 27);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // orderByTitleSkinRadioButton
            // 
            this.orderByTitleSkinRadioButton.AutoSize = true;
            this.orderByTitleSkinRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.orderByTitleSkinRadioButton.Checked = true;
            this.orderByTitleSkinRadioButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.orderByTitleSkinRadioButton.DownBack = null;
            this.orderByTitleSkinRadioButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.orderByTitleSkinRadioButton.Location = new System.Drawing.Point(284, 3);
            this.orderByTitleSkinRadioButton.MouseBack = null;
            this.orderByTitleSkinRadioButton.Name = "orderByTitleSkinRadioButton";
            this.orderByTitleSkinRadioButton.NormlBack = null;
            this.orderByTitleSkinRadioButton.SelectedDownBack = null;
            this.orderByTitleSkinRadioButton.SelectedMouseBack = null;
            this.orderByTitleSkinRadioButton.SelectedNormlBack = null;
            this.orderByTitleSkinRadioButton.Size = new System.Drawing.Size(50, 21);
            this.orderByTitleSkinRadioButton.TabIndex = 0;
            this.orderByTitleSkinRadioButton.TabStop = true;
            this.orderByTitleSkinRadioButton.Text = "标题";
            this.orderByTitleSkinRadioButton.UseVisualStyleBackColor = false;
            // 
            // orderByAddTimeSkinRadioButton
            // 
            this.orderByAddTimeSkinRadioButton.AutoSize = true;
            this.orderByAddTimeSkinRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.orderByAddTimeSkinRadioButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.orderByAddTimeSkinRadioButton.DownBack = null;
            this.orderByAddTimeSkinRadioButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.orderByAddTimeSkinRadioButton.Location = new System.Drawing.Point(204, 3);
            this.orderByAddTimeSkinRadioButton.MouseBack = null;
            this.orderByAddTimeSkinRadioButton.Name = "orderByAddTimeSkinRadioButton";
            this.orderByAddTimeSkinRadioButton.NormlBack = null;
            this.orderByAddTimeSkinRadioButton.SelectedDownBack = null;
            this.orderByAddTimeSkinRadioButton.SelectedMouseBack = null;
            this.orderByAddTimeSkinRadioButton.SelectedNormlBack = null;
            this.orderByAddTimeSkinRadioButton.Size = new System.Drawing.Size(74, 21);
            this.orderByAddTimeSkinRadioButton.TabIndex = 1;
            this.orderByAddTimeSkinRadioButton.TabStop = true;
            this.orderByAddTimeSkinRadioButton.Text = "添加时间";
            this.orderByAddTimeSkinRadioButton.UseVisualStyleBackColor = false;
            // 
            // orderByEditTimeSkinRadioButton
            // 
            this.orderByEditTimeSkinRadioButton.AutoSize = true;
            this.orderByEditTimeSkinRadioButton.BackColor = System.Drawing.Color.Transparent;
            this.orderByEditTimeSkinRadioButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.orderByEditTimeSkinRadioButton.DownBack = null;
            this.orderByEditTimeSkinRadioButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.orderByEditTimeSkinRadioButton.Location = new System.Drawing.Point(124, 3);
            this.orderByEditTimeSkinRadioButton.MouseBack = null;
            this.orderByEditTimeSkinRadioButton.Name = "orderByEditTimeSkinRadioButton";
            this.orderByEditTimeSkinRadioButton.NormlBack = null;
            this.orderByEditTimeSkinRadioButton.SelectedDownBack = null;
            this.orderByEditTimeSkinRadioButton.SelectedMouseBack = null;
            this.orderByEditTimeSkinRadioButton.SelectedNormlBack = null;
            this.orderByEditTimeSkinRadioButton.Size = new System.Drawing.Size(74, 21);
            this.orderByEditTimeSkinRadioButton.TabIndex = 2;
            this.orderByEditTimeSkinRadioButton.TabStop = true;
            this.orderByEditTimeSkinRadioButton.Text = "修改时间";
            this.orderByEditTimeSkinRadioButton.UseVisualStyleBackColor = false;
            // 
            // skinLabel1
            // 
            this.skinLabel1.AutoSize = true;
            this.skinLabel1.BackColor = System.Drawing.Color.Transparent;
            this.skinLabel1.BorderColor = System.Drawing.Color.White;
            this.skinLabel1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.skinLabel1.Location = new System.Drawing.Point(17, 0);
            this.skinLabel1.Name = "skinLabel1";
            this.skinLabel1.Size = new System.Drawing.Size(44, 17);
            this.skinLabel1.TabIndex = 3;
            this.skinLabel1.Text = "排序：";
            // 
            // imageLabelFlowLayoutPanel
            // 
            this.imageLabelFlowLayoutPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.imageLabelFlowLayoutPanel.BackColor = System.Drawing.Color.Transparent;
            this.imageLabelFlowLayoutPanel.Controls.Add(this.addSearchLabelSkinButton);
            this.imageLabelFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.imageLabelFlowLayoutPanel.Name = "imageLabelFlowLayoutPanel";
            this.imageLabelFlowLayoutPanel.Size = new System.Drawing.Size(705, 70);
            this.imageLabelFlowLayoutPanel.TabIndex = 0;
            // 
            // addSearchLabelSkinButton
            // 
            this.addSearchLabelSkinButton.BackColor = System.Drawing.Color.Transparent;
            this.addSearchLabelSkinButton.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.addSearchLabelSkinButton.DownBack = null;
            this.addSearchLabelSkinButton.DrawType = CCWin.SkinControl.DrawStyle.Img;
            this.addSearchLabelSkinButton.Image = global::ImageManager.Properties.Resources.add;
            this.addSearchLabelSkinButton.Location = new System.Drawing.Point(3, 3);
            this.addSearchLabelSkinButton.MouseBack = null;
            this.addSearchLabelSkinButton.Name = "addSearchLabelSkinButton";
            this.addSearchLabelSkinButton.NormlBack = null;
            this.addSearchLabelSkinButton.Size = new System.Drawing.Size(32, 28);
            this.addSearchLabelSkinButton.TabIndex = 0;
            this.addSearchLabelSkinButton.UseVisualStyleBackColor = false;
            this.addSearchLabelSkinButton.Click += new System.EventHandler(this.AddSearchSkinButton_Click);
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pictureToolStripMenuItem,
            this.selectToolStripMenuItem,
            this.clearMemoryToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1054, 25);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // pictureToolStripMenuItem
            // 
            this.pictureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addImagesToolStripMenuItem,
            this.addFolderToolStripMenuItem,
            this.screenShotToolStripMenuItem,
            this.useGlobalScreenShotHotKeyToolStripMenuItem,
            this.removeImageToolStripMenuItem,
            this.addImageLabelToolStripMenuItem});
            this.pictureToolStripMenuItem.Name = "pictureToolStripMenuItem";
            this.pictureToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.pictureToolStripMenuItem.Text = "图片";
            this.pictureToolStripMenuItem.DropDownOpening += new System.EventHandler(this.PictureToolStripMenuItem_DropDownOpening);
            // 
            // addImagesToolStripMenuItem
            // 
            this.addImagesToolStripMenuItem.Name = "addImagesToolStripMenuItem";
            this.addImagesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.addImagesToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.addImagesToolStripMenuItem.Text = "添加图片";
            this.addImagesToolStripMenuItem.Click += new System.EventHandler(this.AddImagesToolStripMenuItem_Click);
            // 
            // addFolderToolStripMenuItem
            // 
            this.addFolderToolStripMenuItem.Name = "addFolderToolStripMenuItem";
            this.addFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.O)));
            this.addFolderToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.addFolderToolStripMenuItem.Text = "添加文件夹";
            this.addFolderToolStripMenuItem.Click += new System.EventHandler(this.AddFolderToolStripMenuItem_Click);
            // 
            // screenShotToolStripMenuItem
            // 
            this.screenShotToolStripMenuItem.Name = "screenShotToolStripMenuItem";
            this.screenShotToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.screenShotToolStripMenuItem.Text = "截图";
            this.screenShotToolStripMenuItem.Click += new System.EventHandler(this.ScreenShotToolStripMenuItem_Click);
            // 
            // useGlobalScreenShotHotKeyToolStripMenuItem
            // 
            this.useGlobalScreenShotHotKeyToolStripMenuItem.Name = "useGlobalScreenShotHotKeyToolStripMenuItem";
            this.useGlobalScreenShotHotKeyToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.useGlobalScreenShotHotKeyToolStripMenuItem.Text = "使用全局截图快捷键";
            this.useGlobalScreenShotHotKeyToolStripMenuItem.Click += new System.EventHandler(this.UseGlobalScreenShotHotKeyToolStripMenuItem_Click);
            // 
            // removeImageToolStripMenuItem
            // 
            this.removeImageToolStripMenuItem.Name = "removeImageToolStripMenuItem";
            this.removeImageToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.removeImageToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.removeImageToolStripMenuItem.Text = "删除";
            this.removeImageToolStripMenuItem.Click += new System.EventHandler(this.RemoveImageToolStripMenuItem_Click);
            // 
            // addImageLabelToolStripMenuItem
            // 
            this.addImageLabelToolStripMenuItem.Name = "addImageLabelToolStripMenuItem";
            this.addImageLabelToolStripMenuItem.Size = new System.Drawing.Size(217, 22);
            this.addImageLabelToolStripMenuItem.Text = "添加标签";
            this.addImageLabelToolStripMenuItem.Click += new System.EventHandler(this.AddImageLabelToolStripMenuItem_Click);
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAllToolStripMenuItem,
            this.selectCancelToolStripMenuItem,
            this.selectInvertToolStripMenuItem});
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            this.selectToolStripMenuItem.Text = "选择";
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.selectAllToolStripMenuItem.Text = "全选";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.SelectAllToolStripMenuItem_Click);
            // 
            // selectCancelToolStripMenuItem
            // 
            this.selectCancelToolStripMenuItem.Name = "selectCancelToolStripMenuItem";
            this.selectCancelToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.selectCancelToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.selectCancelToolStripMenuItem.Text = "取消选择";
            this.selectCancelToolStripMenuItem.Click += new System.EventHandler(this.SelectCancelToolStripMenuItem_Click);
            // 
            // selectInvertToolStripMenuItem
            // 
            this.selectInvertToolStripMenuItem.Name = "selectInvertToolStripMenuItem";
            this.selectInvertToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.I)));
            this.selectInvertToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.selectInvertToolStripMenuItem.Text = "反选";
            this.selectInvertToolStripMenuItem.Click += new System.EventHandler(this.SelectInvertToolStripMenuItem_Click);
            // 
            // clearMemoryToolStripMenuItem
            // 
            this.clearMemoryToolStripMenuItem.Name = "clearMemoryToolStripMenuItem";
            this.clearMemoryToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.clearMemoryToolStripMenuItem.Text = "清理内存";
            this.clearMemoryToolStripMenuItem.Click += new System.EventHandler(this.ClearMemoryToolStripMenuItem_Click);
            // 
            // imagePanel
            // 
            this.imagePanel.AutoScroll = true;
            this.imagePanel.BackColor = System.Drawing.Color.Transparent;
            this.imagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imagePanel.Location = new System.Drawing.Point(0, 95);
            this.imagePanel.Name = "imagePanel";
            this.imagePanel.Size = new System.Drawing.Size(1054, 546);
            this.imagePanel.TabIndex = 2;
            this.imagePanel.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ImagePanel_Scroll);
            this.imagePanel.SizeChanged += new System.EventHandler(this.ImagePanel_SizeChanged);
            // 
            // openFileDialog
            // 
            this.openFileDialog.Multiselect = true;
            // 
            // isAscSkinCheckBox
            // 
            this.isAscSkinCheckBox.AutoSize = true;
            this.isAscSkinCheckBox.BackColor = System.Drawing.Color.Transparent;
            this.isAscSkinCheckBox.Checked = true;
            this.isAscSkinCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.isAscSkinCheckBox.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.isAscSkinCheckBox.DownBack = null;
            this.isAscSkinCheckBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.isAscSkinCheckBox.Location = new System.Drawing.Point(67, 3);
            this.isAscSkinCheckBox.MouseBack = null;
            this.isAscSkinCheckBox.Name = "isAscSkinCheckBox";
            this.isAscSkinCheckBox.NormlBack = null;
            this.isAscSkinCheckBox.SelectedDownBack = null;
            this.isAscSkinCheckBox.SelectedMouseBack = null;
            this.isAscSkinCheckBox.SelectedNormlBack = null;
            this.isAscSkinCheckBox.Size = new System.Drawing.Size(51, 21);
            this.isAscSkinCheckBox.TabIndex = 4;
            this.isAscSkinCheckBox.Text = "升序";
            this.isAscSkinCheckBox.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ImageManager.Properties.Resources.background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1054, 641);
            this.Controls.Add(this.imagePanel);
            this.Controls.Add(topPanel);
            this.Controls.Add(this.menuStrip);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ImagePanel_Scroll);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.Leave += new System.EventHandler(this.MainForm_Leave);
            topPanel.ResumeLayout(false);
            flowLayoutPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.imageLabelFlowLayoutPanel.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem pictureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImagesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addImageLabelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectCancelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInvertToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox saerchTextBox;
        private System.Windows.Forms.FlowLayoutPanel imageLabelFlowLayoutPanel;
        private System.Windows.Forms.Panel imagePanel;
        private CCWin.SkinControl.SkinButton searchSkinButton;
        private System.Windows.Forms.ToolStripMenuItem clearMemoryToolStripMenuItem;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private CCWin.SkinControl.SkinLabel skinLabel1;
        private CCWin.SkinControl.SkinRadioButton orderByTitleSkinRadioButton;
        private CCWin.SkinControl.SkinRadioButton orderByAddTimeSkinRadioButton;
        private CCWin.SkinControl.SkinRadioButton orderByEditTimeSkinRadioButton;
        private System.Windows.Forms.ToolStripMenuItem screenShotToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ToolStripMenuItem useGlobalScreenShotHotKeyToolStripMenuItem;
        private CCWin.SkinControl.SkinButton addSearchLabelSkinButton;
        private CCWin.SkinControl.SkinCheckBox isAscSkinCheckBox;
    }
}