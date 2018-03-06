using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoZhenJiu.entity;


namespace AutoZhenJiu.fn
{
    class FnConfig
    {

        public FnConfig() { }

        //要不要把getConfig1() 和 getConfig2()也放进来


        /// <summary>
        /// 根据选定的方案刷新config1
        /// </summary>o
        public void refreshConfig1(Config1 config1, int sNo)
        {
            Scheme scheme;
            string schemeStr = new FnFile().getScheme();
            if (schemeStr.Trim().Length == 0)
            {
                scheme = new Scheme();
            }
            else
            {
                scheme = new Scheme((JObject)JsonConvert.DeserializeObject(schemeStr));
            }
            
            Coordinate cdnt = new Coordinate();
            string str = "";
            switch(sNo){
               case Scheme.S1: str = scheme.s1; break;
               case Scheme.S2: str = scheme.s2; break;
               case Scheme.S3: str = scheme.s3; break;
               case Scheme.S4: str = scheme.s4; break;
               case Scheme.S5: str = scheme.s5; break;
            }

            cdnt = new Coordinate((JObject)JsonConvert.DeserializeObject(str));

           config1.frontX = cdnt.frontX;
           config1.frontY = cdnt.frontY;
           config1.frontTime = cdnt.frontTime;
           config1.backX = cdnt.backX;
           config1.backY = cdnt.backY;
           config1.backTime = cdnt.backTime;

        }

        /// <summary>
        /// 根据选定的方案刷新Scheme中对应的坐标
        /// </summary>o
        public void refreshScheme(Scheme scheme, Config1 config1, int sNo)
        {
            Coordinate cdnt = new Coordinate();
            cdnt.frontX = config1.frontX;
            cdnt.frontY = config1.frontY;
            cdnt.frontTime = config1.frontTime;
            cdnt.backX = config1.backX;
            cdnt.backY = config1.backY;
            cdnt.backTime = config1.backTime;

            switch (sNo)
            {
                case Scheme.S1:
                    scheme.s1 = cdnt.toString();
                    break;
                case Scheme.S2:
                    scheme.s2 = cdnt.toString();
                    break;
                case Scheme.S3:
                    scheme.s3 = cdnt.toString();
                    break;
                case Scheme.S4:
                    scheme.s4 = cdnt.toString(); 
                    break;
                case Scheme.S5:
                    scheme.s5 = cdnt.toString();
                    break;
            }

        }

    }
}
