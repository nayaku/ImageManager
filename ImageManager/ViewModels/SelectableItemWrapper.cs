using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.ViewModels
{
    public class SelectableItemWrapper<T> : PropertyChangedBase
    {
        public T Item { get; set; }
        public bool IsSelected { get; set; }
        public SelectableItemWrapper(T item)
        {
            Item = item;
        }

        public static implicit operator T(SelectableItemWrapper<T> wrapper)
        {
            return wrapper.Item;
        }
        public static implicit operator SelectableItemWrapper<T>(T item)
        {
            return new SelectableItemWrapper<T>(item);
        }
        

    }
}
