using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using AutoZhenJiu.fn;

namespace AutoZhenJiu.entity
{
    /// <summary>
    /// 保存在下位机的参数
    /// </summary>
    class Config2
    {
        string DEFAULT_PWD = "12345";

        public string tempXZ; //温度修正
        public int tempHC; //温度回差
        public int bodyTemp; //体表温度上限
        public int distance; //体表保持距离
        public int delay; //点火延时
        public int fanSpeed; //风扇速度
        public int horDGLength; //水平导轨长度
        public int verDGLength; //垂直导轨长度
        public int horSpeedQ; //水平启停速度-启
        public int horSpeedT; //水平启停速度-停
        public int verSpeedQ; //垂直启停速度-启
        public int verSpeedT; //垂直启停速度-停
        public int vibrateRange; //振动幅度
        public int vibrateSpeedQ; //振动启停速度-启
        public int vibrateSpeedT; //振动启停速度-停
        public int vibrateDelay; //振动延时

        public int loadSpeedQ; //装载启停速度-启
        public int loadSpeedT; //装载启停速度-停

        public int ctrlCode; //遥控器编码

        public int gateDelay; //舱门开启延时

        public bool autoUpload; //系统参数是否自动上报

        public string password; //访问密码（设备参数）
        public string password2; //访问密码（使用许可）2018.3.6，“设备参数”和“使用许可”分开用两套密码

        public Config2()
        {
            tempXZ = "0,0,0,0"; //温度修正
            tempHC = 0; //温度回差
            bodyTemp = 0;
            distance = 0;
            delay = 0;
            fanSpeed = 0;
            horDGLength = 0;
            verDGLength = 0;
            horSpeedQ = 0;
            horSpeedT = 0;
            verSpeedQ = 0;
            verSpeedT = 0;
            vibrateRange = 0;
            vibrateSpeedQ = 0;
            vibrateSpeedT = 0;
            vibrateDelay = 0;
            loadSpeedQ = 0;
            loadSpeedT = 0;
            ctrlCode = 0;
            gateDelay = 0;
            autoUpload = false;
            password = new FnString().toMD5(DEFAULT_PWD);
            password2 = new FnString().toMD5(DEFAULT_PWD);
        }

        public Config2(JObject obj)
        {
            tempXZ = obj["tempXZ"] != null ? (string)obj["tempXZ"] : "0,0,0,0";
            tempHC = obj["tempHC"] != null ? (int)obj["tempHC"] : 0;
            bodyTemp =  obj["bodyTemp"] != null ? (int)obj["bodyTemp"] : 0;
            distance = obj["distance"] != null ? (int)obj["distance"] : 0;
            delay = obj["delay"] != null ? (int)obj["delay"] : 0;
            fanSpeed = obj["fanSpeed"] != null ? (int)obj["fanSpeed"] : 0;
            horDGLength = obj["horDGLength"] != null ? (int)obj["horDGLength"] : 0;
            verDGLength = obj["verDGLength"] != null ? (int)obj["verDGLength"] : 0;
            horSpeedQ = obj["horSpeedQ"] != null ? (int)obj["horSpeedQ"] : 0;
            horSpeedT = obj["horSpeedT"] != null ? (int)obj["horSpeedT"] : 0;
            verSpeedQ = obj["verSpeedQ"] != null ? (int)obj["verSpeedQ"] : 0;
            verSpeedT = obj["verSpeedT"] != null ? (int)obj["verSpeedT"] : 0;
            vibrateRange = obj["vibrateRange"] != null ? (int)obj["vibrateRange"] : 0;
            vibrateSpeedQ = obj["vibrateSpeedQ"] != null ? (int)obj["vibrateSpeedQ"] : 0;
            vibrateSpeedT = obj["vibrateSpeedT"] != null ? (int)obj["vibrateSpeedT"] : 0;

            vibrateDelay = obj["vibrateDelay"] != null ? (int)obj["vibrateDelay"] : 0;
            loadSpeedQ = obj["loadSpeedQ"] != null ? (int)obj["loadSpeedQ"] : 0;
            loadSpeedT = obj["loadSpeedT"] != null ? (int)obj["loadSpeedT"] : 0;

            ctrlCode = obj["ctrlCode"] != null ? (int)obj["ctrlCode"] : 0;
            gateDelay = obj["gateDelay"] != null ? (int)obj["gateDelay"] : 0;

            autoUpload = obj["autoUpload"] != null ? (bool)obj["autoUpload"] : false;

            password = obj["password"] != null ? (string)obj["password"] : new FnString().toMD5(DEFAULT_PWD);
            password2 = obj["password2"] != null ? (string)obj["password2"] : new FnString().toMD5(DEFAULT_PWD);
        }

    }
}
