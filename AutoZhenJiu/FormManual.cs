using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using AutoZhenJiu.fn;
using AutoZhenJiu.entity;


namespace AutoZhenJiu
{
    public partial class FormManual : Form, IMessageFilter
    {

        const int TYPE_F = 1, TYPE_B = 0;

        bool isOrderOn; //是否有命令在运行
        bool isCommOver; //交互是否停止

        Config1 config1;
        Order ORDER;
        static FnComm fnComm;
        static int currOrder, currExpLength;

        bool isWaiting; //是否在等待

        bool isBedIning, isBedOuting; //床进床出状态
        bool isGateOpening, isGateClosing; //舱门开关状态
        bool isCoverOpening, isCoverClosing; //舱盖开关状态
        bool isHeating; //是否在加热

        bool isVibrating; //是否正在振动

        string log; //输出在界面上的信息

        int lastInput;

        int totalTime; //艾灸总时间（从配置信息中取）
        int currZJTime; //当前穴位时间

        string[] frontX, backX, 
                   frontY, backY,
                   frontTime, backTime, 
                   frontZH, backZH;
        Hashtable xw_time; //穴位_时间（按钮_时间）

        int passedTime; //已运行时间

        EventHandler h1, h2, h3, h4, h5, h6, h7, h8, h9, h10, h11, h12, h13, h14, h15, h16, h17, h18, h19, h20, h26_28, h27_28, h29_30;

        //指令1每次的实际指令
        byte[] currOrder1Real;
        
        //功能按钮组，用来进行一些统一的处理
        Button[] btnTeam1;

        int[] fx, bx; //正面8个穴位的X坐标，反面8个穴位的x坐标
        Button currBtn; //代表当前穴位的按钮
        int F_B; //正面或反面（0：正面，1：反面）

        int testX; //模拟的水平步进电机位置

        SoundPlayer player;

        public FormManual()
        {
            InitializeComponent();

            setFormSize();

            //for test
            Application.AddMessageFilter(this);
            lastInput = Environment.TickCount;
            timerWelcome.Tick += new EventHandler(watch);
            //timerWelcome.Start();

            getConfig1();
            ORDER = new Order().init();
            fnComm = new FnComm();

            init();

            makeTeams();
            addEvents();

            //for test
            btnConfigSys.Click += new EventHandler(openFormConfig);

            btnConfig1.Click += new EventHandler(openFormConfig);
            btnCommParam.Click += new EventHandler(openFormCommParam);
            btnTXSZ.Click += new EventHandler(openFormCommParam);

            btnTurnOn.Click += new EventHandler(order2);

            btnHeat.Click += new EventHandler(order29_30);

            /*
            btnBedIn.MouseDown += new MouseEventHandler(order4);
            btnBedOut.MouseDown += new MouseEventHandler(order5);
            btnBedIn.MouseUp += new MouseEventHandler(order3);
            btnBedOut.MouseUp += new MouseEventHandler(order3);
            */

            btnBedIn.Click += new EventHandler(order3_4);
            btnBedOut.Click += new EventHandler(order3_5);

            /*
            btnGateOpen.MouseDown += new MouseEventHandler(order7);
            btnGateClose.MouseDown += new MouseEventHandler(order8);
            btnGateOpen.MouseUp += new MouseEventHandler(order6);
            btnGateClose.MouseUp += new MouseEventHandler(order6);
            */
              
            btnGateOpen.Click += new EventHandler(order6_7);
            btnGateClose.Click += new EventHandler(order6_8);

            btnCoverOpen.Click += new EventHandler(order26_28);
            btnCoverClose.Click += new EventHandler(order27_28);

            btnFan0.Click += new EventHandler(order9);
            btnFan1.Click += new EventHandler(order9);
            btnFan2.Click += new EventHandler(order9);
            btnFan3.Click += new EventHandler(order9);

            btnLeft.MouseDown += new MouseEventHandler(order11);
            btnRight.MouseDown += new MouseEventHandler(order12);
            btnLeft.MouseUp += new MouseEventHandler(order10);
            btnRight.MouseUp += new MouseEventHandler(order10);

            btnUp.MouseDown += new MouseEventHandler(order14);
            btnDown.MouseDown += new MouseEventHandler(order15);
            btnUp.MouseUp += new MouseEventHandler(order13);
            btnDown.MouseUp += new MouseEventHandler(order13);

            btnTurnOff.Click += new EventHandler(order19);

            btnEdit.Click += new EventHandler(edit);
            btnBack.Click += new EventHandler(back);
            btnBK.Click += new EventHandler(back);

            //18.2.28 从FormAuto搬过来的
            btnF1.Click += delegate(Object o, EventArgs e) { order18(btnF1, TYPE_F, 1); changeCurrBtn(btnF1); };
            btnF2.Click += delegate(Object o, EventArgs e) { order18(btnF2, TYPE_F, 2); changeCurrBtn(btnF2); };
            btnF3.Click += delegate(Object o, EventArgs e) { order18(btnF3, TYPE_F, 3); changeCurrBtn(btnF3); };
            btnF4.Click += delegate(Object o, EventArgs e) { order18(btnF4, TYPE_F, 4); changeCurrBtn(btnF4); };
            btnF5.Click += delegate(Object o, EventArgs e) { order18(btnF5, TYPE_F, 5); changeCurrBtn(btnF5); };
            btnF6.Click += delegate(Object o, EventArgs e) { order18(btnF6, TYPE_F, 6); changeCurrBtn(btnF6); };
            btnF7.Click += delegate(Object o, EventArgs e) { order18(btnF7, TYPE_F, 7); changeCurrBtn(btnF7); };
            btnF8.Click += delegate(Object o, EventArgs e) { order18(btnF8, TYPE_F, 8); changeCurrBtn(btnF8); };

            btnB1.Click += delegate(Object o, EventArgs e) { order18(btnB1, TYPE_B, 1); changeCurrBtn(btnB1); };
            btnB2.Click += delegate(Object o, EventArgs e) { order18(btnB2, TYPE_B, 2); changeCurrBtn(btnB2); };
            btnB3.Click += delegate(Object o, EventArgs e) { order18(btnB3, TYPE_B, 3); changeCurrBtn(btnB3); };
            btnB4.Click += delegate(Object o, EventArgs e) { order18(btnB4, TYPE_B, 4); changeCurrBtn(btnB4); };
            btnB5.Click += delegate(Object o, EventArgs e) { order18(btnB5, TYPE_B, 5); changeCurrBtn(btnB5); };
            btnB6.Click += delegate(Object o, EventArgs e) { order18(btnB6, TYPE_B, 6); changeCurrBtn(btnB6); };
            btnB7.Click += delegate(Object o, EventArgs e) { order18(btnB7, TYPE_B, 7); changeCurrBtn(btnB7); };
            btnB8.Click += delegate(Object o, EventArgs e) { order18(btnB8, TYPE_B, 8); changeCurrBtn(btnB8); };

            //18.2.28 从FormAuto搬过来的
            btnVibrate.Click += new EventHandler(order16_17);
            btnLoad.Click += new EventHandler(order23);
            btnGW.Click += new EventHandler(order24);
            btnFW.Click += new EventHandler(order25);

            this.Resize += new EventHandler(doWhenResize);

            //this.OnShown += new EventHandler(show);
            //this.FormClosed += new FormClosedEventHandler(doWhenClosing);

            //定义句柄
            h1 = new EventHandler(getResp1);

            if (Global.sp != null)
            {
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
            }

            //【定时，2秒一次】查询系统参数，判断温度情况，如果不一致，以电脑端为准让下位机给下位机重新设定
            
            timerTemp.Tick += new EventHandler(order1);

            passedTime = 0;
            timerTotalTime.Tick += new EventHandler(timingTotalTime);

            timerCurrZJ.Tick += new EventHandler(timingCurrZJTime);
  
            //timerTest1.Tick += new EventHandler(changeTestX);

        }

