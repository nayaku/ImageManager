using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageManager
{
    public partial class ProgressDialog : Form
    {
        /// <summary>
        /// 自身实例
        /// </summary>
        private static ProgressDialog _instance;
        /// <summary>
        /// 路径
        /// </summary>
        private string[] _paths;
        /// <summary>
        /// 加载任务
        /// </summary>
        private Task _workTask;
        /// <summary>
        /// 取消任务
        /// </summary>
        private Task _cancelTask;
        /// <summary>
        /// 拷贝到图片库
        /// </summary>
        private bool _isCopyToLib;
        /// <summary>
        /// 操作开始时间
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// 取消加载任务标记
        /// </summary>
        public CancellationTokenSource WorkingTokenSource = new CancellationTokenSource();

        private ProgressDialog(string[] paths)
        {
            _paths = paths;
            InitializeComponent();
            var result = MessageBox.Show(this,$"是否复制图片到图片库？\r\n图片库的地址为{Settings.Default.ImageLibPath}。\r\n * 强烈建议选择是！", "", MessageBoxButtons.YesNo,
               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            if(result==DialogResult.Yes)
            {
                _isCopyToLib = true;
            }
            else
            {
                _isCopyToLib = false;
            }

            skinProgressBar.Value = 0;
            _workTask = new Task(DoWork,WorkingTokenSource.Token);
            _workTask.Start();
        }

        public static ProgressDialog GetNewInstance(string[] paths)
        {
            if(_instance == null)
            {
                return _instance = new ProgressDialog(paths);
            }
            MessageBox.Show(null, $"已经存在一个正在运行的添加图片的操作！", "", MessageBoxButtons.OK,
               MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            return _instance;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        /// <param name="msg"></param>
        public void ShowMessage(string msg)
        {
            messageRichTextBox.BeginInvoke((MethodInvoker)delegate
            {
                messageRichTextBox.AppendText(msg + "\r\n");
                messageRichTextBox.ScrollToCaret();
            });
        }

        /// <summary>
        /// 增加进度条的数值
        /// </summary>
        /// <param name="value"></param>
        public void IncreaseProgressBarValue(int value)
        {
            skinProgressBar.Invoke((MethodInvoker)delegate
            {
                skinProgressBar.Value += value;
            });
        }

        /// <summary>
        /// 运行
        /// </summary>
        public void DoWork()
        {
            // 测试用时
            long tw_startTime, tw_endTime;
            //Dao.UpdateTokenSource = new CancellationTokenSource();
            
            //if (Dao.HasUpdated == false)
            //{
            //    Dao.UpdateMessageEventHander = (object sender, string msg) =>
            //    {
            //        ShowMessage(msg);

            //    };
            //    Dao.UpdateImageMD5();
            //}
            IncreaseProgressBarValue(30);
            _startTime = DateTime.Now;
            //取消
            if (WorkingTokenSource.Token.IsCancellationRequested)
            {
                return;
            }
            tw_startTime = DateTime.Now.Ticks;

            foreach (var file in _paths)
            {
                AddAllFiles(file);
                //取消
                if (WorkingTokenSource.Token.IsCancellationRequested)
                {
                    return;
                }
            }
            tw_endTime = DateTime.Now.Ticks;
            
            //Debug.WriteLine($"函数运行花费{(tw_endTime - tw_startTime)/10000}毫秒。其中获取MD5码花费了{Dao.t_md5Time / 10000}，读数据库花费了{Dao.t_readDatabaseTime / 10000}，插入数据花费了{t_insertDatabaseTime / 10000}毫秒。");

            IncreaseProgressBarValue(70);
            ShowMessage("添加成功！");
            ShowMessage("结束！");
            BeginInvoke((MethodInvoker)delegate
            {
                var result = MessageBox.Show(this, $"图片添加完成！\r\n是否立即在主窗口显示？", "", MessageBoxButtons.YesNo,
               MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if(result == DialogResult.Yes)
                {
                    MainForm.GetInstance().ShowLastAddImage(_startTime);
                }
                _workTask.Wait();
                Close();
                
            });
        }

        /// <summary>
        /// 取消
        /// </summary>
        public void Cancel()
        {
            ShowMessage("收到取消指令，正在准备取消中！");
            Dao.UpdateTokenSource.Cancel();
            WorkingTokenSource.Cancel();
            _workTask.Wait();
            var myImages = Dao.GetLastAddImages(_startTime);
            ShowMessage($"准备进行逆向操作，一共需要逆向{myImages.Length}个文件。");
            foreach(var myImage in myImages)
            {
                ShowMessage($"正在删除{myImage.Path}图片的数据。");
                Dao.RemoveImage(myImage);
            }
            BeginInvoke((MethodInvoker)delegate
            {
                _cancelTask.Wait();
                Close();

            });
        }
        long t_insertDatabaseTime = 0;
        private void AddAllFiles(string filePathString)
        {
            FileInfo info = new FileInfo(filePathString);
            if ((info.Attributes & FileAttributes.Directory) != 0)
            {
                string[] fileStrings = Directory.GetFileSystemEntries(filePathString);

                foreach (string file in fileStrings)
                {
                    //取消
                    if (WorkingTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    AddAllFiles(file);
                    //取消
                    if (WorkingTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
            else
            {
                if (File.Exists(filePathString))
                {
                    if (ImageReaderFactory.GetInstance().IsSupport(filePathString))
                    {
                        var title = Utils.ConvertToValidString(Path.GetFileNameWithoutExtension(filePathString));
                        if (_isCopyToLib)
                            filePathString = "<ImgLib>\\" + Utils.CopyToLib(filePathString);
                        // 测试代码
                        long t_startTime, t_endTime;
                        
                        (bool iei,string md5) = Dao.IsExistImage(filePathString);
                        t_startTime = DateTime.Now.Ticks;
                        if (iei)
                        {
                            ShowMessage($"图片{filePathString}已经存在于数据库中！");
                        }
                        else
                        {
                            Dao.AddImage(title, filePathString,md5);
                        }
                        t_endTime = DateTime.Now.Ticks;
                        t_insertDatabaseTime += t_endTime - t_startTime;
                        ShowMessage($"添加图片{filePathString}成功！");
                        if (WorkingTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                }
            }

        }
        private void CancelSkinButton_Click(object sender, EventArgs e)
        {
            
            
            if(_workTask.Status == TaskStatus.Running)
            {
                var result = MessageBox.Show(this, $"是否确认取消？\r\n * 注意取消以后的回溯操作需要一段时间，并且不能再选择取消！", "", MessageBoxButtons.YesNo,
               MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    cancelSkinButton.Enabled = false;
                    _cancelTask = new Task(Cancel);
                    _cancelTask.Start();
                }
                if (e is FormClosingEventArgs)
                {
                    ((FormClosingEventArgs)e).Cancel = true;
                }
            }
            else if(_cancelTask!=null && _cancelTask.Status == TaskStatus.Running)
            {
                if (e is FormClosingEventArgs)
                {
                    ((FormClosingEventArgs)e).Cancel = true;
                }
            }
            else
            {

            }
            
        }


        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(e.CloseReason == CloseReason.TaskManagerClosing || e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.WindowsShutDown)
            {
                CancelSkinButton_Click(sender, e);
            }
            _instance = null;
            
        }
        //private new void Dispose()
        //{
        //    MainForm.GetInstance().Invoke((MethodInvoker)delegate { MainForm.GetInstance().Search(); });
        //    Dispose(true);
        //    GC.SuppressFinalize(this);

        //}

    }
}
