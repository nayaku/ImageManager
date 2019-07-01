namespace ImageManager
{
    partial class ImageLabelUserControl
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
            this.skinPanel1 = new CCWin.SkinControl.SkinPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.imgLabelNameLabel = new System.Windows.Forms.Label();
            this.xLabel = new System.Windows.Forms.Label();
            this.skinPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // skinPanel1
            // 
            this.skinPanel1.AutoSize = true;
            this.skinPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.skinPanel1.BackColor = System.Drawing.Color.Transparent;
            this.skinPanel1.BorderColor = System.Drawing.Color.Black;
            this.skinPanel1.Controls.Add(this.flowLayoutPanel1);
            this.skinPanel1.Controls.Add(this.xLabel);
            this.skinPanel1.ControlState = CCWin.SkinClass.ControlState.Normal;
            this.skinPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.skinPanel1.DownBack = null;
            this.skinPanel1.Font = new System.Drawing.Font("宋体", 10F);
            this.skinPanel1.Location = new System.Drawing.Point(0, 0);
            this.skinPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.skinPanel1.MouseBack = null;
            this.skinPanel1.Name = "skinPanel1";
            this.skinPanel1.NormlBack = null;
            this.skinPanel1.Padding = new System.Windows.Forms.Padding(2);
            this.skinPanel1.Radius = 12;
            this.skinPanel1.RoundStyle = CCWin.SkinClass.RoundStyle.All;
            this.skinPanel1.Size = new System.Drawing.Size(131, 43);
            this.skinPanel1.TabIndex = 3;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.imgLabelNameLabel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(2, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(106, 39);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // imgLabelNameLabel
            // 
            this.imgLabelNameLabel.AutoSize = true;
            this.imgLabelNameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imgLabelNameLabel.Location = new System.Drawing.Point(0, 0);
            this.imgLabelNameLabel.Margin = new System.Windows.Forms.Padding(0);
            this.imgLabelNameLabel.Name = "imgLabelNameLabel";
            this.imgLabelNameLabel.Size = new System.Drawing.Size(49, 14);
            this.imgLabelNameLabel.TabIndex = 0;
            this.imgLabelNameLabel.Text = "label1";
            this.imgLabelNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.imgLabelNameLabel.Click += new System.EventHandler(this.ImgLabelNameLabel_Click);
            // 
            // xLabel
            // 
            this.xLabel.AutoSize = true;
            this.xLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.xLabel.Location = new System.Drawing.Point(108, 2);
            this.xLabel.Name = "xLabel";
            this.xLabel.Size = new System.Drawing.Size(21, 14);
            this.xLabel.TabIndex = 1;
            this.xLabel.Text = "✖";
            this.xLabel.Click += new System.EventHandler(this.XLabel_Click);
            // 
            // ImageLabelUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.skinPanel1);
            this.Name = "ImageLabelUserControl";
            this.Size = new System.Drawing.Size(131, 43);
            this.skinPanel1.ResumeLayout(false);
            this.skinPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CCWin.SkinControl.SkinPanel skinPanel1;
        private System.Windows.Forms.Label xLabel;
        private System.Windows.Forms.Label imgLabelNameLabel;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}