        private void FormManual_Load(object sender, EventArgs e)
        {
           
        }

        /// <summary>
        /// 为各输入框初始化内容
        /// </summary>
        public void init()
        {
            isVibrating = false;

            loadParam();

            player = new SoundPlayer();

       }

        /// <summary>
        /// 加载参数
        /// </summary>
        public void loadParam()
        {
            getConfig1();

            xw_time = new Hashtable();
            //加载穴位参数
            if (Global.client != null)
            {
                if (Global.client.param != null)
                {
                    try
                    {
                        loadParamClient();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("载入客户参数出错：\r\n" + e.Message);
                        loadParamDFT();
                    }
                }
                else
                {
                    MessageBox.Show("客户参数未填写");
                    loadParamDFT();
                }


            }
            else //如果还没有客户信息，就加载默认参数
            {
                loadParamDFT();
            }

            string[] tempSDs = config1.tempSD.Split(",".ToCharArray());
            lblTemp4.Text = handleTempForShow(Int32.Parse(tempSDs[0]));
            lblTemp2.Text = handleTempForShow(Int32.Parse(tempSDs[1]));
            lblTemp3.Text = handleTempForShow(Int32.Parse(tempSDs[2]));
            lblTemp1.Text = handleTempForShow(Int32.Parse(tempSDs[3]));

            F_B = 0; //默认为正面

            string[] frontX = config1.frontX.Split(",".ToCharArray());
            fx = new int[frontX.Length];
            for (int i = 0; i < frontX.Length; i++) { fx[i] = Int32.Parse(frontX[i]); }

            string[] backX = config1.backX.Split(",".ToCharArray());
            bx = new int[backX.Length];
            for (int i = 0; i < backX.Length; i++) { bx[i] = Int32.Parse(backX[i]); }

            btnF1.Text = frontZH[0]; btnB1.Text = backZH[0];
            btnF2.Text = frontZH[1]; btnB2.Text = backZH[1];
            btnF3.Text = frontZH[2]; btnB3.Text = backZH[2];
            btnF4.Text = frontZH[3]; btnB4.Text = backZH[3];
            btnF5.Text = frontZH[4]; btnB5.Text = backZH[4];
            btnF6.Text = frontZH[5]; btnB6.Text = backZH[5];
            btnF7.Text = frontZH[6]; btnB7.Text = backZH[6];
            btnF8.Text = frontZH[7]; btnB8.Text = backZH[7];
        }

         /// <summary>
        /// 加载默认参数（从config中取
        /// </summary>
        public void loadParamDFT()
        {
            frontX = config1.frontX.Split(",".ToCharArray());
            frontY = config1.frontY.Split(",".ToCharArray());
            frontTime = config1.frontTime.Split(",".ToCharArray());
            frontZH = config1.frontZH.Split(",".ToCharArray());
            backX = config1.backX.Split(",".ToCharArray());
            backY = config1.backY.Split(",".ToCharArray());
            backTime = config1.backTime.Split(",".ToCharArray());
            backZH = config1.backZH.Split(",".ToCharArray());

            //穴位（按钮）与其对应的时间进行关联
            xw_time["F1"] = frontTime[0]; xw_time["B1"] = backTime[0];
            xw_time["F2"] = frontTime[1]; xw_time["B2"] = backTime[1];
            xw_time["F3"] = frontTime[2]; xw_time["B3"] = backTime[2];
            xw_time["F4"] = frontTime[3]; xw_time["B4"] = backTime[3];
            xw_time["F5"] = frontTime[4]; xw_time["B5"] = backTime[4];
            xw_time["F6"] = frontTime[5]; xw_time["B6"] = backTime[5];
            xw_time["F7"] = frontTime[6]; xw_time["B7"] = backTime[6];
            xw_time["F8"] = frontTime[7]; xw_time["B8"] = backTime[7];

            lblParamSrc.Text = "参数来源：默认";
        }

        /// <summary>
        /// 加载默认参数（从config中取
        /// </summary>
        public void loadParamClient()
        {
            JObject param = JObject.Parse(Global.client.param);

            frontX = ((string)param["frontX"]).Split(",".ToCharArray());
            frontY = ((string)param["frontY"]).Split(",".ToCharArray());
            frontTime = ((string)param["frontTime"]).Split(",".ToCharArray());
            frontZH = ((string)param["frontZH"]).Split(",".ToCharArray());
            backX = ((string)param["backX"]).Split(",".ToCharArray());
            backY = ((string)param["backY"]).Split(",".ToCharArray());
            backTime = ((string)param["backTime"]).Split(",".ToCharArray());
            backZH = ((string)param["backZH"]).Split(",".ToCharArray());

            //穴位（按钮）与其对应的时间进行关联
            xw_time["F1"] = frontTime[0]; xw_time["B1"] = backTime[0];
            xw_time["F2"] = frontTime[1]; xw_time["B2"] = backTime[1];
            xw_time["F3"] = frontTime[2]; xw_time["B3"] = backTime[2];
            xw_time["F4"] = frontTime[3]; xw_time["B4"] = backTime[3];
            xw_time["F5"] = frontTime[4]; xw_time["B5"] = backTime[4];
            xw_time["F6"] = frontTime[5]; xw_time["B6"] = backTime[5];
            xw_time["F7"] = frontTime[6]; xw_time["B7"] = backTime[6];
            xw_time["F8"] = frontTime[7]; xw_time["B8"] = backTime[7];

            lblParamSrc.Text = "参数来源：" + Global.client.name;
        }

        /// <summary>
        /// 组织按钮
        /// </summary>
        public void makeTeams()
        {
            btnTeam1 = new Button[19];
            btnTeam1[0] = btnTurnOn;
            btnTeam1[1] = btnBedIn;
            btnTeam1[2] = btnBedOut;
            btnTeam1[3] = btnGateOpen;
            btnTeam1[4] = btnGateClose;
            btnTeam1[5] = btnCoverOpen;
            btnTeam1[6] = btnCoverClose;
            btnTeam1[7] = btnFan0;
            btnTeam1[8] = btnFan1;
            btnTeam1[9] = btnFan2;
            btnTeam1[10] = btnLeft;
            btnTeam1[11] = btnRight;
            btnTeam1[12] = btnUp;
            btnTeam1[13] = btnDown;

            //2018.2.28补充
            btnTeam1[14] = btnVibrate;
            btnTeam1[15] = btnFW;
            btnTeam1[16] = btnLoad;
            btnTeam1[17] = btnGW;

            btnTeam1[18] = btnTurnOff;
          
        }

