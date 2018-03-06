using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoZhenJiu.entity
{
    public class MissionPoint
    {
        public string no; //穴位标记，如F1， F2，B1， B2等
        public bool isVibrate;
        public int time; //时间
        public int timeFixed; //时间（备用）
        public Button btn;

        public MissionPoint()
        {
            no = "";
            isVibrate = false;
            time = 0;
            timeFixed = 0;
        }

    }
}
