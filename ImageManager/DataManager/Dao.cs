using ImageManager.Properties;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace ImageManager
{
    /// <summary>
    /// 数据库连接封装类
    /// </summary>
    public static class Dao
    {
        /// <summary>
        /// 保存的数据库连接
        /// </summary>
        private static SQLiteConnection _connnect;

        /// <summary>
        /// 标签字典
        /// </summary>
        private static readonly Dictionary<string, ImageLabel> _ImgLabelDict = new Dictionary<string, ImageLabel>();

        /// <summary>
        /// 图片字典
        /// </summary>
        private static readonly Dictionary<Int64, MyImage> _ImgDict = new Dictionary<Int64, MyImage>();

        /// <summary>
        /// 更新消息事件
        /// </summary>
        public static EventHandler<String> UpdateMessageEventHander;

        /// <summary>
        /// 是否已经更新过
        /// </summary>
        public static bool HasUpdated = false;

        /// <summary>
        /// 取消加载任务标记
        /// </summary>
        public static CancellationTokenSource UpdateTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 构造函数
        /// </summary>
        static Dao()
        {
            ConnectDataBase();
        }

        /// <summary>
        /// 连接数据库 启动时会自动连接
        /// </summary>
        public static void ConnectDataBase()
        {
            String path = Settings.Default.ImageLibPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(Settings.Default.ImageLibPath);
            }
            var databaseFileName = path + "\\database.sqlite";
            var needCreateDatabase = false;
            if (!File.Exists(databaseFileName))
            {
                SQLiteConnection.CreateFile(databaseFileName);
                needCreateDatabase = true;
            }
            _connnect = new SQLiteConnection($"Data Source={databaseFileName};Version=3;Synchronous=OFF;Journal Mode=Memory;");
            _connnect.Open();
            if (needCreateDatabase)
            {
                string sql = Resources.CreateDatabaseSql;
                using (var command = new SQLiteCommand
                {
                    Connection = _connnect,
                    CommandText = Resources.CreateDatabaseSql
                })
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        ///// <summary>
        ///// 初始化Setting
        ///// </summary>
        //private static void InitSetting()
        //{
        //    //var sql = "SELECT settings.key AS settings_key, settings.value AS settings_value FROM settings ";
        //    //var command = new SQLiteCommand(sql, _connnect);
        //    //var reader = command.ExecuteReader();
        //    //var keyOrdId = reader.GetOrdinal("settings_key");
        //    //var valueOrdId = reader.GetOrdinal("settings_value");
        //    //while (reader.NextResult())
        //    //{
        //    //    var key = reader.GetString(keyOrdId);
        //    //    var value = reader.GetString(keyOrdId);
        //    //    if (key.Equals("image_libaray_path"))
        //    //    {
        //    //        Setting.ImageLibPath = value;
        //    //    }
        //    //    else if (key.Equals("image_width"))
        //    //    {
        //    //        Setting.ImageWidth = int.Parse(value);
        //    //    }
        //    //}

        //}

        ///// <summary>
        ///// 设置设置
        ///// </summary>
        ///// <param name="key">键</param>
        ///// <param name="value">值</param>
        //private static void SetSetting(string key, object value)
        //{
        //    var sql = $"UPDATE settings SET @key = @value";
        //    var command = new SQLiteCommand(sql, _connnect);
        //    command.Parameters.AddWithValue("@key", key);
        //    command.Parameters.AddWithValue("@value", value);
        //    var reader = command.ExecuteNonQuery();
        //}

        /// <summary>
        /// 获取最新添加的图片
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static MyImage[] GetLastAddImages(DateTime dateTime)
        {
            var sql = "SELECT path, title, id FROM images WHERE add_time >= @add_time ";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@add_time", dateTime);
                using (var reader = command.ExecuteReader())
                {
                    var myImageLinkedList = new LinkedList<MyImage>();
                    var pathOrd = reader.GetOrdinal("path");
                    var titleOrd = reader.GetOrdinal("title");
                    var idOrd = reader.GetOrdinal("id");

                    while (reader.Read())
                    {
                        var id = reader.GetInt64(idOrd);
                        var path = reader.GetString(pathOrd);
                        var title = reader.GetString(titleOrd);
                        myImageLinkedList.AddLast(_GetMyImageFromDB(id, title, path));
                    }
                    var myImages = new MyImage[myImageLinkedList.Count];
                    myImageLinkedList.CopyTo(myImages, 0);
                    return myImages;
                }
            }
        }

        /// <summary>
        /// 查找所有图片
        /// </summary>
        /// <param name="orderby">    排序关键列</param>
        /// <param name="pageIndex">  页号</param>
        /// <param name="keywords">   关键词</param>
        /// <param name="imageLabels">包含的标签</param>
        /// <param name="isAsc">      是否为升序</param>
        public static MyImage[] GetImages(string orderby = "title", int pageIndex = 0, string keywords = "", ImageLabel[] imageLabels = null, bool isAsc = true)
        {
            var sqliteParameterList = new LinkedList<SQLiteParameter>();
            var pageNum = Settings.Default.PageNumber;
            var sql = "";
            // 时候限定检索标签
            if (imageLabels != null)
            {
                sql = "SELECT images.id AS images_id, images.path AS images_path, images.title AS images_title FROM images JOIN label_contain ON images.id = label_contain.image_id JOIN labels ON label_contain.label_id = labels.id WHERE ( ";
                var imgLabelStringBuilder = new StringBuilder();
                for (var index = 0; index < imageLabels.Length; index++)// (var imgLabel in imageLabels)
                {
                    var imgLabel = imageLabels[index];
                    imgLabelStringBuilder.Append($" labels.name = @imgLabel_Name_{index}");
                    if (index != imageLabels.Length - 1)
                        imgLabelStringBuilder.Append($" OR");
                    sqliteParameterList.AddLast(new SQLiteParameter($"@imgLabel_Name_{index}", imgLabel.Name));
                }
                sql += imgLabelStringBuilder.ToString() + " ) AND ";
            }
            else
            {
                sql = "SELECT images.id AS images_id, images.path AS images_path, images.title AS images_title FROM images WHERE ";
            }
            // 根据关键词检索
            keywords = keywords.Trim();
            var keywordArray = keywords.Split(new char[] { ' ' });
            var keywordStringBuilder = new StringBuilder();
            if (keywordArray.Length == 1)
            {
                keywordStringBuilder.Append($"images.title LIKE '%'||@word||'%' ");
                sqliteParameterList.AddLast(new SQLiteParameter("@word", keywordArray[0]));
            }
            else
            {
                keywordStringBuilder.Append("(");
                for (var index = 0; index < keywordArray.Length; index++)//foreach(var word in keywordArray)
                {
                    var word = keywordArray[index];

                    if (index != keywordArray.Length - 1)
                    {
                        keywordStringBuilder.Append($"images.title LIKE '%'||@word_{index}||'%' OR ");
                        sqliteParameterList.AddLast(new SQLiteParameter($"@word_{index}", word));
                    }
                    else
                    {
                        keywordStringBuilder.Append($"images.title LIKE '%'||@word_{index}||'%' ");
                        sqliteParameterList.AddLast(new SQLiteParameter($"@word_{index}", word));
                    }
                }
                keywordStringBuilder.Append(")");
            }
            sql += keywordStringBuilder.ToString();
            // 限定标签
            if (imageLabels != null)
            {
                sql += "GROUP BY images.id HAVING count(label_contain.label_id) = " + imageLabels.Length;
            }
            sql += $" ORDER BY {orderby} {(isAsc ? "ASC" : "DESC")} ";
            //sqliteParameterList.AddLast(new SQLiteParameter("@orderby", orderby));
            sql += $"LIMIT @start, @pageNum";
            sqliteParameterList.AddLast(new SQLiteParameter("@start", pageIndex * pageNum));
            sqliteParameterList.AddLast(new SQLiteParameter("@pageNum", pageNum));
            //Debug.WriteLine("查询的SQL语句为：" + sql);
            //var sql = $"SELECT images.id, images.path, images.title FROM images LIMIT {pageIndex*pageNum}, {pageNum}";

            //var sql= "SELECT images.id, images.path, images.title FROM images JOIN label_contain ON images.id = label_contain.image_id JOIN labels ON label_contain.label_id = labels.id WHERE images.title LIKE '%[aa|bb]%' AND labels.name in ('aaa','bbb') LIMIT 0, 10";

            var myImageLinkedList = new LinkedList<MyImage>();
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                var sqliteParameters = new SQLiteParameter[sqliteParameterList.Count];
                sqliteParameterList.CopyTo(sqliteParameters, 0);
                command.Parameters.AddRange(sqliteParameters);
                using (var reader = command.ExecuteReader())
                {
                    var imgIdOrd = reader.GetOrdinal("images_id");
                    var imgTitleOrd = reader.GetOrdinal("images_title");
                    var imgPathOrd = reader.GetOrdinal("images_path");

                    while (reader.Read())
                    {
                        var imgId = reader.GetInt64(imgIdOrd);

                        MyImage img;
                        if (!_ImgDict.ContainsKey(imgId))
                        {
                            img = _GetMyImageFromDB(imgId);
                        }
                        else
                        {
                            img = _ImgDict[imgId];
                        }
                        myImageLinkedList.AddLast(img);
                    }
                }
                var imgArray = new MyImage[myImageLinkedList.Count];
                myImageLinkedList.CopyTo(imgArray, 0);
                return imgArray;
            }
        }

        /// <summary>
        /// 从数据库读取MyImage或者根据指定的值来创建MyImage
        /// </summary>
        /// <param name="id">   </param>
        /// <param name="title"></param>
        /// <param name="path"> </param>
        /// <returns></returns>
        private static MyImage _GetMyImageFromDB(Int64 id, String title = null, String path = null)
        {
            var sql = "";
            if (title == null || path == null)
            {
                sql = $"SELECT images.id AS images_id, images.path AS images_path, images.title AS images_title FROM images WHERE images.id = @id ";
                using (var command = new SQLiteCommand(sql, _connnect))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            title = reader.GetString(reader.GetOrdinal("images_title"));
                            path = reader.GetString(reader.GetOrdinal("images_path"));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            sql = $"SELECT labels.id AS labels_id, labels.name AS labels_name, labels.color AS labels_color, labels.num AS labels_num FROM labels JOIN label_contain ON  label_contain.label_id = labels.id WHERE label_contain.image_id = @id";
            using (var labelCommand = new SQLiteCommand(sql, _connnect))
            {
                labelCommand.Parameters.AddWithValue("@id", id);
                using (var labelReader = labelCommand.ExecuteReader())
                {
                    var labelLinkedList = new LinkedList<ImageLabel>();
                    var labIdOrd = labelReader.GetOrdinal("labels_id");
                    var labNameOrd = labelReader.GetOrdinal("labels_name");
                    var labColorOrd = labelReader.GetOrdinal("labels_color");
                    var labNumOrd = labelReader.GetOrdinal("labels_num");
                    while (labelReader.Read())
                    {
                        var labName = labelReader.GetString(labNameOrd);
                        ImageLabel label;
                        if (!_ImgLabelDict.ContainsKey(labName))
                        {
                            label = _GetImageLabelFromDB(name: labName);
                        }
                        else
                        {
                            label = _ImgLabelDict[labName];
                        }
                        labelLinkedList.AddLast(label);
                    }
                    var labelArray = new ImageLabel[labelLinkedList.Count];
                    labelLinkedList.CopyTo(labelArray, 0);
                    Array.Sort(labelArray);

                    return _ImgDict[id] = new MyImage(id, title, path, labelArray);
                }
            }
        }

        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="labelName">标签名</param>
        /// <returns>如果标签不存在返回null</returns>
        public static ImageLabel GetImageLabel(string labelName)
        {
            if (_ImgLabelDict.ContainsKey(labelName))
                return _ImgLabelDict[labelName];
            return _GetImageLabelFromDB(name: labelName);
        }

        /// <summary>
        /// 根据关键词获取标签
        /// </summary>
        /// <param name="key">关键词</param>
        public static ImageLabel[] GetImageLabels(string key)
        {
            var sql = $"SELECT labels.id AS labels_id , labels.name AS labels_name, labels.color AS labels_color, labels.num AS labels_num FROM labels WHERE labels.name LIKE '%'||@key||'%'";
            using (var labelCommand = new SQLiteCommand(sql, _connnect))
            {
                labelCommand.Parameters.AddWithValue("@key", key);
                using (var labelReader = labelCommand.ExecuteReader())
                {
                    var labIdOrd = labelReader.GetOrdinal("labels_id");
                    var labNameOrd = labelReader.GetOrdinal("labels_name");
                    var labColorOrd = labelReader.GetOrdinal("labels_color");
                    var labNumOrd = labelReader.GetOrdinal("labels_num");
                    var labelLinkedList = new LinkedList<ImageLabel>();
                    while (labelReader.Read())
                    {
                        var name = labelReader.GetString(labNameOrd);
                        ImageLabel label;
                        if (!_ImgLabelDict.ContainsKey(name))
                            label = _GetImageLabelFromDB(labelReader.GetInt64(labIdOrd), labelReader.GetString(labNameOrd), labelReader.GetInt32(labColorOrd), labelReader.GetInt64(labNumOrd));
                        else
                            label = _ImgLabelDict[name];
                        labelLinkedList.AddLast(label);
                    }
                    var labelArray = new ImageLabel[labelLinkedList.Count];
                    labelLinkedList.CopyTo(labelArray, 0);
                    Array.Sort(labelArray);
                    return labelArray;
                }
            }
        }

        /// <summary>
        /// 从数据库读取标签或者根据所给的数值建立标签
        /// </summary>
        /// <param name="id">   </param>
        /// <param name="name"> </param>
        /// <param name="color"></param>
        /// <param name="num">  </param>
        /// <returns>如果标签不存在返回null</returns>
        private static ImageLabel _GetImageLabelFromDB(Int64 id = -1, string name = null, int color = 0, Int64 num = 0)
        {
            SQLiteParameter sQLiteParameter;
            var sql = "";
            if (name == null || id == -1)
            {
                sql = "SELECT labels.id AS labels_id, labels.name AS labels_name, labels.color AS labels_color, labels.num AS labels_num FROM labels WHERE ";
                if (id != -1)
                {
                    sql += $"labels.id = @id ";
                    sQLiteParameter = new SQLiteParameter("@id", id);
                }
                else if (name != null)
                {
                    sql += $"labels.name = @name ";
                    sQLiteParameter = new SQLiteParameter("@name", name);
                }
                else
                    return null;
                using (var command = new SQLiteCommand(sql, _connnect))
                {
                    command.Parameters.Add(sQLiteParameter);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            id = reader.GetInt64(reader.GetOrdinal("labels_id"));
                            name = reader.GetString(reader.GetOrdinal("labels_name"));
                            color = reader.GetInt32(reader.GetOrdinal("labels_color"));
                            num = reader.GetInt64(reader.GetOrdinal("labels_num"));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            return _ImgLabelDict[name] = new ImageLabel(id, name, Color.FromArgb(color), num);
        }

        /// <summary>
        /// 修改图片标题
        /// </summary>
        /// <param name="myImage">需要改名的图片</param>
        /// <param name="title">  更改后的标题</param>
        public static void RenameImage(MyImage myImage, string title)
        {
            var sql = $"UPDATE images SET title=@title WHERE images.id = @myImage_ID";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@myImage_ID", myImage.ID);
                command.ExecuteNonQuery();
                myImage.RenameTitle(title);
            }
        }

        /// <summary>
        /// 更新数据库内文件的MD5码
        /// </summary>
        /// <param name="isUpdateAll">true表示更新所有图片，false表示只更新修改日期改变的图片。默认问false</param>
        public static void UpdateImageMD5(bool isUpdateAll = false)
        {
            var sql = "SELECT images.id AS images_id, images.path AS images_path, images.edit_time AS images_edit_time FROM images";
            using (var command0 = new SQLiteCommand(sql, _connnect))
            {
                using (var reader = command0.ExecuteReader())
                {
                    var idOrdId = reader.GetOrdinal("images_id");
                    var pathOrdId = reader.GetOrdinal("images_path");
                    var editTimeOrdId = reader.GetOrdinal("images_edit_time");
                    while (reader.NextResult())
                    {
                        var id = reader.GetInt64(idOrdId);
                        var path = Utils.ConvertPath(reader.GetString(pathOrdId));
                        var editTime = reader.GetDateTime(editTimeOrdId);
                        UpdateMessageEventHander(null, $"正在检查文件'{path}'");
                        if (File.Exists(path) && (new FileInfo(path).LastWriteTime.CompareTo(editTime) != 0))
                        {
                            var md5 = Utils.GetMD5ByHashAlgorithm(path);
                            var updateSql = $"UPDATE images SET md5=@md5 WHERE images.id = @id";
                            using (var command = new SQLiteCommand(sql, _connnect))
                            {
                                command.Parameters.AddWithValue("@md5", md5);
                                command.Parameters.AddWithValue("@id", id);
                                command.ExecuteNonQuery();
                                UpdateMessageEventHander(null, $"更新文件的MD5完成。");
                                if (UpdateTokenSource.Token.IsCancellationRequested)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    HasUpdated = true;
                }
            }
        }

        // 测试
        public static long t_md5Time = 0;
        public static long t_readDatabaseTime = 0;
        /// <summary>
        /// 判断图片时候存在
        /// </summary>
        /// <param name="path"></param>
        /// <param name="md5"> </param>
        /// <returns>存在返回true。此外还返回md5码。</returns>
        public static (bool, string) IsExistImage(string path, string md5 = null)
        {
            long t_startTime, t_endTime;
            if (md5 == null)
            {
                t_startTime = DateTime.Now.Ticks;
                md5 = Utils.GetMD5ByHashAlgorithm(Utils.ConvertPath(path));
                t_endTime = DateTime.Now.Ticks;
                t_md5Time += t_endTime - t_startTime;
            }
            t_startTime = DateTime.Now.Ticks;
            var sql = $"select id from images where images.md5 = @md5 limit 0,1";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@md5", md5);
                using (var reader = command.ExecuteReader())
                {
                    t_endTime = DateTime.Now.Ticks;
                    t_readDatabaseTime += t_endTime - t_startTime;
                    if (!reader.HasRows)
                        return (false, md5);
                    else
                        return (true, md5);
                }
            }

        }

        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="path"> 路径</param>
        /// <param name="md5">  </param>
        public static void AddImage(string title, string path, string md5)
        {
            var fileInfo = new FileInfo(Utils.ConvertPath(path));
            //if (md5 == null) md5 = Utils.GetMD5Hash(path);
            var sql = $"INSERT INTO images(title,path,add_time,edit_time,md5) VALUES(@title ,@path ,@time ,@fileInfo_LastWriteTime , @md5 )";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@path", path);
                command.Parameters.AddWithValue("@time", DateTime.Now);
                command.Parameters.AddWithValue("@fileInfo_LastWriteTime", fileInfo.LastWriteTime);
                command.Parameters.AddWithValue("@md5", md5);
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 添加一组图片到数据库。
        /// 使用前必须判断先调用IsExistImage函数判断添加的图片在数据库中不存在。
        /// </summary>
        /// <param name="image">二维数组每行必须包含4列：标题、路径、md5(前面3列为字符串)、最后修改时间(DateTime类型)。</param>
        public static void AddImages(object[,] image)
        {

        }

        /// <summary>
        /// 移除图片
        /// </summary>
        /// <param name="myImage">需要移除的图片</param>
        public static void RemoveImage(MyImage myImage)
        {
            // 尝试删除缩略图缓冲
            ImageCache.RemoveCache(myImage);

            var sql = $"DELETE FROM images WHERE images.id = @myImage_id";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@myImage_id", myImage.ID);
                command.ExecuteNonQuery();
                if (_ImgDict.ContainsKey(myImage.ID))
                    _ImgDict.Remove(myImage.ID);
                if (myImage.Path.Contains("<ImgLib>"))
                {
                    File.Delete(Utils.ConvertPath(myImage.Path));
                }
            }
        }

        /// <summary>
        /// 查看图片是否已经拥有标签
        /// </summary>
        /// <param name="myImage">   </param>
        /// <param name="imageLabel"></param>
        /// <returns></returns>
        public static bool HasImageLabel(MyImage myImage, ImageLabel imageLabel)
        {
            var sql = "SELECT label_id, image_id FROM label_contain WHERE image_id = @image_id AND label_id = @label_id LIMIT 0,1";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@image_id", myImage.ID);
                command.Parameters.AddWithValue("@label_id", imageLabel.ID);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// 给图片添加标签
        /// </summary>
        /// <param name="myImage">   图片</param>
        /// <param name="imageLabel">标签</param>
        public static void AddImageLabel(MyImage myImage, ImageLabel imageLabel)
        {
            var sql = $"INSERT INTO label_contain(label_id,image_id) VALUES(@imageLabel_ID , @myImage_ID)";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@myImage_ID", myImage.ID);
                command.Parameters.AddWithValue("@imageLabel_ID", imageLabel.ID);
                command.ExecuteNonQuery();
                //myImage.AddImageLabel(imageLabel);
                imageLabel.ChangeNum(imageLabel.Num + 1);
            }
        }

        /// <summary>
        /// 建立新的标签
        /// </summary>
        /// <param name="name"> 标签名</param>
        /// <param name="color">颜色</param>
        /// <returns>添加成功返回true</returns>
        public static bool CreateImageLabel(string name, Color color)
        {
            if (GetImageLabel(name) == null)
            {
                var sql = $"INSERT INTO labels(name,color) VALUES(@name, @color)";
                var command = new SQLiteCommand(sql, _connnect);
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@color", color.ToArgb());
                command.ExecuteNonQuery();
                return true;
            }
            else
            {
                return false;
            }
        }

        ///// <summary>
        ///// 修改标签名字
        ///// </summary>
        ///// <param name="imgLabel">图片标签</param>
        ///// <param name="name">修改后的名字</param>
        //public static void RenameImageLabel(ImageLabel imgLabel, string name)
        //{
        //    var sql = $"UPDATE labels SET name='{name}' WHERE labels.id = {imgLabel}";
        //    new SQLiteCommand(sql, _connnect).ExecuteNonQuery();

        //}

        /// <summary>
        /// 重设标签颜色
        /// </summary>
        /// <param name="imgLabel">图片标签</param>
        /// <param name="color">   修改后的颜色</param>
        public static void ResetImageLabelColor(ImageLabel imgLabel, Color color)
        {
            var sql = $"UPDATE labels SET color=@color WHERE labels.id = @imgLabel_ID";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@color", color.ToArgb());
                command.Parameters.AddWithValue("@imgLabel_ID", imgLabel.ID);
                command.ExecuteNonQuery();
                imgLabel.ChangeColor(color);
            }
        }

        /// <summary>
        /// 解绑图片与标签
        /// </summary>
        /// <param name="myImage"> 图片</param>
        /// <param name="imgLabel">标签</param>
        public static void UnlinkImageLabel(MyImage myImage, ImageLabel imgLabel)
        {
            var sql = $"DELETE FROM label_contain WHERE label_contain.label_id = @imgLabel_ID AND label_contain.image_id = @myImage_ID";
            using (var command = new SQLiteCommand(sql, _connnect))
            {
                command.Parameters.AddWithValue("@imgLabel_ID", imgLabel.ID);
                command.Parameters.AddWithValue("@myImage_ID", myImage.ID);
                command.ExecuteNonQuery();
                imgLabel.ChangeNum(imgLabel.Num - 1);
                myImage.RemoveImageLabel(imgLabel);
                if (imgLabel.Num == 0)
                {
                    sql = $"DELETE FROM labels WHERE id = @id";
                    using (var command2 = new SQLiteCommand(sql, _connnect))
                    {
                        command.Parameters.AddWithValue("@id", imgLabel.ID);
                        _ImgLabelDict.Remove(imgLabel.Name);
                    }
                }
            }
        }

        /// <summary>
        /// 清空所有缓冲
        /// </summary>
        public static void Clear()
        {
            _ImgDict.Clear();
            _ImgLabelDict.Clear();
        }
    }
}