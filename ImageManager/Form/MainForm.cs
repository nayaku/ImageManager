using ImageManager.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WPFCaptureScreenShot;
using static ImageManager.HotKey;

namespace ImageManager
{
    public partial class MainForm : Form, IEnumerable
    {

        /// <summary>
        /// 自身实例
        /// </summary>
        private static MainForm _instance;
        /// <summary>
        /// 每列高度
        /// </summary>
        private int[] _columnsHeight;
        /// <summary>
        /// 等待图片加载完成的队列
        /// </summary>
        private List<ImageUserControl> _loadingImageUserControlList = new List<ImageUserControl>();
        IEnumerator enumerator;

        private MainForm()
        {
            InitializeComponent();
            imagePanel.HorizontalScroll.Visible = false;
            //var wpfwindow = new CaptureWindow();
            //wpfwindow.ShowDialog();
            // 绑定与图片加载缓冲的更新
            ImageCache.LoadFinishImageDelegate = LoadImageFinish;
            UpdateAllImageUserControl();
            //imagePanel.MouseWheel += new MouseEventHandler(delegate (object sender, MouseEventArgs e)
            //{
            //    ImagePanel_Scroll(sender, null);
            //});

            useGlobalScreenShotHotKeyToolStripMenuItem.Checked = Settings.Default.UseGlobalScreenShotHotKey;
            UseGlobalScreenShotHotKey();

        }

        /// <summary>
        /// 获取自身实例
        /// </summary>
        /// <returns></returns>
        public static MainForm GetInstance()
        {
            if (_instance == null)
                _instance = new MainForm();
            return _instance;
        }

        /// <summary>
        /// 添加需要显示的图片
        /// </summary>
        /// <param name="imageUserControl"></param>
        public void AddImageUserControl(ImageUserControl imageUserControl)
        {
            if (imagePanel.InvokeRequired)
            {
                imagePanel.Invoke(new MethodInvoker(delegate ()
                {
                    AddImageUserControl(imageUserControl);
                }));
            }
            else
            {
                var eachWidth = Settings.Default.ImageWidth + Settings.Default.ImagePadding;
                var index = Utils.GetMinIndex(_columnsHeight);
                if (imagePanel.Controls.Count >= 1)
                {
                    var firstImageUserControl = (ImageUserControl)imagePanel.Controls[0];
                    var newPoint = new Point(index * eachWidth, _columnsHeight[index]);
                    newPoint.Y += firstImageUserControl.Location.Y;
                    if (imageUserControl.Location != newPoint)
                    {
                        imageUserControl.Location = newPoint;
                    }
                }
                if (!imagePanel.Controls.Contains(imageUserControl))
                {
                    long t_startTime, t_endTime;
                    t_startTime = DateTime.Now.Ticks;
                    imagePanel.Controls.Add(imageUserControl);
                    t_endTime = DateTime.Now.Ticks;
                    Debug.WriteLine($"imagePanel.Controls.Add函数耗时{(t_endTime - t_startTime) / 1000}毫秒。");
                }

                _columnsHeight[index] += imageUserControl.Height + Settings.Default.ImagePadding;
            }

        }

        /// <summary>
        /// 清除所有图片和加载的图片
        /// </summary>
        public void Clear()
        {
            if (imagePanel.InvokeRequired)
            {
                imagePanel.Invoke(new MethodInvoker(delegate ()
                {
                    Clear();
                }));
            }
            else
            {
                //imagePanel.SuspendLayout();
                ImageCache.ClearLoadStack();
                _loadingImageUserControlList.Clear();
                imagePanel.Controls.Clear();
                UpdateAllImageUserControl();
                //imagePanel.ResumeLayout();
            }
        }