         /// <summary>
        /// 添加事件
        /// </summary>
        public void addEvents()
        {
            //1. 为功能按钮添加变色事件
            foreach(Button btn in btnTeam1){
                btn.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
                btn.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);
            }
        }

        /// <summary>
        /// 打开针灸参数设置窗口
        /// </summary>
        public void openFormConfig(object sender, EventArgs e)
        {
            haveARest();

                if (Application.OpenForms["FormConfig"] == null)
                {
                    new FormConfig().Show();
                }
                else
                {
                    Form form = Application.OpenForms["FormConfig"];
                    form.Show();
                    ((FormConfig)form).showAgain();
                }

            //隐藏
               
  
        }

        /// <summary>
        /// 暂时隐藏本窗口
        /// </summary>
        private void haveARest()
        {
            isCommOver = true;
            isOrderOn = false;
            currOrder = 0;
            if (timerTemp != null)
            {
                timerTemp.Stop();
            }
           
            if (Global.sp != null && Global.sp.IsOpen)
            {
                Global.sp.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
                Global.sp.ReadExisting();
                Global.sp.Close();
                Global.sp.Open();
            }
            
            this.Hide();

            timerWelcome.Stop();
            Application.RemoveMessageFilter(this);
        }

        /// <summary>
        /// 打开系统参数窗口
        /// </summary>
        public void openFormCommParam(object sender, EventArgs e)
        {
            if (Application.OpenForms["FormCommParam"] == null)
            {
                new FormCommParam().Show();
            }
            else
            {
                Form form = Application.OpenForms["FormCommParam"];
                form.Activate();
                form.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        public void back(object sender, EventArgs e)
        {
            haveARest();
            openFormAuto();
        }

        /// <summary>
        /// 重新show的时候做点事情
        /// </summary>
        public void showAgain()
        {
            isCommOver = false;
            Console.WriteLine("手动界面：showAgain");

            if (Global.sp != null)
            {
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
            }

            if(timerTemp != null){
                timerTemp.Start();
            }

            timerWelcome.Start();
            Application.AddMessageFilter(this);

            //此处如果用的是默认参数，就刷新（可能在系统配置界面进行了修改，此处要更新）
            loadParam();

            this.Show();
        }

        /// <summary>
        /// 重新打开自动模式窗口
        /// </summary>
        public void openFormAuto()
        {
            Form form = Application.OpenForms["FormAuto"];
            form.Show();
            ((FormAuto)form).showAgain();
        }

       //发送部分,针对所有命令**************************************
        private void beginSend(byte[] order, int orderNo, int expLength)
        {
            //Console.WriteLine("命令" + currOrder + "  isOrderOn: " + isOrderOn);
          //while (! isOrderOn)
            if (!isOrderOn)
            {
                isOrderOn = true;
                currOrder = orderNo; currExpLength = expLength;
                byte[] orderReal = fnComm.sendOrder(order);

                if (currOrder == 1)
                {
                    currOrder1Real = orderReal;
                }

                log = "命令  " + currOrder + " 发送完成";
                this.Invoke(new EventHandler(showOrder));

                watcherStart();
            }
            else
            {
                if (currOrder == 1) { //Console.WriteLine("=> 放弃！"); 
                } 
            }
        }

        public void send(byte[] order, int orderNo, int expLength)
        {
            /*
            if (!isOrderOn)
            {
                new Thread(new ThreadStart(() => beginSend(order, orderNo, expLength))).Start();
            }
            else
            {
                log = "当前命令 " + currOrder + " 未完成，本次动作撤销";
                this.Invoke(new EventHandler(showOrder));
            }
            */
         
            isOrderOn = false;
            new Thread(new ThreadStart(() => beginSend(order, orderNo, expLength))).Start();
        }

        /// <summary>
        /// 发送指令
        /// </summary>
        public void order1(object sender, EventArgs e) { Console.WriteLine("定时查询"); flushPort(); send(ORDER.Z1, 1, ORDER.L1); }
        public void order2(object sender, EventArgs e) { send(ORDER.Z2, 2, ORDER.L2); }
        public void order3(object sender, EventArgs e) 
        {
           
            if (!isWaiting)
            { //如果已经计时结束（超过指定的秒数）
                //Console.WriteLine("超过秒数");
                send(ORDER.Z3, 3, ORDER.L3);
            }
            else
            {
                //Console.WriteLine("没到秒数");
            }
        }
        public void order4(object sender, EventArgs e) {
            send(ORDER.Z4, 4, ORDER.L4);
            isWaiting = true;
            System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(timerButtonCall), this, 2000, 0);
        }
        public void order5(object sender, EventArgs e) {
            send(ORDER.Z5, 5, ORDER.L5);
            isWaiting = true;
            System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(timerButtonCall), this, 1000, 0);
        }

        /// <summary>
        /// 床进/床停
        /// </summary>
        public void order3_4(object sender, EventArgs e)
        {
            Console.WriteLine("@isBedIning: " + isBedIning);
            if (!isBedIning)
            { send(ORDER.Z4, 4, ORDER.L4); isBedIning = true; isBedOuting = false; }
            else
            { send(ORDER.Z3, 3, ORDER.L3); isBedIning = false; }
        }

        /// <summary>
        /// 床出/床停
        /// </summary>
        public void order3_5(object sender, EventArgs e)
        {
            Console.WriteLine("@isBedOuting: " + isBedOuting);
            if (!isBedOuting)
            { send(ORDER.Z5, 5, ORDER.L5); isBedOuting = true; isBedIning = false; }
            else
            { send(ORDER.Z3, 3, ORDER.L3); isBedOuting = false; }
        }

        /// <summary>
        /// 舱门开/舱门停
        /// </summary>
        public void order6_7(object sender, EventArgs e)
        {
            Console.WriteLine("@isGateOpening: " + isGateOpening);
            if (!isGateOpening)
            { send(ORDER.Z7, 7, ORDER.L7); isGateOpening = true; isGateClosing = false; }
            else
            { send(ORDER.Z6, 6, ORDER.L6); isGateOpening = false; }
        }

        /// <summary>
        /// 舱门开/舱门停
        /// </summary>
        public void order6_8(object sender, EventArgs e)
        {
            Console.WriteLine("@isGateClosing: " + isGateClosing);
            if (!isGateClosing)
            { send(ORDER.Z8, 8, ORDER.L8); isGateClosing = true; isGateOpening = false; }
            else
            { send(ORDER.Z6, 6, ORDER.L6); isGateClosing = false; }
        }

        public void order6(object sender, EventArgs e) { send(ORDER.Z6, 6, ORDER.L6); }
        public void order7(object sender, EventArgs e) { send(ORDER.Z7, 7, ORDER.L7); }
        public void order8(object sender, EventArgs e) { send(ORDER.Z8, 8, ORDER.L8); }

