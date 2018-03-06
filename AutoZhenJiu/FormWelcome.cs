using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoZhenJiu
{
    public partial class FormWelcome : Form, IMessageFilter
    {
        public const int FORM_AUTO = 0, FORM_MANUAL = 1;
        public int source;
        //第一次512不动，第二次开始处理
        int event512Counter;
        int lastInput;

        public FormWelcome(int source)
        {
            InitializeComponent();
            this.source = source;
            //for test
            Application.AddMessageFilter(this);
            //timerWelcome.Tick += new EventHandler(watch);
        }

        private void FormWelcome_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 观察，30秒无操作就弹出待机界面
        /// </summary>
        public void watch(object sender, EventArgs e)
        {
            Console.WriteLine("TimerWelcome Tick----------------");
            if (Environment.TickCount - lastInput > 10 * 1000)
            {
                lastInput = Environment.TickCount;
                Console.WriteLine("5s 未操作 , 打开欢迎界面----------------");
                hide();
            }
        }

        /// <summary>
        /// 重新show的时候做点事情
        /// </summary>
        public void showAgain()
        {
            //timerWelcome.Start();
            Application.AddMessageFilter(this);
        }

        public bool PreFilterMessage(ref Message msg)
        {
            Console.WriteLine("欢迎界面 事件：" + msg.Msg);
            const int WM_LBUTTONDOWN = 0x201; //
            const int WM_RBUTTONDOWN = 0x00A4; //
            const int WM_KEYDOWN = 0x100;
            switch (msg.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_KEYDOWN:
                case 522:
                //case 512:
                    
                    
                   // event512Counter++;
                    //实测，打开的时候时候会有一次默认的512，所以第一次不处理，第二次开始处理
                   // if(event512Counter > 1){
                   //     hide();
                   // }
                    hide();
                    break;
            }
            return false;
        }

        private void hide()
        {
            this.Hide();
            event512Counter = 0;
            //timerWelcome.Stop();
            Application.RemoveMessageFilter(this);
            Form form;
            switch(source){
                case FORM_AUTO:
                    form = Application.OpenForms["FormAuto"];
                    ((FormAuto)form).showAgain();
                    break;
                case FORM_MANUAL: 
                     form = Application.OpenForms["FormManual"];
                     ((FormManual)form).showAgain();
                    break;
            }
           
        }

    }
}
