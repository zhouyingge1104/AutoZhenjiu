using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AutoZhenJiu.fn
{
    class FnFile
    {
        const string CONFIG1_FILE_NAME = "config1.json",
                         CONFIG2_FILE_NAME = "config2.json",
                         COMMPARAM_FILE_NAME = "commparam.json",
                         SCHEME_FILE_NAME = "scheme.json";

        public FnFile(){}

        /// <summary>
        /// 获取...
        /// </summary>
        public string fromFile(string fileName)
        {
            string str = "";
            FileInfo p = new FileInfo(System.Environment.CurrentDirectory + "\\" + fileName);
            if (!p.Exists)
            {
                p.Create().Close(); //【写blog 这里必须加Close() ，否则下一步会提示被占用】
            }
            StreamReader reader = new StreamReader(p.FullName);
            str = reader.ReadToEnd();
            reader.Close();
            return str;
        }

        /// <summary>
        /// 写入...
        /// </summary>
        public void toFile(string str, string fileName)
        {
            FileInfo p = new FileInfo(System.Environment.CurrentDirectory + "\\" + fileName);
            if (!p.Exists)
            {
                p.Create().Close(); //【写blog 这里必须加Close() ，否则下一步会提示被占用】
            }
            StreamWriter writer = new StreamWriter(p.FullName);
            writer.WriteLine(str);
            writer.Close();
        }

        //具体****************************************************

        /// <summary>
        /// 获取配置 config1.json
        /// </summary>
        public string getConfig1()
        {
            return fromFile(CONFIG1_FILE_NAME);
        }

        /// <summary>
        /// 写入配置文件 config2.json
        /// </summary>
        public void toConfig2(string str)
        {
            toFile(str, CONFIG2_FILE_NAME);
        }

        /// <summary>
        /// 获取配置 config2.json
        /// </summary>
        public string getConfig2()
        {
            return fromFile(CONFIG2_FILE_NAME);
        }

        /// <summary>
        /// 写入配置文件 config1.json
        /// </summary>
        public void toConfig1(string str)
        {
            toFile(str, CONFIG1_FILE_NAME);
        }

        /// <summary>
        /// 获取系统参数 commparam.json
        /// </summary>
        public string getCommParam()
        {
            return fromFile(COMMPARAM_FILE_NAME);
        }

        /// <summary>
        /// 写入系统参数 sysparam.json
        /// </summary>
        public void toCommParam(string str)
        {
            toFile(str, COMMPARAM_FILE_NAME);
        }

        /// <summary>
        /// 获取配置 scheme.json
        /// </summary>
        public string getScheme()
        {
            return fromFile(SCHEME_FILE_NAME);
        }

        /// <summary>
        /// 写入配置文件 scheme.json
        /// </summary>
        public void toScheme(string str)
        {
            toFile(str, SCHEME_FILE_NAME);
        }

    }
}
