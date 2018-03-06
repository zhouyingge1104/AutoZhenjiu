using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace AutoZhenJiu.fn
{
    class FnString
    {
        public FnString() { }

        /// <summary>
        /// 发送指令
        /// </summary>o
        public void fillZeroAtHead(string str, int count)
        {
            for (int i = 0; i < count; i ++ )
            {
                str = ("0" + str);
            }
        }

        /// <summary>
        /// 获取字符串的MD5
        /// </summary>
        public string toMD5(string str)
        {
            byte[] data = Encoding.GetEncoding("GB2312").GetBytes(str);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] outBytes = md5.ComputeHash(data);
            string outStr = "";
            for (int i = 0; i < outBytes.Length; i ++ )
            {
                outStr += outBytes[i].ToString("x2");
            }
            return outStr.ToLower();

        }

    }
}
