using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoZhenJiu.entity;
using AutoZhenJiu.fn;

namespace AutoZhenJiu
{
    public partial class FormCommParam : Form, IMessageFilter
    {
        CommParam sysparam;
        int lastInput;

        public FormCommParam()
        {
            InitializeComponent();
            //for test
            Application.AddMessageFilter(this);
            lastInput = Environment.TickCount;
            timer.Interval = 1000;

            timer.Tick += new EventHandler(watch);
            /*
            timer.Tick += delegate { 
                if(Environment.TickCount - lastInput > 5 * 1000){
                    //timer.Stop();
                    lastInput = Environment.TickCount;
                    Console.WriteLine("5s 未操作 --------------------------");
                }
            };*/

            //timer.Start();


            getCommParam();
            init();

            btnSave.Click += new EventHandler(save);

            this.FormClosing += new FormClosingEventHandler(doWhenClosing);
        }

        /// <summary>
        /// 观察，等待
        /// </summary>
        public void watch(object sender, EventArgs e) {
            if (Environment.TickCount - lastInput > 5 * 1000)
            {
                //timer.Stop();
                lastInput = Environment.TickCount;
                Console.WriteLine("5s 未操作 , 打开欢迎界面----------------");
                //openFormWelcome();
            }
        }


        /// <summary>
        /// 观察，30秒无操作就弹出待机界面
        /// </summary>
        private void openFormWelcome()
        {
            if (Application.OpenForms["FormWelcome"] == null)
            {
                Console.WriteLine("new welcome");
                new FormWelcome(2).Show();
            }
            else
            {
                Console.WriteLine("exist welcome");
                Form form = Application.OpenForms["FormWelcome"];
                form.Show();

            }
        }

        public bool PreFilterMessage(ref Message msg)
        {
            const int WM_LBUTTONDOWN = 0x201; //
            const int WM_KEYDOWN = 0x100;
            switch(msg.Msg){
                case WM_LBUTTONDOWN:
                case WM_KEYDOWN:
                case 522:
                case 512:
                    lastInput = Environment.TickCount;
                   
                    Console.WriteLine("事件：" + msg.Msg);
                    break;
            }
            return false;
        }

        private void FormSysParam_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public void getCommParam()
        {
            string paramStr = new FnFile().getCommParam();
            if (paramStr.Trim().Length != 0)
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(paramStr);
                sysparam = new CommParam(obj);
            }
            else
            {
                sysparam = new CommParam();
            }
        }

        /// <summary>
        /// 为各输入框初始化内容
        /// </summary>
        public void init()
        {
            for(int i = 0; i < cbxPort.Items.Count; i ++){
                if(cbxPort.Items[i].ToString().Equals(sysparam.port)){
                    cbxPort.SelectedIndex = i;
                }
            }

            for (int i = 0; i < cbxBaudrate.Items.Count; i++)
            {
                if (cbxBaudrate.Items[i].ToString().Equals(sysparam.baudrate))
                {
                    cbxBaudrate.SelectedIndex = i;
                }
            }

        }

        /// <summary>
        /// 检查内容是否填写
        /// </summary>
        public bool checkValid()
        {
            if (cbxPort.SelectedIndex < 0){ MessageBox.Show("请选择串口号"); return false; }
            if (cbxBaudrate.SelectedIndex < 0){ MessageBox.Show("请选择波特率"); return false; }
            return true;
        }

         /// <summary>
        /// 保存配置
        /// </summary>
        public void save(object sender, EventArgs e)
        {
            if (!checkValid()) { return; }

            CommParam sysparam = new CommParam();
            sysparam.port = cbxPort.SelectedItem.ToString();
            sysparam.baudrate = cbxBaudrate.SelectedItem.ToString();

            string str = JsonConvert.SerializeObject(sysparam);

            new FnFile().toCommParam(str);
            MessageBox.Show("保存成功");
            this.Close();
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void doWhenClosing(object sender, EventArgs e)
        {
            Console.WriteLine("closing ，timer停止");
            timer.Stop();
            Application.RemoveMessageFilter(this);
        }

    }
}
