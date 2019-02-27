using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ImageManager
{
    /// <summary>
    /// 生产图片类的工厂
    /// </summary>
    public class ImageReaderFactory
    {
        /// <summary>
        /// 自身实例
        /// </summary>
        private static ImageReaderFactory _instance = null;
        private Dictionary<String, Type> _supportDict = new Dictionary<string, Type>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private ImageReaderFactory()
        {
            var types = Assembly.GetCallingAssembly().GetTypes();
            var aType = typeof(SuperImageReader);
            //Debug.Log(aType.FullName);
            //List<A> alist = new List<A>();
            foreach (var type in types)
            {
                var baseType = type.BaseType;  //获取基类
                while (baseType != null)  //获取所有基类
                {
                    //Debug.Log(baseType.Name);
                    if (baseType.Name == aType.Name)
                    {
                        // 获取子类的支持文件类型
                        var field = type.GetField("SupportImageTypes");
                        var suports = field.GetValue(null) as string[];
                        Type objtype = Type.GetType(type.FullName, true);
                        foreach (var sp_ext in suports)
                        {
                            _supportDict[sp_ext.ToLower()] = objtype;
                        }
                        break;
                    }
                    else
                    {
                        baseType = baseType.BaseType;
                    }
                }

            }
        }

        /// <summary>
        /// 获取自身实例
        /// </summary>
        /// <returns></returns>
        public static ImageReaderFactory GetInstance()
        {
            if (_instance == null)
                return _instance = new ImageReaderFactory();
            return _instance;
        }

        /// <summary>
        /// 创建新的图片读取器
        /// </summary>
        /// <param name="path">路径</param>
        public SuperImageReader CreateImageReader(string path)
        {
            path = Utils.ConvertPath(path);
            var ext = Path.GetExtension(path);
            
            if (_supportDict.ContainsKey(ext.ToLower()))
            {
                Type objtype = _supportDict[ext.ToLower()];
                object obj = Activator.CreateInstance(objtype);
                var reader = obj as SuperImageReader;
                return reader;

            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 是否支持
        /// </summary>
        /// <param name="path">路径</param>
        public bool IsSupport(string path)
        {
            var ext = Path.GetExtension(path);
            return _supportDict.ContainsKey(ext.ToLower());
        }
        public string[] GetSupportExtension()
        {
            return _supportDict.Keys.ToArray();
        }
    }
}