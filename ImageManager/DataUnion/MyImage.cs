using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageManager
{
    /// <summary>
    /// 封装了添加到管理器的图片的类
    /// </summary>
    public class MyImage
    {
        /// <summary>
        /// 更新事件
        /// </summary>
        public EventHandler UpdateEventHandler;

        /// <summary>
        /// 拥有的标签
        /// </summary>
        private ImageLabel[] _labels;

        /// <summary>
        /// 所在的路径
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 编号
        /// </summary>
        public Int64 ID { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">编号</param>
        /// <param name="title">标题</param>
        /// <param name="path">路径</param>
        /// <param name="labels">标签</param>
        public MyImage(Int64 id, string title, string path, ImageLabel[] labels)
        {
            ID = id;
            Title = title;
            Path = path;
            _labels = labels;
        }

        /// <summary>
        /// 获取指定的标签
        /// </summary>
        /// <param name="index">下标</param>
        public ImageLabel GetImageLabel(int index)
        {
            return _labels[index];
        }

        /// <summary>
        /// 获取标签的数量
        /// </summary>
        public int GetImageLabelNum()
        {
            return _labels.Length;
        }
        public bool HasImageLabel(ImageLabel imageLabel)
        {
            foreach(var label in _labels)
            {
                if(label == imageLabel)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 修改标题
        /// </summary>
        /// <param name="title"></param>
        public void RenameTitle(string title)
        {
            Title = title;
            UpdateEventHandler(this, new EventArgs());
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="imageLabel"></param>
        public void RemoveImageLabel(ImageLabel imageLabel)
        {
            var labels = new ImageLabel[_labels.Length - 1];
            int count = 0;
            foreach(var label in _labels)
            {
                if(imageLabel != label)
                {
                    labels[count] = label;
                }
            }
            _labels = labels;
            UpdateEventHandler(this, new EventArgs());
        }

        public void AddImageLabel(ImageLabel imageLabel)
        {
            Dao.AddImageLabel(this, imageLabel);
            var len = _labels.Length;
            Array.Resize(ref _labels, len + 1);
            _labels[len] = imageLabel;
            UpdateEventHandler(this, new EventArgs());
        }

    }
}