        public void order9(object sender, EventArgs e) 
        {
            if (sender.Equals(btnFan0))
            {
                byte[] Z9_0 = ORDER.Z9; Z9_0[5] = 0x00; send(Z9_0, 9, ORDER.L9);
             }
             if (sender.Equals(btnFan1))
             {
                 byte[] Z9_1 = ORDER.Z9; Z9_1[5] = 0x01; send(Z9_1, 9, ORDER.L9);

                 btnFan2.BackColor = Color.FromArgb(42, 87, 154);
              }
              if (sender.Equals(btnFan2))
              {
                  byte[] Z9_2 = ORDER.Z9; Z9_2[5] = 0x02; send(Z9_2, 9, ORDER.L9);

                  btnFan1.BackColor = Color.FromArgb(42, 87, 154);
              }
              if (sender.Equals(btnFan3))
              {
                  byte[] Z9_3 = ORDER.Z9; Z9_3[5] = 0x03; send(Z9_3, 9, ORDER.L9);
              }
        }

        public void order10(object sender, EventArgs e) { send(ORDER.Z10, 10, ORDER.L10); }
        public void order11(object sender, EventArgs e) { send(ORDER.Z11, 11, ORDER.L11); }
        public void order12(object sender, EventArgs e) { send(ORDER.Z12, 12, ORDER.L12); }

        public void order13(object sender, EventArgs e) { send(ORDER.Z13, 13, ORDER.L13); }
        public void order14(object sender, EventArgs e) { send(ORDER.Z14, 14, ORDER.L14); }
        public void order15(object sender, EventArgs e) { send(ORDER.Z15, 15, ORDER.L15); }

        /// <summary>
        /// 发送指令：振动/停止振动
        /// </summary>
        public void order16_17(object sender, EventArgs e)
        {
            if (!isVibrating)
            { send(ORDER.Z17, 17, ORDER.L17); isVibrating = true; }
            else
            { send(ORDER.Z16, 16, ORDER.L16); isVibrating = false; }
        }

        /// <summary>
        /// 运行到指定穴位
        /// </summary>
        public void order18(object sender, int type, int rawIndex)
        {
            int index = rawIndex - 1;
            int x = 0, y = 0;
            switch (type)
            {
                case TYPE_F:
                    x = Int32.Parse(frontX[index]);
                    y = Int32.Parse(frontX[index]);
                    break;
                case TYPE_B:
                    x = Int32.Parse(backX[index]);
                    y = Int32.Parse(backY[index]);
                    break;
            }

            Console.WriteLine("坐标 x:" + x + "  Y:" + y);

            byte[] order = new byte[12];
            Console.WriteLine("Index: " + index);
            for (int i = 0; i < 6; i++)
            {
                order[i] = ORDER.Z18[i];
            }
            order[6] = Convert.ToByte(x / 256);
            order[7] = Convert.ToByte(x % 256); ;
            order[8] = Convert.ToByte(y / 256); ;
            order[9] = Convert.ToByte(y % 256); ;

            send(order, 18, ORDER.L18);

        }

        public void order19(object sender, EventArgs e) { 
            send(ORDER.Z19, 19, ORDER.L16); 

            btnFan1.BackColor = Color.FromArgb(42, 87, 154);
            btnFan2.BackColor = Color.FromArgb(42, 87, 154);
        }

        /// <summary>
        /// 装载
        /// </summary>
        public void order23(object sender, EventArgs e) { send(ORDER.Z23, 23, ORDER.L23); }

        /// <summary>
        /// 归位
        /// </summary>
        public void order24(object sender, EventArgs e) { send(ORDER.Z24, 24, ORDER.L24); }

        /// <summary>
        /// 复位
        /// </summary>
        public void order25(object sender, EventArgs e) { send(ORDER.Z25, 25, ORDER.L25); }


        /// <summary>
        /// 发送指令：舱盖开/舱盖停
        /// </summary>
        public void order26_28(object sender, EventArgs e)
        {
            Console.WriteLine("@isCoverOpening: " + isCoverOpening);
            if (!isCoverOpening)
            { send(ORDER.Z26, 26, ORDER.L26); isCoverOpening = true; isCoverClosing = false; }
            else
            { send(ORDER.Z28, 28, ORDER.L28); isCoverOpening = false; }
        }

        /// <summary>
        /// 发送指令：舱盖关/舱盖停
        /// </summary>
        public void order27_28(object sender, EventArgs e)
        {
            Console.WriteLine("@isCoverClosing: " + isCoverClosing);
            if (!isCoverClosing)
            { send(ORDER.Z27, 27, ORDER.L27); isCoverClosing = true; isCoverOpening = false; }
            else
            { send(ORDER.Z28, 28, ORDER.L28); isCoverClosing = false; }
        }

        /// <summary>
        /// 发送指令：加热开/关
        /// </summary>
        public void order29_30(object sender, EventArgs e)
        {
            Console.WriteLine("isHeating: " + isHeating);
            totalTime = config1.totalTime;
            tbxTotalTimeMin.Text = "0";
            tbxTotalTimeSec.Text = "0";

            if (!isHeating)
            { 
                send(ORDER.Z29, 29, ORDER.L29); isHeating = true; btnHeat.BackColor = Color.Green;

                totalTime = config1.totalTime;

                timerTotalTime.Enabled = true;
                timerTotalTime.Start();

                //timerCurrX.Enabled = true;
                //timerCurrX.Start();

                //timerTest的作用：生成模拟的水平步进电机位置数据，随着时间的推移不断增加
                //废止：2018.2.28 不用坐标，改用点击当前按钮了
                //timerTest1.Enabled = true;
                //timerTest1.Start();

            }
            else
            { 
                send(ORDER.Z30, 30, ORDER.L30); isHeating = false; btnHeat.BackColor = Color.Crimson;

                timerTotalTime.Enabled = false;
                timerTotalTime.Stop();

                //timerCurrX.Enabled = false;
                //timerCurrX.Stop();

                //废止：2018.2.28 不用坐标，改用点击当前按钮了
                //timerTest1.Enabled = false;
                //timerTest1.Stop();

                //reset
                if(currBtn != null){
                    currBtn.BackColor = Color.Sienna;
                }
                
                //播放结束提示音
                //播放铃声提示
                /*
                axPlayer.URL = System.Environment.CurrentDirectory + "\\hint_stop.mp3";
                axPlayer.Ctlcontrols.play();//播放文件
                 */

                SystemSounds.Beep.Play();
                SystemSounds.Beep.Play();

            }

            
        }

