using ImageManager.Properties;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ImageManager
{
    /// <summary>
    /// 图像加载缓冲类
    /// </summary>
    public static class ImageCache
    {
        /// <summary>
        /// 加载队列
        /// </summary>
        private static ConcurrentStack<String> _loadStk = new ConcurrentStack<string>();
        /// <summary>
        /// 缓冲字典
        /// </summary>
        private static ConcurrentDictionary<String, Image> _cacheDict = new ConcurrentDictionary<string, Image>();
        /// <summary>
        /// 加载任务
        /// </summary>
        private static Task _loadTask;

        /// <summary>
        /// 取消加载任务标记
        /// </summary>
        public static CancellationTokenSource LoadingTokenSource = new CancellationTokenSource();
        /// <summary>
        /// 图片委托
        /// </summary>
        /// <param name="path"></param>
        /// <param name="image"></param>
        public delegate void ImageDelegate(String path, Image image);
        /// <summary>
        /// 加载完成事件通知
        /// </summary>
        public static ImageDelegate LoadFinishImageDelegate;

        /// <summary>
        /// 
        /// </summary>
        public static Image ErrorImage = Resources.ImageLoadError;

        /// <summary>
        /// 发生失败时等待的毫秒数
        /// </summary>
        private static readonly int _failSleepTime = 20;
        

        /// <summary>
        /// 添加需要加载的图片
        /// </summary>
        /// <param name="path">位置</param>
        public static void AddImage(string path)
        {
            _loadStk.Push(Utils.ConvertPath(path));
            if (_loadTask == null || _loadTask.Status == TaskStatus.Canceled || _loadTask.Status == TaskStatus.Faulted || _loadTask.Status == TaskStatus.RanToCompletion)
            {
                _loadTask = new Task(LoadImg, LoadingTokenSource.Token, TaskCreationOptions.LongRunning);
                _loadTask.Start();
            }
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        private static void LoadImg()
        {
            while (!_loadStk.IsEmpty)
            {
                // 取消
                if (LoadingTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }
                
                string path = null;
                while (!_loadStk.TryPop(out path))
                {
                    Thread.Sleep(_failSleepTime);
                }
                if (LoadingTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }
                Image image;
                if (!_cacheDict.ContainsKey(path))
                {
                    var imageReader = ImageReaderFactory.GetInstance().CreateImageReader(path);
                    image = imageReader.Read(path);
                    // 取消
                    if (LoadingTokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (image == null)
                    {
                        if (ErrorImage.Width != Settings.Default.ImageWidth)
                        {
                            ErrorImage = SuperImageReader.ZoomImage((Image)Resources.ImageLoadError.Clone());
                        }
                        image = ErrorImage;
                    }
                    else
                    {
                        image = SuperImageReader.ZoomImage(image);

                        // 取消
                        if (LoadingTokenSource.Token.IsCancellationRequested)
                        {
                            break;
                        }
                        if (Settings.Default.EnableImageCache)
                        {
                            while (!_cacheDict.TryAdd(path, image))
                            {
                                Thread.Sleep(_failSleepTime);
                            }
                        }
                    }
                }
                else
                {
                    image = _cacheDict[path];
                }
                
                //通知加载完成
                LoadFinishImageDelegate(path, image);
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        public static void Clear()
        {
            ClearLoadStack();
            _cacheDict.Clear();
        }

        /// <summary>
        /// 清空加载栈
        /// </summary>
        public static void ClearLoadStack()
        {
            if (_loadTask != null)
            {
                LoadingTokenSource.Cancel();
                //try
                //{
                //    _loadTask.Wait();
                //}
                //catch (AggregateException e)
                //{
                //    Debug.WriteLine(e);
                //}
                //catch (ObjectDisposedException e)
                //{
                //    Debug.WriteLine(e);
                //}
                _loadStk.Clear();
                LoadingTokenSource = new CancellationTokenSource();
            }
            
        }

        /// <summary>
        /// 删除指定路径缓存
        /// </summary>
        /// <param name="path"></param>
        public static void RemoveCache(MyImage myImage)
        {
            var path = Utils.ConvertPath(myImage.Path);
            _cacheDict.TryRemove(path,out var value);
        }
    }
}