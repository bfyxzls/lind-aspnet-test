using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace WEBAPI.Core
{
    public class JsonHelper
    {
        private static object lockObj = new object();
        /// <summary>
        /// 将对象转为Json字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SerializeObject<T>(T obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        /// <summary>
        /// 将字符串转为对象(转换失败直接new一个对象,不返回null)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeDefaultObject<T>(string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 将字符串转为对象(转换失败返回null)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeObject<T>(string value) where T : class
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 将字符串转为匿名对象，调用方式：DeserializeAnonymousType(value, new { a = 1, b = "" });
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject) where T : class
        {
            try
            {
                return JsonConvert.DeserializeAnonymousType(value, anonymousTypeObject);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

       

        #region Binary
        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] SerializeToBinary(object value)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            MemoryStream memStream = new MemoryStream();
            memStream.Seek(0, 0);
            serializer.Serialize(memStream, value);
            return memStream.ToArray();
        }
        /// <summary>
        /// 二进制反序列化
        /// </summary>
        /// <param name="someBytes"></param>
        /// <returns></returns>
        public static object DeserializeFromBinary(byte[] someBytes)
        {
            IFormatter bf = new BinaryFormatter();
            object res = null;
            if (someBytes == null)
                return null;
            using (var memoryStream = new MemoryStream())
            {
                memoryStream.Write(someBytes, 0, someBytes.Length);
                memoryStream.Seek(0, 0);
                memoryStream.Position = 0;
                res = bf.Deserialize(memoryStream);
            }
            return res;

        }
        #endregion
    }
}
