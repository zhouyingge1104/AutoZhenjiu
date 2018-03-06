using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoZhenJiu.entity
{
    /// <summary>
    /// 保存在上位机的参数
    /// </summary>
    class Config1
    {
        public string frontX; //正面的X坐标，形式如："1,3,16,22,17,98,232"，放在一个字符串中
        public string frontY;
        public string frontTime;
        public string frontZH; //穴位名称
        public string backX;
        public string backY;
        public string backTime;
        public string backZH; //穴位名称
        public string tempHighLimit; //温度上限，形式如："28, 85, 83, 89"，放在一个字符串中
        public string tempLowLimit;
        public string tempSD; //温度设定
        
        public int totalTime; //针灸总时间
        public bool autoTurnOn;

        public Config1()
        {
            frontX = ",,,,,,,"; 
            frontY = ",,,,,,,";
            frontTime = ",,,,,,,";
            frontZH = ",,,,,,,";
            backX = ",,,,,,,";
            backY = ",,,,,,,";
            backTime = ",,,,,,,";
            backZH = ",,,,,,,";
            tempHighLimit = ",,,";
            tempLowLimit = ",,,";
            tempSD = ",,,";
            totalTime = 0;
            autoTurnOn = false;
        }

        public Config1(JObject obj)
        {
            frontX = obj["frontX"] != null ? (string)obj["frontX"] : "0,0,0,0,0,0,0,0";
            frontY = obj["frontY"] != null ? (string)obj["frontY"] : "0,0,0,0,0,0,0,0";
            frontTime = obj["frontTime"] != null ? (string)obj["frontTime"] : "0,0,0,0,0,0,0,0";
            frontZH = obj["frontZH"] != null ? (string)obj["frontZH"] : "-,-,-,-,-,-,-,-";
            backX = obj["backX"] != null ? (string)obj["backX"] : "0,0,0,0,0,0,0,0";
            backY = obj["backY"] != null ? (string)obj["backY"] : "0,0,0,0,0,0,0,0";
            backTime = obj["backTime"] != null ? (string)obj["backTime"] : "0,0,0,0,0,0,0,0";
            backZH = obj["backZH"] != null ? (string)obj["backZH"] : "-,-,-,-,-,-,-,-";
            tempHighLimit = obj["tempHighLimit"] != null ? (string)obj["tempHighLimit"] : "0,0,0,0";
            tempLowLimit = obj["tempLowLimit"] != null ? (string)obj["tempLowLimit"] : "0,0,0,0";
            tempSD = obj["tempSD"] != null ? (string)obj["tempSD"] : "0,0,0,0";
            totalTime = obj["totalTime"] != null ? (int)obj["totalTime"] : 0;
            autoTurnOn = obj["autoTurnOn"] != null ? (bool)obj["autoTurnOn"] : false;
        }

    }
}