        /// <summary>
        /// 清空内存
        /// </summary>
        public void ClearMemory()
        {
            var systemInfo = new SystemInfo();
            var memoryAvailable = systemInfo.MemoryAvailable;
            ClearMemoryUtil.Clear();
            var memoryAvailable2 = systemInfo.MemoryAvailable;
            var result = Math.Round((memoryAvailable2 - memoryAvailable) / 1024f / 1024f, 2);
            var memoryAvailableNow = Math.Round(memoryAvailable2 / 1024f / 1024f / 1024f, 4);
            var memorySystem = Math.Round(systemInfo.PhysicalMemory / 1024f / 1024f / 1024f, 4);
            MessageBox.Show(this, $"清理内存{result}MB。\r\n当前可内存为{memoryAvailableNow}GB。\r\n总内存为{memorySystem}GB。", "", MessageBoxButtons.OK,
               MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// 简单枚举模拟协程
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            //ImageCache.ClearLoadStack();
            //_loadingImageUserControlDict.Clear();
            //imagePanel.Controls.Clear();
            //UpdateAllImageUserControl();
            Clear();
            ImageLabel[] imageLabels = null;

            if (imageLabelFlowLayoutPanel.Controls.Count != 1)
            {
                imageLabels = new ImageLabel[imageLabelFlowLayoutPanel.Controls.Count-1];
                for (var index = 0; index < imageLabels.Length; index++)
                {
                    imageLabels[index] = ((ImageLabelUserControl)imageLabelFlowLayoutPanel.Controls[index]).ImageLabel;

                }
            }
            var keywords = saerchTextBox.Text = saerchTextBox.Text;
            var orderBy = "";
            if (orderByTitleSkinRadioButton.Checked) orderBy = "title";
            else if (orderByAddTimeSkinRadioButton.Checked) orderBy = "add_time";
            else if (orderByEditTimeSkinRadioButton.Checked) orderBy = "edit_time";
            var page = 0;
            while (true)
            {
                AddMyImages(Dao.GetImages(orderBy, page, keywords, imageLabels, isAscSkinCheckBox.Checked));
                page++;
                yield return "";
            }

        }

        public void ImagePanel_MouseWheel(object sender, MouseEventArgs e)
        {
            ImagePanel_Scroll(sender, null);
        }

        /// <summary>
        /// 导入图片
        /// </summary>
        /// <param name="imgpaths"></param>
        public void ImportImages(string[] imgpaths)
        {
            var progressDialog = ProgressDialog.GetNewInstance(imgpaths);
            progressDialog.Show();

        }

        /// <summary>
        /// 删除图片控件，并更新界面
        /// </summary>
        /// <param name="imageUserControls"></param>
        public void RemoveImage(ImageUserControl[] imageUserControls)
        {
            var result = MessageBox.Show(this, $"是否确定要删除？\r\n* 这个操作不可逆！", "", MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                foreach (var imageUserControl in imageUserControls)
                {
                    Dao.RemoveImage(imageUserControl.MyImage);
                    imagePanel.Controls.Remove(imageUserControl);
                    if (imageUserControl.MyImage.Path.Contains("<ImgPath>"))
                    {
                        File.Delete(Utils.ConvertPath(imageUserControl.MyImage.Path));
                    }
                }

            }
            UpdateAllImageUserControl();
        }

        /// <summary>
        /// 搜索
        /// </summary>
        public void Search()
        {
            imagePanel.MouseWheel -= ImagePanel_MouseWheel;
            imagePanel.MouseWheel += ImagePanel_MouseWheel;
            enumerator = GetEnumerator();
            enumerator.MoveNext();
        }

        /// <summary>
        /// 显示最新添加的图片
        /// </summary>
        public void ShowLastAddImage(DateTime dateTime)
        {
            //while (enumerator.MoveNext()) {
            //    Debug.WriteLine(enumerator.Current);
            //}
            imagePanel.MouseWheel -= ImagePanel_MouseWheel;
            Clear();
            AddMyImages(Dao.GetLastAddImages(dateTime));

        }

        /// <summary>
        /// 更新所有图片组件
        /// </summary>
        public void UpdateAllImageUserControl()
        {
            var eachWidth = Settings.Default.ImageWidth + Settings.Default.ImagePadding;
            var panelWidth = imagePanel.Width;
            if (HorizontalScroll.Visible)
            {
                panelWidth -= 20;
            }
            var colNum = (panelWidth + Settings.Default.ImagePadding) / eachWidth;

            _columnsHeight = new int[colNum];
            foreach (ImageUserControl imageUserControl in imagePanel.Controls)
            {
                AddImageUserControl(imageUserControl);
            }
        }

        /// 
        /// 监视Windows消息
        /// 重载WndProc方法，用于实现热键响应
        /// 
        /// 
        protected override void WndProc(ref Message m)
        {

            const int WM_HOTKEY = 0x0312;
            //按快捷键 
            switch (m.Msg)
            {
                case WM_HOTKEY:
                    switch (m.WParam.ToInt32())
                    {
                        case 100:    //按下的是Ctrl+Alt+F4
                            ScreenShot();//此处填写快捷键响应代码 
                            break;

                    }
                    break;
            }
            base.WndProc(ref m);
        }

        private void AddFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var path = folderBrowserDialog.SelectedPath;
                ImportImages(new string[] { path });
            }
        }