        /// <summary>
        /// 串口收到下位机返回的数据
        /// 特性：dataReceived方法只调用一次，但是在该方法调用的时候，bytesToRead不一定是完整的，可能要在若干个毫秒之后才完整
        /// </summary>
        public void dataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {

                //只要处理指令1，其他命令忽略
                if (currOrder != 1) { isOrderOn = false; flushPort(); return; }

                while (currOrder > 0 && Global.sp.BytesToRead < currExpLength)
                {
                    if (isCommOver) { return; }
                    if (!isOrderOn) { Console.WriteLine("------------------等待时 停止"); flushPort(); return; }; //isOrderOn如果为false，说明本次命令已超时，被停止了
                }
                //此处，虽然位数满了，但可能超时了，isOrderOn被置为false

                if (isCommOver) { return; }
                if (!isOrderOn) { Console.WriteLine("------------------数据满，开始去取前 停止"); flushPort(); return; };

                //数据位完整之后
                int[] resp = new int[currExpLength];
                for (int i = 0; i < currExpLength; i++)
                {
                    if (isCommOver) { return; }
                    if (!isOrderOn) { Console.WriteLine("------------------读取的过程中 停止"); flushPort(); return; };
                    resp[i] = (int)Global.sp.ReadByte();
                }


                if (isCommOver) { return; }
                if (!isOrderOn) { Console.WriteLine("------------------Global.resp赋值前 停止"); flushPort(); return; };

                //Console.WriteLine("命令" + currOrder + "  OK-数据读取完毕 ");

                Global.resp = resp;

                isOrderOn = false; //数据正常收到并处理完成之后的标记位该表


            }catch(Exception ex){
                lblLog.Text = ex.Message;
            }

