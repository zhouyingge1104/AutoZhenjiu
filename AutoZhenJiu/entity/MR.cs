using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoZhenJiu.entity
{
    class MR
    {
        public int id;
        public int clientId;
        public string mr;
        public string addTime;
        public string date1;
        public string doctor;

        public MR()
        {
            id = -1;
            mr = "";
            addTime = "";
            date1 = "";
            doctor = "";
        }

    }
}
