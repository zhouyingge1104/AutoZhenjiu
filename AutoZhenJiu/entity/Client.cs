using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoZhenJiu.entity
{
    class Client
    {
        public int id;
        public string name;
        public string gender;
        public string height;
        public string weight;
        public string idCardNo;
        public string addTime;
        public string param;
        public int age;
        public string phoneNo;

        public Client()
        {
            id = -1;
            name = "";
            gender = "";
            height = "";
            weight = "";
            idCardNo = "";
            addTime = "";
            param = "";
            age = 1;
            phoneNo = "";
        }

    }
}
