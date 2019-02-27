using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ImageManager
{
    public class ImageLabel :IComparable<ImageLabel>
    {
        /// <summary>
        /// 更新事件
        /// </summary>
        public EventHandler UpdateEventHandler;
        public Int64 ID { get; }
        public string Name { get; }
        public Color Color { get; private set; }
        public Int64 Num { get; private set; }

        public ImageLabel(Int64 id, string name, Color color, Int64 num)
        {
            ID = id;
            Name = name;
            Color = color;
            Num = num;
        }

        /// <summary>
        /// 更改标签颜色
        /// </summary>
        /// <param name="color"></param>
        public void ChangeColor(Color color)
        {
            Color = color;
            UpdateEventHandler?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// 更改数量
        /// </summary>
        /// <param name="num"></param>
        public void ChangeNum(Int64 num)
        {
            Num = num;
            UpdateEventHandler?.Invoke(this, new EventArgs());
        }

        public static bool operator ==(ImageLabel u1, ImageLabel u2)
        {
            if(u1 is null)
            {
                if (u2 is null) return true;
                else return false;
            }
            return u1.Equals(u2);
        }
        public static bool operator !=(ImageLabel u1, ImageLabel u2)
        {
            return !(u1 == u2);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            return Name.Equals(((ImageLabel)obj).Name);

        }
        public override string ToString()
        {
            return Name;

        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <summary>
        /// 标签排序器，按照num从大到小排序
        /// </summary>
        /// 
        public static int Compare(ImageLabel x, ImageLabel y)
        {
            if (x.Num > y.Num)
                return 1;
            else if (x.Num == y.Num)
                return -1;
            else
                return 0;
        }

        public int CompareTo(ImageLabel other)
        {
            return Compare(this, other);
        }
    }
   
}