            switch (currOrder)  //调用Invoke，其实就是在跨线程操作。。。
            {
                case 1: this.Invoke(h1); break;
                /*
                case 2: this.Invoke(h2); break;
                case 3: this.Invoke(h3); break;
                case 4: this.Invoke(h4); break;
                case 5: this.Invoke(h5); break;
                case 6: this.Invoke(h6); break;
                case 7: this.Invoke(h7); break;
                case 8: this.Invoke(h8); break;
                case 9: this.Invoke(h9); break;
                case 10: this.Invoke(h10); break;
                case 11: this.Invoke(h11); break;
                case 12: this.Invoke(h12); break;
                case 13: this.Invoke(h13); break;
                case 14: this.Invoke(h14); break;
                case 15: this.Invoke(h15); break;
                case 16: this.Invoke(h16); break;
                case 17: this.Invoke(h17); break;
                case 18: this.Invoke(h18); break;
                case 19: this.Invoke(h19); break;
                case 20: this.Invoke(h20); break;
                */
                /*
                case 1: this.Invoke(new EventHandler(getResp1)); break;
                case 2: this.Invoke(new EventHandler(getResp2)); break;
                case 3: this.Invoke(new EventHandler(getResp3)); break;
                case 4: this.Invoke(new EventHandler(getResp4)); break;
                case 5: this.Invoke(new EventHandler(getResp5)); break;
                case 6: this.Invoke(new EventHandler(getResp6)); break;
                case 7: this.Invoke(new EventHandler(getResp7)); break;
                case 8: this.Invoke(new EventHandler(getResp8)); break;
                case 9: this.Invoke(new EventHandler(getResp9)); break;
                case 10: this.Invoke(new EventHandler(getResp10)); break;
                case 11: this.Invoke(new EventHandler(getResp11)); break;
                case 12: this.Invoke(new EventHandler(getResp12)); break;
                case 13: this.Invoke(new EventHandler(getResp13)); break;
                case 14: this.Invoke(new EventHandler(getResp14)); break;
                case 15: this.Invoke(new EventHandler(getResp15)); break;
                case 16: this.Invoke(new EventHandler(getResp16)); break;
                case 17: this.Invoke(new EventHandler(getResp17)); break;
                case 18: this.Invoke(new EventHandler(getResp18)); break;
                case 19: this.Invoke(new EventHandler(getResp19)); break;
                case 20: this.Invoke(new EventHandler(getResp20)); break;
                */
            }
        }

        /// <summary>
        /// 关于按钮的计时
        /// </summary>
        private void timerButtonCall(object obj)
        {
            isWaiting = false;
        }

        /// <summary>
        /// 本次命令超时
        /// </summary>
        private void orderTimeoutCall(object obj)
        {
            //这里也要判断一下，如果是正常结束，就不能执行这个操作
            if(isOrderOn == true){
                isOrderOn = false; //超时之后的标记位该表
                flushPort();
            }
           
        }

        /// <summary>
        /// 启动线程，标记当前命令是否超时(200毫秒算超时）
        /// </summary>
        private void watcherStart()
        {
            System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback(orderTimeoutCall), this, 1000, 0);
        }

        /// <summary>
        /// 直接在界面上输出信息
        /// </summary>
        private void showLog(object sender, EventArgs e)
        {
            lblLog.Text = DateTime.Now.ToString("HH:mm:ss") + " " + log;
        }

        private void showOrder(object sender, EventArgs e)
        {
            lblOrder.Text = DateTime.Now.ToString("HH:mm:ss") + " " + log;
        }

        /// <summary>
        /// 处理串口返回值
        /// </summary>
        private void getResp1(object sender, EventArgs e)
        {
            try
            {

                int[] resp = new int[Global.resp.Length];

                try
                {
                    for (int i = 0; i < resp.Length; i++)
                    {
                        resp[i] = Global.resp[i];
                    }
                }
                catch (Exception ex)
                {
                    log = "冲突，指令1未处理";
                    this.Invoke(new EventHandler(showLog));
                    return;
                }

                //观察校验，如果不同就不显示
                string check = "";
                if (!fnComm.compareCheck(currOrder1Real, resp))
                {
                    check = "校验不同，未更新";
                    //showLog(null, null);
                    return;
                }
                else
                {
                    check = "校验相同";
                }

                //log显示
                log = "命令 " + currOrder + " 完成 " + check;
                //this.Invoke(new EventHandler(showLog));
                showLog(null, null);

                Console.WriteLine("【getResp1】长度：" + resp.Length);

                if (resp.Length < ORDER.L1)
                {
                    Console.WriteLine("resp长度不足");
                    return;
                }

                for (int i = 0; i < resp.Length; i++)
                {
                    Console.Write(resp[i] + " ");
                }
                Console.WriteLine("--");
                //showResp(resp);
                //比较温度
                //获取下位机温度信息

                int[] tempReal = new int[4];
                int[] tempIns = new int[4];

                tempReal[0] = resp[9] * 256 + resp[10];
                tempReal[1] = resp[11] * 256 + resp[12];
                tempReal[2] = resp[13] * 256 + resp[14];
                tempReal[3] = resp[15] * 256 + resp[16];

                tempIns[0] = resp[53] * 256 + resp[54];
                tempIns[1] = resp[55] * 256 + resp[56];
                tempIns[2] = resp[57] * 256 + resp[58];
                tempIns[3] = resp[59] * 256 + resp[60];

                //显示的形式要处理一下
                tbxTemp4.Text = handleTempForShow(tempReal[0]); lblTemp4.Text = handleTempForShow(tempReal[0]);
                tbxTemp2.Text = handleTempForShow(tempReal[1]); lblTemp2.Text = handleTempForShow(tempReal[1]);
                tbxTemp3.Text = handleTempForShow(tempReal[2]); lblTemp3.Text = handleTempForShow(tempReal[2]);
                tbxTemp1.Text = handleTempForShow(tempReal[3]); lblTemp1.Text = handleTempForShow(tempReal[3]);

                //获取电脑温度信息
                string[] strs = config1.tempSD.Split(",".ToCharArray());

                if (resp.Length < ORDER.L1)
                {
                    Console.WriteLine("resp长度不足");
                    lblResp.Text = DateTime.Now.ToString("HH:mm:ss") + "  " + resp.Length + "  resp长度不足";
                    return;
                }

                int[] tempPC = new int[4];
                tempPC[0] = Int32.Parse(strs[0]);
                tempPC[1] = Int32.Parse(strs[1]);
                tempPC[2] = Int32.Parse(strs[2]);
                tempPC[3] = Int32.Parse(strs[3]);

                bool same = true;
                for (int i = 0; i < tempPC.Length; i++)
                {
                    if (tempIns[i] != tempPC[i]) { same = false; break; }
                }
                if (!same)
                {
                    //order20
                    byte[] order = new byte[17];

                    for (int i = 0; i < 7; i++)
                    {
                        order[i] = ORDER.Z20[i];
                    }
                    order[7] = Convert.ToByte(tempPC[0] / 256);
                    order[8] = Convert.ToByte(tempPC[0] % 256); ;
                    order[9] = Convert.ToByte(tempPC[1] / 256); ;
                    order[10] = Convert.ToByte(tempPC[1] % 256);
                    order[11] = Convert.ToByte(tempPC[2] / 256);
                    order[12] = Convert.ToByte(tempPC[2] % 256);
                    order[13] = Convert.ToByte(tempPC[3] / 256);
                    order[14] = Convert.ToByte(tempPC[3] % 256);

                    //for test
                    //Console.WriteLine("比对，不同，发送：");
                    for (int i = 0; i < order.Length; i++)
                    {
                        //Console.Write(order[i] + " ");
                    }

                    send(order, 20, ORDER.L20);

                    lblResp.Text = DateTime.Now.ToString("HH:mm:ss") + "  DIFF";
                }
                else
                {
                    lblResp.Text = DateTime.Now.ToString("HH:mm:ss") + "  SAME";
                }

                //加热标志
                int heatFlag = resp[73] * 256 + resp[74];
                Console.WriteLine("heatFlag: " + heatFlag);
                switch (heatFlag)
                {
                    case 0: isHeating = false; btnHeat.BackColor = Color.Crimson; break; //停止
                    case 1: isHeating = true; btnHeat.BackColor = Color.Green; break; //开启
                }


                //判断一下水平步进电机位置
                //37
                int x = resp[37] * 256 + resp[38];
                //tbxCurrX.Text = x + "";
                //判断在在哪个穴位附件
                //for test
                x = testX; //使用模拟的
                tbxCurrZJTimeMin.Text = x + "";
                judgeX(x);

                //2018.3.1 判断设备是否具有使用许可
                int PERMIT_ALLOW = 0, PERMIT_FORBIDDEN = 1;
                int permit = resp[7] * 256 + resp[8];
                if (permit == PERMIT_FORBIDDEN)
                {
                    Console.WriteLine("请充值");
                    if (Application.OpenForms["FormHint"] == null)
                    {
                        FormHint formHint = new FormHint();

                        //怎样显示在标题栏的中间？

                        formHint.Show();
                        formHint.Top = this.Top;
                        formHint.Left = this.Left + this.Width / 2 - formHint.Width / 2;
                    }
                    else
                    {
                        Application.OpenForms["FormHint"].Show();
                    }


                }
                else
                {
                    Console.WriteLine("许可情况：" + permit);
                }

            }catch(Exception ex){
                lblLog.Text = ex.Message;
            }

        }

        /// <summary>
        /// 根据位置X判断当前穴位
        /// </summary>
        private void judgeX(int currX)
        {
           

            if(F_B == 0){ //正面穴位
                for(int i = 0; i < fx.Length; i++){
                    int x= fx[i];
                    //for test
                     //lblTest1.Text = F_B + "  testX: " + currX + "   pos: " + x;

                    if(currX > (x-50) && currX < (x+50)){ //如果满足这个条件，说明在当前穴位范围
                        //怎样为currBtn赋值
                        if(currBtn != null){
                            currBtn.BackColor = Color.Sienna;
                        }
                        
                        setCurrBtn(i);
                        return;
                    }

                    if (i >= (fx.Length - 1) && currX >= (x + 50))
                    {
                        F_B = 1;
                        testX = 0;
                        currBtn.BackColor = Color.Sienna;
                        currBtn = null;
                    }

                }
            }

            if (F_B == 1) //反面穴位
            {
                for (int i = 0; i < bx.Length; i++)
                {
                    int x = bx[i];

                    //for test
                    //lblTest1.Text = F_B + "  testX: " + currX + "   pos: " + x;

                    if (currX > (x - 50) && currX < (x + 50))
                    { //如果满足这个条件，说明在当前穴位范围
                        //怎样为currBtn赋值
                        if (currBtn != null)
                        {
                            currBtn.BackColor = Color.Sienna;
                        }
                        setCurrBtn(i);

                        return;
                    }

                    if (i == (bx.Length - 1) && currX >= (x + 50))
                    {
                        F_B = 1;
                        currBtn.BackColor = Color.Sienna;
                        currBtn = null;
                    }

                }
            }

            //
        }

        /// <summary>
        /// 确定当前穴位按钮
        /// </summary>
        private void setCurrBtn(int index){
            switch(F_B){
                case 0:
                    switch(index){
                         /*
                        case 0: currBtn = btnF1; lblCurrBtn.Text = "F1"; break;
                        case 1: currBtn = btnF2; lblCurrBtn.Text = "F2"; break;
                        case 2: currBtn = btnF3; lblCurrBtn.Text = "F3"; break;
                        case 3: currBtn = btnF4; lblCurrBtn.Text = "F4"; break;
                        case 4: currBtn = btnF5; lblCurrBtn.Text = "F5"; break;
                        case 5: currBtn = btnF6; lblCurrBtn.Text = "F6"; break;
                        case 6: currBtn = btnF7; lblCurrBtn.Text = "F7"; break;
                        case 7: currBtn = btnF8; lblCurrBtn.Text = "F8"; break;
                         */
                    }
                    break;
                case 1:
                    switch (index)
                    {
                        /*
                        case 0: currBtn = btnB1; lblCurrBtn.Text = "B1"; break;
                        case 1: currBtn = btnB2; lblCurrBtn.Text = "B2"; break;
                        case 2: currBtn = btnB3; lblCurrBtn.Text = "B3"; break;
                        case 3: currBtn = btnB4; lblCurrBtn.Text = "B4"; break;
                        case 4: currBtn = btnB5; lblCurrBtn.Text = "B5"; break;
                        case 5: currBtn = btnB6; lblCurrBtn.Text = "B6"; break;
                        case 6: currBtn = btnB7; lblCurrBtn.Text = "B7"; break;
                        case 7: currBtn = btnB8; lblCurrBtn.Text = "B8"; break;
                        */
                    }
                    break;
            }
        }

        /// <summary>
        /// 测试用，显示返回内容，可以通用化
        /// </summary>
        public void showResp(int[] resp)
        {
            string msg = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " RESP " + currOrder + " 返回：";
            for (int i = 0; i < resp.Length; i++)
            {
                string hex = Convert.ToString(resp[i], 16).ToUpper();
                msg += (hex.Length == 1 ? "0" + hex : hex) + " ";
            }
            ////Console.WriteLine(msg);
        }

        /// <summary>
        /// 获取配置1
        /// </summary>
        public void getConfig1()
        {
            string cfgStr = new FnFile().getConfig1();
            JObject obj = (JObject)JsonConvert.DeserializeObject(cfgStr);
            if (obj != null)
            {
                config1 = new Config1(obj);
            }
            else
            {
                config1 = new Config1();
            }

        }

        /// <summary>
        /// 修改时间
        /// </summary>
        public void edit(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Text.Equals("修改"))
            {
                //Console.WriteLine("修改...");
                timerTemp.Stop();
                timerTemp.Enabled = false;

                layoutShow.Visible = false;
                layoutEdit.Visible = true;

                btn.Text = "完成";
                //显示NTC设定值

                string[] tempSDs = config1.tempSD.Split(",".ToCharArray());
                tbxTemp4.Text = handleTempForShow(Int32.Parse(tempSDs[0]));
                tbxTemp2.Text = handleTempForShow(Int32.Parse(tempSDs[1]));
                tbxTemp3.Text = handleTempForShow(Int32.Parse(tempSDs[2]));
                tbxTemp1.Text = handleTempForShow(Int32.Parse(tempSDs[3]));

            }
            else
            {
                if (!checkFormat()) { return; }

                //Console.WriteLine("完成...");
                btn.Text = "修改";

                layoutShow.Visible = true;
                layoutEdit.Visible = false;

                //saveConfig1

                string tempSD = handleTempForSave(tbxTemp4.Text) + "," +
                                       handleTempForSave(tbxTemp2.Text) + "," +
                                       handleTempForSave(tbxTemp3.Text) + "," +
                                       handleTempForSave(tbxTemp1.Text);

                config1.tempSD = tempSD;
                

                string cfgStr = JsonConvert.SerializeObject(config1);

                new FnFile().toConfig1(cfgStr);

                //发送命令20
                byte[] order = new byte[17];
                for (int i = 0; i < 7; i++)
                {
                    order[i] = ORDER.Z20[i];
                }

                int temp1 = handleTempForSave(tbxTemp4.Text);
                int temp2 = handleTempForSave(tbxTemp2.Text);
                int temp3 = handleTempForSave(tbxTemp3.Text);
                int temp4 = handleTempForSave(tbxTemp1.Text);

                lblTemp4.Text = tbxTemp4.Text;
                lblTemp2.Text = tbxTemp2.Text;
                lblTemp3.Text = tbxTemp3.Text;
                lblTemp1.Text = tbxTemp1.Text;

                order[7] = Convert.ToByte(temp1 / 256);
                order[8] = Convert.ToByte(temp1 % 256);
                order[9] = Convert.ToByte(temp2 / 256); ;
                order[10] = Convert.ToByte(temp2 % 256);
                order[11] = Convert.ToByte(temp3 / 256);
                order[12] = Convert.ToByte(temp3 % 256);
                order[13] = Convert.ToByte(temp4 / 256);
                order[14] = Convert.ToByte(temp4 % 256);

                //for test
                //Console.WriteLine("下发温度指令：");
                for (int i = 0; i < order.Length; i++ )
                {
                    //Console.Write(order[i] + " ");
                }

                //Global.resp = new int[currExpLength];
                send(order, 20, ORDER.L20);

                timerTemp.Enabled = true;
                timerTemp.Start();
            }
            
            
        }

         /// <summary>
        /// 处理格式，显示小数点
        /// </summary>
        private string handleTempForShow (int x)  
        {
            string str = x.ToString();
            if(str.Length == 1){
                return str + ".0";
            }
            string part1 = str.Substring(0, str.Length - 1);
            string part2 = str.Substring(str.Length - 1);
            return part1 + "." + part2;

        }

        /// <summary>
        /// 处理格式，例如40.1 => 401 , 30 => 300
        /// </summary>
        private int handleTempForSave(string x)
        {
            double y = Double.Parse(x);
            string str = y.ToString("0.0");
            str = str.Replace(".", "");
            return Int32.Parse(str);
        }

        /// <summary>
        /// 检查各输入框的格式
        /// </summary>
        public bool checkFormat()
        {
            int[] ntcHigh = new int[4], ntcLow = new int[4];

            string[] tempsStr = config1.tempHighLimit.Split(",".ToCharArray());
            for(int i = 0; i < tempsStr.Length; i ++){
                ntcHigh[i] = Int32.Parse(tempsStr[i]);
            }

            tempsStr = config1.tempLowLimit.Split(",".ToCharArray());
            for(int i = 0; i < tempsStr.Length; i ++){
                ntcLow[i] = Int32.Parse(tempsStr[i]);
            }

            double y;
            //根据NTC温度上下限范围来判断
            if (!Double.TryParse(tbxTemp4.Text.Trim(), out y)) { MessageBox.Show("温度1 格式有误，请修正"); return false; }
            else { if (y < ntcLow[0] || y > ntcHigh[0]) { MessageBox.Show("温度1 超出范围，请修正"); return false; } }
            if (!Double.TryParse(tbxTemp2.Text.Trim(), out y)) { MessageBox.Show("温度2 格式有误，请修正"); return false; }
            else { if (y < ntcLow[1] || y > ntcHigh[1]) { MessageBox.Show("温度2 超出范围，请修正"); return false; } }
            if (!Double.TryParse(tbxTemp3.Text.Trim(), out y)) { MessageBox.Show("温度3 格式有误，请修正"); return false; }
            else { if (y < ntcLow[2] || y > ntcHigh[2]) { MessageBox.Show("温度3 超出范围，请修正"); return false; } }
            if (!Double.TryParse(tbxTemp1.Text.Trim(), out y)) { MessageBox.Show("温度4 格式有误，请修正"); return false; }
            else { if (y < ntcLow[3] || y > ntcHigh[3]) { MessageBox.Show("温度4 超出范围，请修正"); return false; } }

            return true;

        }

        /// <summary>
        /// 按钮按下变绿
        /// </summary>
        private void changeColorWhenBtnDown(object sender, EventArgs e)
        {
            Button btn = (Button)sender;

            if (btn == btnTurnOn)
            {  btn.BackColor = Color.Orange;  }
            else
            {  btn.BackColor = Color.Green; }

            
        }

        /// <summary>
        /// 按钮弹起变蓝
        /// </summary>
        private void changeColorWhenBtnUp(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            //延时5秒再变回原色
            System.Timers.Timer timer = new System.Timers.Timer();
            if(btn == btnLeft || btn == btnRight || btn == btnUp || btn == btnDown){
                 timer.Interval = 1;
            }else{
                timer.Interval = 5000;
            }
           
            if(btn != btnFan1 && btn != btnFan2){
                Color color = Color.FromArgb(42, 87, 154);
                timer.Elapsed += (sender1, e1) => changeBtnColor(sender1, e1, btn, color);
                timer.Start();
            }
 
        }

        /// <summary>
        /// 改变按钮背景颜色
        /// </summary>
        private void changeBtnColor(object sender, System.Timers.ElapsedEventArgs e, Button btn,  Color color)
        {
            if(btn == btnTurnOff){
                btn.BackColor = Color.Crimson;
            }else{
                btn.BackColor = Color.FromArgb(42, 87, 154);
            }
            
        }

        /// <summary>
        /// 针灸总时间计时(改为倒计时，而非时间累积)！！！
        /// </summary>
        private void timingTotalTime(object sender, EventArgs e)
        {
            totalTime--;
            tbxTotalTimeMin.Text = (totalTime / 60) + "";
            tbxTotalTimeSec.Text = (totalTime % 60) + "";

            if (totalTime == 0 )
            { //仅提示，不停止
                timerTotalTime.Stop();
                btnHeat.BackColor = Color.Crimson;
                isHeating = false;
                //播放铃声提示，两次
                player.SoundLocation = System.Environment.CurrentDirectory + "\\hint.wav";

                 new Thread(new ThreadStart(() => {
                     player.PlaySync();
                     player.PlaySync();
                 })).Start();

            }

        }

        /// <summary>
        /// 当前穴位的的倒计时
        /// </summary>
        private void timingCurrZJTime(object sender, EventArgs e)
        {
            currZJTime--;
            tbxCurrZJTimeMin.Text = (currZJTime / 60) + "";
            tbxCurrZJTimeSec.Text = (currZJTime % 60) + "";

            //闪灯
            Console.WriteLine("Lighten  CurrBtn ");
            if (currBtn == null) { Console.Write("  NULL "); return; }
            Console.Write("  Do ");
            if (currBtn.BackColor == Color.Sienna)
            {
                currBtn.BackColor = Color.Green;
            }
            else
            {
                currBtn.BackColor = Color.Sienna;
            }

            //如果完成了或者用户点击了其他穴位的按钮导致currZJTime被置0
            if (currZJTime == 0)
            { //仅提示，不停止
                timerCurrZJ.Stop();

                currBtn.Enabled = true;
                currBtn.BackColor = Color.Sienna;

                player.SoundLocation = System.Environment.CurrentDirectory + "\\hint_stop.wav";

                new Thread(new ThreadStart(() =>
                {
                    player.PlaySync();
                })).Start();

            }

        }

        /// <summary>
        /// 切换当前穴位按钮
        /// </summary>
        public void changeCurrBtn(Button btn)
        {
            timerCurrZJ.Stop();

            if(currBtn != null){
                currBtn.Enabled = true;
                currBtn.BackColor = Color.Sienna;
            }
            
            //先让所有穴位按钮都可用
            btnF1.Enabled = true; btnF2.Enabled = true; btnF3.Enabled = true; btnF4.Enabled = true; btnF5.Enabled = true; btnF6.Enabled = true; btnF7.Enabled = true; btnF8.Enabled = true;
            btnB1.Enabled = true; btnB2.Enabled = true; btnB3.Enabled = true; btnB4.Enabled = true; btnB5.Enabled = true; btnB6.Enabled = true; btnB7.Enabled = true; btnB8.Enabled = true;

            currBtn = btn;
            currBtn.Enabled = false;
            currZJTime = 1; //默认设置为1秒

            //获取当前穴位的时间
            if(btn == btnF1){ string timeStr = (string)xw_time["F1"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF2){ string timeStr = (string)xw_time["F2"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF3){ string timeStr = (string)xw_time["F3"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF4){ string timeStr = (string)xw_time["F4"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF5){ string timeStr = (string)xw_time["F5"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF6){ string timeStr = (string)xw_time["F6"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF7){ string timeStr = (string)xw_time["F7"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnF8){ string timeStr = (string)xw_time["F8"];  Int32.TryParse(timeStr, out currZJTime); }

            if(btn == btnB1){ string timeStr = (string)xw_time["B1"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB2){ string timeStr = (string)xw_time["B2"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB3){ string timeStr = (string)xw_time["B3"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB4){ string timeStr = (string)xw_time["B4"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB5){ string timeStr = (string)xw_time["B5"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB6){ string timeStr = (string)xw_time["B6"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB7){ string timeStr = (string)xw_time["B7"];  Int32.TryParse(timeStr, out currZJTime); }
            if(btn == btnB8){ string timeStr = (string)xw_time["B8"];  Int32.TryParse(timeStr, out currZJTime); }

            timerCurrZJ.Start();
        }

        /// <summary>
        /// 观察，30秒无操作就弹出待机界面
        /// </summary>
        public void watch(object sender, EventArgs e)
        {
            if (Environment.TickCount - lastInput > 10 * 1000)
            {
                //timer.Stop();
                /*
                lastInput = Environment.TickCount;
                Console.WriteLine("5s 未操作 , 打开欢迎界面----------------");
                haveARest();
                openFormWelcome();
                 */
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
                new FormWelcome(FormWelcome.FORM_MANUAL).Show();
            }
            else
            {
                Console.WriteLine("exist welcome");
                Form form = Application.OpenForms["FormWelcome"];
                form.Show();
                ((FormWelcome)form).source = FormWelcome.FORM_MANUAL;
                ((FormWelcome)form).showAgain();
            }
        }

        public bool PreFilterMessage(ref Message msg)
        {
            const int WM_LBUTTONDOWN = 0x201; //
            const int WM_KEYDOWN = 0x100;
            switch (msg.Msg)
            {
                case WM_LBUTTONDOWN:
                case WM_KEYDOWN:
                case 522:
                case 512:
                    lastInput = Environment.TickCount;

                    //Console.WriteLine("手动界面 事件：" + msg.Msg);
                    break;
            }
            return false;
        }

        /// <summary>
        /// 最小化时的处理
        /// </summary>
        public void doWhenResize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (Application.OpenForms["FormHint"] != null)
                {
                    Application.OpenForms["FormHint"].Hide();
                }
            }
        }

        /// <summary>
        /// 窗口关闭时的处理
        /// </summary>
        public void doWhenClosing(object sender, EventArgs e)
        {
            timerTemp.Stop();
            timerTemp.Enabled = false;
            if(Global.sp != null && Global.sp.IsOpen){
                 Global.sp.ReadExisting();
                 Global.sp.Close();
                 Global.sp.Open();
            }
            currOrder = 0;

            timerWelcome.Stop();
            Application.RemoveMessageFilter(this);
            //h5 = null;

        }

        /// <summary>
        /// 清空串口数据
        /// </summary>
        private void flushPort()
        {
            if (Global.sp != null && Global.sp.IsOpen)
            {
                Global.sp.ReadExisting();
            }
        }


        private void setFormSize()
        {
            this.Width = 1280;
            this.Height = 1024;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            axPlayer.URL = System.Environment.CurrentDirectory + "\\hint.mp3";
            axPlayer.Ctlcontrols.play();//播放文件
        }

    }
}
