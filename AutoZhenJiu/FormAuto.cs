using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoZhenJiu.entity;
using AutoZhenJiu.fn;

namespace AutoZhenJiu
{

    public partial class FormAuto : Form, IMessageFilter
    {
        const int TYPE_F = 1, TYPE_B = 0;

        bool isOrderOn; //是否有命令在运行
        bool isCommOver; //交互是否停止

        Config1 config1;
        CommParam param;

        Order ORDER;
        FnComm fnComm;
        int currOrder, currExpLength;
        static int totalZJTime; //针灸总时间

        bool isVibrating; //是否正在振动

        static int currZJIndex; //当前针灸位置（0~15）

        public string[] frontX, backX,
                   frontY, backY,
                   frontTime, backTime,
                   frontZH, backZH;

        static List<MissionPoint> mps = new List<MissionPoint>();
        static List<MissionPoint> mpsFixed = new List<MissionPoint>(); //固定的、不受倒计时影响的
        //为了绕过static的问题
        static TextBox tbxTotalTimeMin2, tbxTotalTimeSec2, tbxCurrTimeMin2, tbxCurrTimeSec2;
        static ComboBox cbxScheme2;
        static Button btnStart2, btnMission2;
        static Button btnF1s, btnF2s, btnF3s, btnF4s, btnF5s, btnF6s, btnF7s, btnF8s, btnB1s, btnB2s, btnB3s, btnB4s, btnB5s, btnB6s, btnB7s, btnB8s;

        string log; //输出在界面上的信息

        public ToolTip toolTip;
        int lastInput;
        static System.Windows.Forms.Timer timer;

        bool wmFlag = true;

        //指令1每次的实际指令
        byte[] currOrder1Real;

