using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoZhenJiu.entity
{
    /// <summary>
    /// 五套方案的坐标参数
    /// </summary>
    class Scheme
    {
        public const int S1 = 0, S2 = 1, S3 = 2, S4 = 3, S5 = 4;
        public string s1, s2, s3, s4, s5; 

        public Scheme()
        {
            s1 = new Coordinate().toString(); 
            s2 = new Coordinate().toString(); 
            s3 = new Coordinate().toString(); 
            s4 = new Coordinate().toString(); 
            s5 = new Coordinate().toString();
            Console.WriteLine("s1:" + s1);
            Console.WriteLine("s2:" + s2);
        }

        public Scheme(JObject obj)
        {
            s1 = (string)obj["s1"];
            s2 = (string)obj["s2"];
            s3 = (string)obj["s3"];
            s4 = (string)obj["s4"];
            s5 = (string)obj["s5"];
        }

    }
}
