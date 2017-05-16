using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS
{
    public class Common
    {
        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 取整数的某一位
        /// </summary>
        /// <param name="_Resource">要取某一位的整数</param>
        /// <param name="_Mask">要取的位置索引，自右至左为0-7</param>
        /// <returns>返回某一位的值（0或者1）</returns>
        public static int getIntegerSomeBit(int _Resource, int _Mask)
        {
            return _Resource >> _Mask & 1;
        }

        /// <summary>
        /// 将整数的某位置为0或1
        /// </summary>
        /// <param name="_Mask">整数的某位</param>
        /// <param name="a">整数</param>
        /// <param name="flag">是否置1，TURE表示置1，FALSE表示置0</param>
        /// <returns>返回修改过的值</returns>
        public static int setIntegerSomeBit(int _Mask, int a, bool flag)
        {
            if (flag)
            {
                a |= (0x1 << _Mask);
            }
            else
            {
                a &= ~(0x1 << _Mask);
            }
            return a;
        }

        /// <summary>
        /// 遍历一个类的所有属性和值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string getProperties<T>(T t)
        {
            string tStr = string.Empty;
            if (t == null)
            {
                return tStr;
            }
            System.Reflection.PropertyInfo[] properties = t.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            if (properties.Length <= 0)
            {
                return tStr;
            }
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                string name = item.Name;
                object value = item.GetValue(t, null);
                if (item.PropertyType.IsValueType || item.PropertyType.Name.StartsWith("String"))
                {
                    tStr += string.Format("{0}:{1},", name, value);
                }
                else
                {
                    getProperties(value);
                }
            }
            return tStr;
        }
    }
}
