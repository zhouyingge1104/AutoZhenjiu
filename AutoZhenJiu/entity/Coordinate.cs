using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutoZhenJiu.entity
{
    /// <summary>
    /// 一套方案的坐标参数
    /// </summary>
    class Coordinate
    {
        public string frontX, frontY, frontTime, backX, backY, backTime;

        public Coordinate()
        {
            frontX = "0,0,0,0,0,0,0,0";
            frontY = "0,0,0,0,0,0,0,0";
            frontTime = "0,0,0,0,0,0,0,0";
            backX = "0,0,0,0,0,0,0,0";
            backY = "0,0,0,0,0,0,0,0";
            backTime = "0,0,0,0,0,0,0,0";
        }

        public Coordinate(JObject obj)
        {
            frontX = (string)obj["frontX"];
            frontY = (string)obj["frontY"];
            frontTime = (string)obj["frontTime"];
            backX = (string)obj["backX"];
            backY = (string)obj["backY"];
            backTime = (string)obj["backTime"];
        }

        /// <summary>
        /// 返回json字符串
        /// </summary>
        /// <returns></returns>
        public string toString(){
            Coordinate cdnt = new Coordinate();
            cdnt.frontX = this.frontX;
            cdnt.frontY = this.frontY;
            cdnt.frontTime = this.frontTime;
            cdnt.backX = this.backX;
            cdnt.backY = this.backY;
            cdnt.backTime = this.backTime;

            Console.WriteLine("- " + JsonConvert.SerializeObject(cdnt));
            Console.WriteLine("-");
            Console.WriteLine("-");

            return JsonConvert.SerializeObject(this);
        }

    }
}
