using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using AutoZhenJiu.entity;

namespace AutoZhenJiu
{
    class Global
    {
        /// <summary>
        /// 全局使用的SerialPort
        /// </summary>
        public static SerialPort sp;

        /// <summary>
        /// 全局使用的用来接收返回数据的数组
        /// </summary>
        public static int[] resp;

        public static string respStr;

        /// <summary>
        /// 当前客户，一般在刷了身份证之后设定当前用户
        /// </summary>
        public static Client client;

    }
}