        public FormAuto()
        {
            InitializeComponent();
            setFormSize();

            //for test
            Application.AddMessageFilter(this);
            lastInput = Environment.TickCount;
            timerWelcome.Tick += new EventHandler(watch);
            //timerWelcome.Start();


            getConfig1();
            getCommParam();

            init();
            makeTeams();

            ORDER = new Order().init();
            fnComm = new FnComm();

            isVibrating = false;

            btnStart.Click += new EventHandler(start);
            btnManualMode.Click += new EventHandler(openFormManual);
            btnSD.Click += new EventHandler(openFormManual);

            //for test
            btnMRTest.Click += new EventHandler(openFormMR);

            btnMR.Click += new EventHandler(openFormMR);
            btnTurnOff.Click += new EventHandler(order19);

            btnVibrate.Click += new EventHandler(order16_17);
            btnLoad.Click += new EventHandler(order23);
            btnGW.Click += new EventHandler(order24);
            btnFW.Click += new EventHandler(order25);

            //按下变绿色
            btnStart.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
            btnStart.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);
            btnVibrate.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
            btnVibrate.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);
            btnLoad.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
            btnLoad.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);
            btnGW.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
            btnGW.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);
            btnFW.MouseDown += new MouseEventHandler(changeColorWhenBtnDown);
            btnFW.MouseUp += new MouseEventHandler(changeColorWhenBtnUp);

            btnMission.Click += new EventHandler(openFormMission);

            btnF1.Click += delegate(Object o, EventArgs e) { order18(btnF1, TYPE_F, 1); };
            btnF2.Click += delegate(Object o, EventArgs e) { order18(btnF2, TYPE_F, 2); };
            btnF3.Click += delegate(Object o, EventArgs e) { order18(btnF3, TYPE_F, 3); };
            btnF4.Click += delegate(Object o, EventArgs e) { order18(btnF4, TYPE_F, 4); };
            btnF5.Click += delegate(Object o, EventArgs e) { order18(btnF5, TYPE_F, 5); };
            btnF6.Click += delegate(Object o, EventArgs e) { order18(btnF6, TYPE_F, 6); };
            btnF7.Click += delegate(Object o, EventArgs e) { order18(btnF7, TYPE_F, 7); };
            btnF8.Click += delegate(Object o, EventArgs e) { order18(btnF8, TYPE_F, 8); };

            btnB1.Click += delegate(Object o, EventArgs e) { order18(btnB1, TYPE_B, 1); };
            btnB2.Click += delegate(Object o, EventArgs e) { order18(btnB2, TYPE_B, 2); };
            btnB3.Click += delegate(Object o, EventArgs e) { order18(btnB3, TYPE_B, 3); };
            btnB4.Click += delegate(Object o, EventArgs e) { order18(btnB4, TYPE_B, 4); };
            btnB5.Click += delegate(Object o, EventArgs e) { order18(btnB5, TYPE_B, 5); };
            btnB6.Click += delegate(Object o, EventArgs e) { order18(btnB6, TYPE_B, 6); };
            btnB7.Click += delegate(Object o, EventArgs e) { order18(btnB7, TYPE_B, 7); };
            btnB8.Click += delegate(Object o, EventArgs e) { order18(btnB8, TYPE_B, 8); };

            cbxScheme.SelectedIndexChanged += new EventHandler(refreshConfig);

            this.EnabledChanged += new EventHandler(doWhenEnabledChanged);
            this.FormClosing += new FormClosingEventHandler(doWhenClosing);

            this.Resize += new EventHandler(doWhenResize);

            openSerialPort();

            if(Global.sp != null){
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
            }

            cbxScheme.SelectedIndex = Scheme.S1;
            refreshConfig(cbxScheme, null);

            toolTip = new ToolTip();
            //toolTip.SetToolTip(lblMission, "");
            //【定时，5秒一次】查询系统参数，判断温度情况，如果不一致，以电脑端为准让下位机给下位机重新设定
            timerTemp.Tick += new EventHandler(order1);
        }

        private void FormAuto_Load(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 按钮文字赋值
        /// </summary>
        public void init()
        {
            loadParam();
        }

        /// <summary>
        /// 组织控件，后面用
        /// </summary>
        private void makeTeams()
        {
           
            tbxTotalTimeMin2 = tbxTotalTimeMin;
            tbxTotalTimeSec2 = tbxTotalTimeSec;
            tbxCurrTimeMin2 = tbxCurrTimeMin;
            tbxCurrTimeSec2 = tbxCurrTimeSec;

            cbxScheme2 = cbxScheme;
            btnStart2 = btnStart;
            btnMission2 = btnMission;

            btnF1s = btnF1; btnB1s = btnB1;
            btnF2s = btnF2; btnB2s = btnB2;
            btnF3s = btnF3; btnB3s = btnB3;
            btnF4s = btnF4; btnB4s = btnB4;
            btnF5s = btnF5; btnB5s = btnB5;
            btnF6s = btnF6; btnB6s = btnB6;
            btnF7s = btnF7; btnB7s = btnB7;
            btnF8s = btnF8; btnB8s = btnB8;
        }

        /// <summary>
        /// 加载参数
        /// </summary>
        public void loadParam()
        {

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

            lblParamSrc.Text = "参数来源：" + Global.client.name;
        }


        /// <summary>
        /// 更新任务(进行一个转换）
        /// </summary>
        public void updatePoints(List<MissionPoint> newMps)
        {
            mps = newMps;
            
            foreach(MissionPoint mp in mps){
                if (mp.no.Equals("F1")) { mp.btn = btnF1s; } if (mp.no.Equals("B1")) { mp.btn = btnB1s; }
                if (mp.no.Equals("F2")) { mp.btn = btnF2s; } if (mp.no.Equals("B2")) { mp.btn = btnB2s; }
                if (mp.no.Equals("F3")) { mp.btn = btnF3s; } if (mp.no.Equals("B3")) { mp.btn = btnB3s; }
                if (mp.no.Equals("F4")) { mp.btn = btnF4s; } if (mp.no.Equals("B4")) { mp.btn = btnB4s; }
                if (mp.no.Equals("F5")) { mp.btn = btnF5s; } if (mp.no.Equals("B5")) { mp.btn = btnB5s; }
                if (mp.no.Equals("F6")) { mp.btn = btnF6s; } if (mp.no.Equals("B6")) { mp.btn = btnB6s; }
                if (mp.no.Equals("F7")) { mp.btn = btnF7s; } if (mp.no.Equals("B7")) { mp.btn = btnB7s; }
                if (mp.no.Equals("F8")) { mp.btn = btnF8s; } if (mp.no.Equals("B8")) { mp.btn = btnB8s; }
            }

        }

        /// <summary>
        /// 启动（自动操作）
        /// </summary>
        private void start(object sender, EventArgs e)
        {
            //检查任务是否设置
            if(mps.Count == 0){
                MessageBox.Show("请设定任务");
                return;
            }

            resetTotalTime();

            Console.WriteLine("start: currZJIndex " + currZJIndex);
            //转向第一个穴位
            toLocation();
            //振动
            if (mps[0].isVibrate) { order16_17(null, null); Console.WriteLine("【开始 振动】: currZJIndex " + currZJIndex  );  }

            timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler(animateCurrPoint);
            timer.Interval = 1000;
            timer.Start();
            cbxScheme.Enabled = false; //当前方案开始之后，就不能换了
            btnStart.Enabled = false;
            btnMission.Enabled = false;

            btnVibrate.Enabled = false;

            //按钮颜色恢复
            foreach (MissionPoint mp in mps){ mp.btn.BackColor = Color.Sienna;}
           
        }

        /// <summary>
        /// 用动画标识当前穴位（按钮）
        /// </summary>
        private void animateCurrPoint(Object sender, EventArgs e){
            
            MissionPoint mp = mps[currZJIndex];
            Button btn = mp.btn;

            if (btn.BackColor == Color.Red)
            {
                btn.BackColor = Color.Green;
            }
            else
            {
                btn.BackColor = Color.Red;
            }

            //处理显示的问题
            tbxTotalTimeMin2.Text = totalZJTime/60 + "";
            tbxTotalTimeSec2.Text = totalZJTime%60 + "";
            tbxCurrTimeMin2.Text = mp.time / 60 + "";
            tbxCurrTimeSec2.Text = mp.time % 60 + "";

            //计时操作
            totalZJTime--;
            mp.time--;

            if (mp.time == 0)
            {
                btn.BackColor = Color.Black;

                //发送一下振动（停），停止
                if (mp.isVibrate) { order16_17(null, null); Console.WriteLine("【结束 振动】: currZJIndex " + currZJIndex); }
                send(ORDER.Z19, 19, ORDER.L19);


                currZJIndex++;
               

                if (currZJIndex < mps.Count)
                {
                    //转向下一个穴位
                    toLocation();
                    //振动
                    MissionPoint mpNext = mps[currZJIndex];
                    if (mpNext.isVibrate) { order16_17(null, null); Console.WriteLine("【开始 振动】: currZJIndex " + currZJIndex); }
                }
                
            }

            if(totalZJTime == 0){
                timer.Stop();
                currZJIndex = 0;
                btnStart2.Enabled = true;
                btnMission2.Enabled = true;
                cbxScheme2.Enabled = true;

                tbxTotalTimeMin2.Text = null;
                tbxTotalTimeSec2.Text = null;
                tbxCurrTimeMin2.Text = null;
                tbxCurrTimeSec2.Text = null;

            }
            
        }

        /// <summary>
        /// 发送位移命令
        /// </summary>
        private void toLocation()
        {
            //order18(btnF5 ?, TYPE_F ?, 5 ?);
            //根据currZJIndex 获取当前穴位的坐标
            int[] fx = new int[8], fy = new int[8], bx = new int[8], by = new int[8];

            string[]
            strs = config1.frontX.Split(",".ToCharArray());
            for (int i = 0; i < strs.Length; i++) { fx[i] = Int32.Parse(strs[i]); }
            strs = config1.frontY.Split(",".ToCharArray());
            for (int i = 0; i < strs.Length; i++) { fy[i] = Int32.Parse(strs[i]); }

            strs = config1.backX.Split(",".ToCharArray());
            for (int i = 0; i < strs.Length; i++) { bx[i] = Int32.Parse(strs[i]); }
            strs = config1.backY.Split(",".ToCharArray());
            for (int i = 0; i < strs.Length; i++) { by[i] = Int32.Parse(strs[i]); }

            int x = 0, y = 0;
            MissionPoint mp = mps[currZJIndex];
            if(mp.no.Equals("F1")){ x = fx[0]; y = fy[0]; } if(mp.no.Equals("B1")){ x = bx[0]; y = by[0]; }
            if(mp.no.Equals("F2")){ x = fx[1]; y = fy[1]; } if(mp.no.Equals("B2")){ x = bx[1]; y = by[1]; }
            if(mp.no.Equals("F3")){ x = fx[2]; y = fy[2]; } if(mp.no.Equals("B3")){ x = bx[2]; y = by[2]; }
            if(mp.no.Equals("F4")){ x = fx[3]; y = fy[3]; } if(mp.no.Equals("B4")){ x = bx[3]; y = by[3]; }
            if(mp.no.Equals("F5")){ x = fx[4]; y = fy[4]; } if(mp.no.Equals("B5")){ x = bx[4]; y = by[4]; }
            if(mp.no.Equals("F6")){ x = fx[5]; y = fy[5]; } if(mp.no.Equals("B6")){ x = bx[5]; y = by[5]; }
            if(mp.no.Equals("F7")){ x = fx[6]; y = fy[6]; } if(mp.no.Equals("B7")){ x = bx[6]; y = by[6]; }
            if(mp.no.Equals("F8")){ x = fx[7]; y = fy[7]; } if(mp.no.Equals("B8")){ x = bx[7]; y = by[7]; }

            order18Direct(x, y);

        }

        /// <summary>
        /// 切换方案时刷新config1
        /// </summary>
        private void refreshConfig(Object sender, EventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            new FnConfig().refreshConfig1(config1, cbx.SelectedIndex);
            resetTotalTime();
        }

        /// <summary>
        /// 重置针灸总时间
        /// </summary>
        private void resetTotalTime()
        {
            currZJIndex = 0;

            totalZJTime = 0;
            /*
            string[]
            timesStr = config1.frontTime.Split(",".ToCharArray());
            for (int i = 0; i < timesStr.Length; i++) { mps[i].time = Int32.Parse(timesStr[i]); totalZJTime += mps[i].time; }
            timesStr = config1.backTime.Split(",".ToCharArray());
            for (int i = 0; i < timesStr.Length; i++) { mps[8 + i].time = Int32.Parse(timesStr[i]); totalZJTime += mps[8 + i].time; }
            */

            //mps = mpsFixed;
            Console.WriteLine("mps重置");
           
            foreach (MissionPoint mp in mps)
            {
                mp.time = mp.timeFixed;
            }

            foreach (MissionPoint mp in mps) { totalZJTime += mp.time; }

        }

        //发送部分**************************************
        private void beginSend(byte[] order, int orderNo, int expLength)
        {
            Console.WriteLine("命令" + currOrder + "  isOrderOn: " + isOrderOn);
            while (!isOrderOn)
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

        public void order1(object sender, EventArgs e) { Console.WriteLine("定时：order1"); send(ORDER.Z1, 1, ORDER.L1); }
        /// <summary>
        /// 发送指令：振动/停止振动
        /// </summary>
        public void order16_17(object sender, EventArgs e) {
            if (! isVibrating)
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
            string[] strs = new string[8];
            int x = 0, y = 0;
            switch (type)
            {
                case TYPE_F:
                    strs = config1.frontX.Split(",".ToCharArray());
                    x = Int32.Parse(strs[index]);
                    strs = config1.frontY.Split(",".ToCharArray());
                    y = Int32.Parse(strs[index]);
                    break;
                case TYPE_B:
                    strs = config1.backX.Split(",".ToCharArray());
                    x = Int32.Parse(strs[index]);
                    strs = config1.backY.Split(",".ToCharArray());
                    y = Int32.Parse(strs[index]);
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

        /// <summary>
        /// 运行到指定穴位 简单
        /// </summary>
        public void order18Direct(int x, int y)
        {
            byte[] order = new byte[12];
           
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

        /// <summary>
        /// 停止
        /// </summary>
        public void order19(object sender, EventArgs e){ 
            send(ORDER.Z19, 19, ORDER.L19);
            if (timer != null) { timer.Stop(); }
            //按钮颜色恢复
            btnStart.Enabled = true;
            btnMission.Enabled = true;
            btnVibrate.Enabled = true;
            foreach (MissionPoint mp in mps) { mp.btn.BackColor = Color.Sienna; }

            //倒计时重新开始

           
        }

        /// <summary>
        /// 装载
        /// </summary>
        public void order23(object sender, EventArgs e){ send(ORDER.Z23, 23, ORDER.L23); }

        /// <summary>
        /// 归位
        /// </summary>
        public void order24(object sender, EventArgs e){ send(ORDER.Z24, 24, ORDER.L24); }

        /// <summary>
        /// 复位
        /// </summary>
        public void order25(object sender, EventArgs e){ send(ORDER.Z25, 25, ORDER.L25); }

        /// <summary>
        /// 串口收到下位机返回的数据
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



                switch (currOrder)
                {
                    case 1: this.Invoke(new EventHandler(getResp1)); break;
                    /*
                case 16: this.Invoke(new EventHandler(getResp16)); break;
                case 17: this.Invoke(new EventHandler(getResp17)); break;
                case 18: this.Invoke(new EventHandler(getResp18)); break;
                case 19: this.Invoke(new EventHandler(getResp19)); break;
                case 23: this.Invoke(new EventHandler(getResp23)); break;
                case 24: this.Invoke(new EventHandler(getResp24)); break;
                case 25: this.Invoke(new EventHandler(getResp25)); break;
                */
                }

            }catch(Exception ex){
                lblLog.Text = ex.Message;
            }

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
        /// 处理串口返回值，自动界面只要显示，不需要修改
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

                showResp(resp);
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
                tbxTemp1.Text = handleTempForShow(tempReal[0]); lblTemp4.Text = handleTempForShow(tempReal[0]);
                tbxTemp2.Text = handleTempForShow(tempReal[1]); lblTemp2.Text = handleTempForShow(tempReal[1]);
                tbxTemp3.Text = handleTempForShow(tempReal[2]); lblTemp3.Text = handleTempForShow(tempReal[2]);
                tbxTemp4.Text = handleTempForShow(tempReal[3]); lblTemp1.Text = handleTempForShow(tempReal[3]);

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

                lblResp.Text = DateTime.Now.ToString("HH:mm:ss") + "  TEMP";

            }catch(Exception ex){
                lblLog.Text = ex.Message;
            }

        }

      

        /// <summary>
        /// 本次命令超时
        /// </summary>
        private void orderTimeoutCall(object obj)
        {
            //这里也要判断一下，如果是正常结束，就不能执行这个操作
            if (isOrderOn == true)
            {
                //Global.sp.Close(); Global.sp.Open();
                isOrderOn = false; //超时之后的标记位该表
                flushPort();
                Console.WriteLine("命令" + currOrder + "超时---");
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
        /// 测试用，显示返回内容，可以通用化
        /// </summary>
        public void showResp(int[] resp)
        {
            string msg = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 返回：";
            for (int i = 0; i < resp.Length; i++)
            {
                string hex = Convert.ToString(resp[i], 16).ToUpper();
                msg += (hex.Length == 1 ? "0" + hex : hex) + " ";
            }
            Console.Write(msg);
        }

        /// <summary>
        /// 打开手动模式窗口
        /// </summary>
        public void openFormMission(object sender, EventArgs e)
        {
            if (timerWelcome != null)
            {
                timerWelcome.Stop();
            }
            

            if (Application.OpenForms["FormMission"] == null)
            {
                Form form = new FormMission(this);
                form.Show();
            }
            else
            {
                Form form = Application.OpenForms["FormMission"];
                form.Show();
            }


        }

        /// <summary>
        /// 打开手动模式窗口
        /// </summary>
        public void openFormManual(object sender, EventArgs e)
        {
            haveARest();
            //把Timer停了
            if(timer != null){ timer.Stop(); }
            //按钮颜色恢复
            btnStart.Enabled = true;
            btnMission.Enabled = true;
            btnVibrate.Enabled = true;
            foreach (MissionPoint mp in mps) { mp.btn.BackColor = Color.Sienna; }
   
            if(Global.sp != null && Global.sp.IsOpen){
                Global.sp.Close();
                Global.sp.Open();
            }

            if (Application.OpenForms["FormManual"] == null)
            {
                new FormManual().Show();
            }
            else
            {
                Form form = Application.OpenForms["FormManual"];
                form.Show();
                ((FormManual)form).showAgain();
            }
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
            //为了控制isCommOver的取值
            this.Enabled = false;

            wmFlag = false;
            timerWelcome.Stop();
            Application.RemoveMessageFilter(this);

        }

        /// <summary>
        /// 重新show的时候做点事情
        /// </summary>
        public void showAgain()
        {
            isCommOver = false;
            Console.WriteLine("自动界面：showAgain");
            this.Enabled = true;
            if (Global.sp != null)
            {
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
            }

            if (timerTemp != null)
            {
                timerTemp.Start();
            }

            wmFlag = true;
            timerWelcome.Start();
            Application.AddMessageFilter(this);

            getConfig1();
            init();

            this.Show();
        }

        /// <summary>
        /// 打开病历填写窗口
        /// </summary>
        public void openFormMR(object sender, EventArgs e)
        {
            if (Application.OpenForms["FormMR"] == null)
            {
                Form form = new FormMR(this);
                form.Show();
            }
            else
            {
                Form form = Application.OpenForms["FormMR"];
                form.Activate();
                form.WindowState = FormWindowState.Normal;
            }
        }

        

        /// <summary>
        /// 获取配置1
        /// </summary>
        public void getConfig1()
        {
            string cfgStr = new  FnFile().getConfig1();
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
        /// 获取系统参数
        /// </summary>
        public void getCommParam()
        {
            string paramStr = new FnFile().getCommParam();
           
            JObject obj = (JObject)JsonConvert.DeserializeObject(paramStr);
            if (obj != null)
            {
                param = new CommParam(obj);
            }
            else
            {
                param = new CommParam();
                param.port = "COM1";
                param.baudrate = "9600";
            }
            
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void openSerialPort()
        {
            if (Global.sp == null)
            {
                Global.sp = new System.IO.Ports.SerialPort();
                Global.sp.PortName = param.port;
                Global.sp.BaudRate = Convert.ToInt32(param.baudrate);
            }
            if(!Global.sp.IsOpen){
                try
                {
                    Global.sp.Open();
                    Console.WriteLine("串口已打开");
                }
                catch (Exception ex)
                {
                    Global.sp = null;
                    MessageBox.Show("串口无法打开，请检查串口设置\r\n" + ex.Message);
                    return;
                }
            }
            
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        public void closeSerialPort()
        {
           if(Global.sp!=null && Global.sp.IsOpen){
               Global.sp.Close();
           }
        }

        /// <summary>
        /// 窗口show(）时的处理
        /// </summary>
        public void doWhenEnabledChanged(object sender, EventArgs e)
        {
            if(this.Enabled){
                isCommOver = false;
                openSerialPort();
            }
            
        }

        /// <summary>
        /// 观察，30秒无操作就弹出待机界面
        /// </summary>
        public void watch(object sender, EventArgs e)
        {
            //Console.WriteLine("TimerWelcome Tick----------------");
            if (Environment.TickCount - lastInput > 10 * 1000)
            {
                /*
                lastInput = Environment.TickCount;
                Console.WriteLine("5s 未操作 , 打开欢迎界面----------------");
                openFormWelcome();
                haveARest();
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
                new FormWelcome(FormWelcome.FORM_AUTO).Show();
            }
            else
            {
                Console.WriteLine("exist welcome");
                Form form = Application.OpenForms["FormWelcome"];
                form.Show();
                ((FormWelcome)form).source = FormWelcome.FORM_AUTO;
                ((FormWelcome)form).showAgain();

            }
        }

        public bool PreFilterMessage(ref Message msg)
        {
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
                    if(wmFlag){
                         lastInput = Environment.TickCount;
                        Console.WriteLine("自动界面 事件：" + msg.Msg);
                    }
                   
                    break;
            }
            return false;
        }

        /// <summary>
        /// 处理格式，显示小数点
        /// </summary>
        private string handleTempForShow(int x)
        {
            string str = x.ToString();
            if (str.Length == 1)
            {
                return str + ".0";
            }
            string part1 = str.Substring(0, str.Length - 1);
            string part2 = str.Substring(str.Length - 1);
            return part1 + "." + part2;

        }

        /// <summary>
        /// 按钮按下变绿
        /// </summary>
        private void changeColorWhenBtnDown(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.Green;
        }

        /// <summary>
        /// 按钮弹起变蓝
        /// </summary>
        private void changeColorWhenBtnUp(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            btn.BackColor = Color.FromArgb(42,87,154);
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
            if (Application.OpenForms["FormHint"] != null)
            {
                Application.OpenForms["FormHint"].Hide();
            }


            isCommOver = true;
            closeSerialPort();
            timerWelcome.Stop();
            Application.RemoveMessageFilter(this);
            Application.Exit();
        }

       

        /// <summary>
        /// 清空串口数据
        /// </summary>
        private void flushPort()
        {
            if(Global.sp != null && Global.sp.IsOpen){
                Global.sp.ReadExisting();
            }
            
        }

        private void setFormSize()
        {
            this.Width = 1280;
            this.Height = 1024;
        }
    }
}