        private void AddImageLabelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var allSelecteds = GetAllSelected();
            if (allSelecteds.Length == 0)
            {
                MessageBox.Show(this, "未选择任何标签！", "", MessageBoxButtons.OK,
              MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                var chooseLabelDialog = new ChooseLabelDialog();
                var result = chooseLabelDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    foreach (var imageUserControls in allSelecteds)
                    {
                        imageUserControls.AddImageLabel(chooseLabelDialog.ImageLabel);
                    }
                }
                chooseLabelDialog.Dispose();
            }
        }

        private void AddImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (openFileDialog.Filter == "")
            {
                var supportExtension = ImageReaderFactory.GetInstance().GetSupportExtension();
                var supportStringBuilder = new StringBuilder();
                foreach (var extension in supportExtension)
                {
                    supportStringBuilder.Append("*");
                    supportStringBuilder.Append(extension);
                    supportStringBuilder.Append(";");
                }
                openFileDialog.Filter = $"支持格式({supportStringBuilder})|{supportStringBuilder}";
            }
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var fileNames = openFileDialog.FileNames;
                ImportImages(fileNames);
            }
        }


        /// <summary>
        /// 添加MyImages
        /// </summary>
        /// <param name="myImages"></param>
        private void AddMyImages(MyImage[] myImages)
        {

            if (myImages != null)
            {
                if (myImages.Length != 0)
                {
                    // 挂起逻辑
                    imagePanel.SuspendLayout();
                }

                foreach (var myImage in myImages)
                {
                    var imageUserControl = new ImageUserControl(myImage);
                    _loadingImageUserControlList.Add(imageUserControl);
                    imageUserControl.ImgLabelNameLabel_ClickEventHander = ImgLabelNameLabel_ClickEvent;
                    imageUserControl.RemoveImageEventHandler = RemoveImageEvent;
                    imageUserControl.UpdateEventHander = UpdateEvent;

                    ImageCache.AddImage(myImage.Path);
                }
            }

        }
        private void ClearMemoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearMemory();
        }

        private void ImagePanel_Scroll(object sender, ScrollEventArgs e)
        {
            //Debug.WriteLine(imagePanel.VerticalScroll.Value+"-"+ imagePanel.VerticalScroll.Maximum+ "-" + imagePanel.VerticalScroll.LargeChange);
            if (imagePanel.VerticalScroll.Value + imagePanel.VerticalScroll.LargeChange >= imagePanel.VerticalScroll.Maximum)
            {
                if (_loadingImageUserControlList.Count == 0)
                {
                    enumerator.MoveNext();
                }
            }

        }

        private void ImagePanel_SizeChanged(object sender, EventArgs e)
        {

            if (imagePanel.Width < Settings.Default.ImageWidth + Settings.Default.ImagePadding)
            {
                imagePanel.Width = Settings.Default.ImageWidth + Settings.Default.ImagePadding;
            }
            UpdateAllImageUserControl();
        }

        /// <summary>
        /// 图片加载完成调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadImageFinish(string path, Image image)
        {
            //测试
            long t_startTime, t_endTime;
            t_startTime = DateTime.Now.Ticks;
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate ()
                {
                    LoadImageFinish(path, image);
                }));
                t_endTime = DateTime.Now.Ticks;
                Debug.WriteLine($"LoadImageFinish函数执行时间为{(t_endTime - t_startTime) / 10000}毫秒。");
            }
            else
            {
                for (var index = 0; index < _loadingImageUserControlList.Count; index++)
                {
                    if (Utils.ConvertPath(_loadingImageUserControlList[index].MyImage.Path).Equals(path))
                    {
                        var imageUserControl = _loadingImageUserControlList[index];
                        _loadingImageUserControlList.RemoveAt(index);
                        imageUserControl.SetImage(image);
                        AddImageUserControl(imageUserControl);
                        break;
                    }
                }
                //启动布局逻辑
                if (_loadingImageUserControlList.Count == 0)
                {
                    imagePanel.ResumeLayout();
                }
            }

        }
        #region 来自ImageUserControl类的上报的事件处理
        public void AddSearchLabel(ImageLabel imageLabel)
        {
            var newImageLabelUserControl = new ImageLabelUserControl(imageLabel);
            // 先删除添加搜索标签的按钮
            imageLabelFlowLayoutPanel.Controls.RemoveAt(imageLabelFlowLayoutPanel.Controls.Count - 1);
            imageLabelFlowLayoutPanel.Controls.Add(newImageLabelUserControl);
            // 再压入添加搜索标签的按钮
            imageLabelFlowLayoutPanel.Controls.Add(addSearchLabelSkinButton);

            newImageLabelUserControl.XLabel_ClickEventHandler = SearchLabelXLabel_ClickEventHandler;
            newImageLabelUserControl.ImgLabelNameLabel_ClickEventHander = null;
        }
        /// <summary>
        /// 图片名字标签被点击事件
        /// </summary>
        public void ImgLabelNameLabel_ClickEvent(object sender, ImageLabelUserControl imageLabelUserControl)
        {
            AddSearchLabel(imageLabelUserControl.ImageLabel);
        }

        public void RemoveImageEvent(object sender, ImageUserControl imageUserControl)
        {
            imagePanel.Controls.Remove(imageUserControl);
            UpdateAllImageUserControl();
        }

        /// <summary>
        /// 截图
        /// </summary>
        public void ScreenShot()
        {
            var captureWindow = new CaptureWindow();
            if ((bool)captureWindow.ShowDialog())
            {
                (new StickerForm(captureWindow.Bitmap)).Show();
            }
            captureWindow.Close();
        }

        /// <summary>
        /// 内容发生变动事件
        /// </summary>
        public void UpdateEvent(object sender, ImageUserControl imageUserControl)
        {
            UpdateAllImageUserControl();
        }

        /// <summary>
        /// 使用截图全局按键
        /// </summary>
        public void UseGlobalScreenShotHotKey()
        {
            if (useGlobalScreenShotHotKeyToolStripMenuItem.Checked)
            {
                screenShotToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+Shift+F5";
                HotKey.RegisterHotKey(Handle, 100, KeyModifiers.Ctrl | KeyModifiers.Shift, Keys.F5);
            }
            else
            {
                screenShotToolStripMenuItem.ShortcutKeyDisplayString = "";
                UnregisterHotKey(Handle, 100);
            }
        }

        public void SearchLabelXLabel_ClickEventHandler(object sender, ImageLabelUserControl imageLabelUserControl)
        {
            imageLabelFlowLayoutPanel.Controls.Remove(imageLabelUserControl);
        }
        private ImageUserControl[] GetAllSelected()
        {
            //var k = from r in imagePanel.Controls. where r.IsSelected() select r;
            var imageUserControlList = new LinkedList<ImageUserControl>();
            foreach (ImageUserControl imageUserControl in imagePanel.Controls)
            {
                if (imageUserControl.IsSelected())
                {
                    imageUserControlList.AddLast(imageUserControl);
                }
            }
            return imageUserControlList.ToArray();
        }

        #endregion
        private void MainForm_Activated(object sender, EventArgs e)
        {
            //注销Id号为100的热键设定
            //UnregisterHotKey(Handle, 100);

        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            Array fileArray = (Array)e.Data.GetData(DataFormats.FileDrop);
            String[] files = (String[])fileArray;
            ImportImages(files);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MainForm_Leave(object sender, EventArgs e)
        {
            //注册热键Ctrl+Alt+F4，Id号为100。功能为截图。
            //RegisterHotKey(Handle, 100, KeyModifiers.Ctrl, Keys.F4);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            Search();
        }

        private void PictureToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            if (GetAllSelected().Length == 0)
            {
                addImageLabelToolStripMenuItem.Enabled = false;
                removeImageToolStripMenuItem.Enabled = false;
            }
            else
            {
                addImageLabelToolStripMenuItem.Enabled = true;
                removeImageToolStripMenuItem.Enabled = true;
            }
        }

        private void RemoveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var imageUserControls = GetAllSelected();
            RemoveImage(imageUserControls);
        }

        private void ScreenShotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ScreenShot();
        }
        private void SearchSkinButton_Click(object sender, EventArgs e)
        {
            Search();

        }

        private void SelectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageUserControl imageUserControl in imagePanel.Controls)
            {
                imageUserControl.SetSelected(true);
            }
        }

        private void SelectCancelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageUserControl imageUserControl in imagePanel.Controls)
            {
                imageUserControl.SetSelected(false);
            }
        }

        private void SelectInvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ImageUserControl imageUserControl in imagePanel.Controls)
            {
                imageUserControl.SetSelected(!imageUserControl.IsSelected());
            }
        }
        private void UseGlobalScreenShotHotKeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.UseGlobalScreenShotHotKey = useGlobalScreenShotHotKeyToolStripMenuItem.Checked = !useGlobalScreenShotHotKeyToolStripMenuItem.Checked;
            Settings.Default.Save();
            UseGlobalScreenShotHotKey();
        }

        private void AddSearchSkinButton_Click(object sender, EventArgs e)
        {
            var chooseLabelDialog = new ChooseLabelDialog();
            var result = chooseLabelDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                var label = chooseLabelDialog.ImageLabel;
                AddSearchLabel(label);
            }
            chooseLabelDialog.Dispose();
        }
    }

}
