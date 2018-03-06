using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace AutoZhenJiu.entity
{
    class CommParam
    {
        public string port; //串口号
        public string baudrate; //波特率
        
        public CommParam()
        {
            port = "";
            baudrate = "";
            
        }

        public CommParam(JObject obj)
        {
             port = (string)obj["port"];
             baudrate = (string)obj["baudrate"];
        }

    }
}
