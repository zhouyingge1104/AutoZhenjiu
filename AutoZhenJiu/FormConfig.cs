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
    public partial class FormConfig : Form
    {
        Config1 config1;
        Config2 config2;
        Scheme scheme;

        internal bool
            config2Access, //是否有权限修改config2（是否登录）
            perssionAccess; //是否有权限进入激活界面（是否登录）

        bool isOrderOn; //是否有命令在运行
        bool isCommOver; //交互是否停止

        Order ORDER;
        FnComm fnComm;
        int currOrder, currExpLength;

        TextBox[] fxs, fys, fTimes, fZHs, bxs, bys, bTimes, bZHs, highs, lows, tempXZs;

        int applyTarget; //申请进入的目标界面  1：设备参数   2：用户许可
        const int TARGET_CONFIG2 = 1,
                     TARGET_PERMISSION = 2;

        //充值相关
        const int LW_UNLIMITED = 0,
                    LW_DAYS = 1,
                    LW_TIMES = 2,
                    LW_TU_OK = 3,
                    LW_TU_FAIL = 4,
                    LW_RESET_OK = 5;

        public const int SOURCE_1 = 1, //config1
                     SOURCE_2 = 2; //permission

        public FormConfig()
        {
            InitializeComponent();

            ORDER = new Order().init();
            fnComm = new FnComm();

            makeTeams();
            getConfig1();
            getConfig2();
            getScheme();
            init();

            //new FnConfig().refreshConfig1(config1, Scheme.S1);

            btnGetConfig2.Click += new EventHandler(order21);
            btnUpdatePwd.Click += new EventHandler(updatePwd);
            btnUpdatePwd2.Click += new EventHandler(updatePwd2);
            btnSave1.Click += new EventHandler(save1);
            btnClose1.Click += new EventHandler(closeForm);

            btnSave2.Click += new EventHandler(save2);
            btnClose2.Click += new EventHandler(closeForm);

            btnClose3.Click += new EventHandler(closeForm);

            cbxScheme.SelectedIndexChanged += new EventHandler(refreshConfig);

            tabCtrl.SelectedIndexChanged += new EventHandler(changeTab);

            btnQueryTUInfo.Click += new EventHandler(order31);
            btnReset.Click += new EventHandler(order32);
            btnTopUp.Click += new EventHandler(order33);

            if (Global.sp != null)
            {
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
            }

        }

        private void FormConfig_Load(object sender, EventArgs e)
        {
        
        }

        //发送部分**************************************
        private void beginSend(byte[] order, int orderNo, int expLength)
        {
            Console.WriteLine("命令" + currOrder + "  isOrderOn: " + isOrderOn);
            while (!isOrderOn)
            {
                isOrderOn = true;
                currOrder = orderNo; currExpLength = expLength;
                fnComm.sendOrder(order);
                watcherStart();
            }
        }

        public void send(byte[] order, int orderNo, int expLength)
        {
            new Thread(new ThreadStart(() => beginSend(order, orderNo, expLength))).Start();
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public void getConfig1()
        {
            string cfgStr = new FnFile().getConfig1();
            if (cfgStr.Trim().Length != 0)
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(cfgStr);
                config1 = new Config1(obj);
            }
            else
            {
                config1 = new Config1();
            }
            
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        public void getConfig2()
        {
            string cfgStr = new FnFile().getConfig2();
            if (cfgStr.Trim().Length != 0)
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(cfgStr);
                config2 = new Config2(obj);
            }
            else
            {
                config2 = new Config2();
            }

        }

        /// <summary>
        /// 获取穴位方案
        /// </summary>
        public void getScheme()
        {
            string schemeStr = new FnFile().getScheme();
            if (schemeStr.Trim().Length != 0)
            {
                JObject obj = (JObject)JsonConvert.DeserializeObject(schemeStr);
                scheme = new Scheme(obj);
            }
            else
            {
                scheme = new Scheme();
            }

        }

        /// <summary>
        /// 为各输入框初始化内容
        /// </summary>
        public void init()
        {
            config2Access = false;
            perssionAccess = false;
            cbxScheme.SelectedIndex = Scheme.S1;
            refreshConfig(cbxScheme, null);

            showScheme();

            string[] tempHighLimit = config1.tempHighLimit.Split(",".ToCharArray());
            string[] tempLowLimit = config1.tempLowLimit.Split(",".ToCharArray());
            string[] tempXZ = config2.tempXZ.Split(",".ToCharArray());



            for (int i = 0; i < highs.Length; i++) { highs[i].Text = tempHighLimit[i]; Console.WriteLine("初始化:" + tempHighLimit[i]); }
            for (int i = 0; i < lows.Length; i++) { lows[i].Text = tempLowLimit[i]; }
            for (int i = 0; i < tempXZs.Length; i++) { tempXZs[i].Text = handleTempForShow(Int32.Parse(tempXZ[i])); }

            tbxTempHC.Text = handleTempForShow(config2.tempHC);
            tbxTotalTime.Text = config1.totalTime + "";
            cbxTurnOnAfterLaunch.Checked = config1.autoTurnOn;

            //下位机参数...
            tbxBodyTemp.Text = handleTempForShow(config2.bodyTemp);
            tbxDistance.Text = config2.distance + "";
            tbxDelay.Text = config2.delay + "";
            initFanSpeed(config2.fanSpeed);
            tbxHorDGLength.Text = config2.horDGLength + "";
            tbxVerDGLength.Text = config2.verDGLength + "";
            tbxHorSpeedQ.Text = config2.horSpeedQ + "";
            tbxHorSpeedT.Text = config2.horSpeedT + "";
            tbxVerSpeedQ.Text = config2.verSpeedQ + "";
            tbxVerSpeedT.Text = config2.verSpeedT + "";
            tbxVibrateRange.Text = config2.vibrateRange + "";
            tbxVibrateSpeedQ.Text = config2.vibrateSpeedQ + "";
            tbxVibrateSpeedT.Text = config2.vibrateSpeedT + "";
            tbxVibrateDelay.Text = config2.vibrateDelay + "";
            tbxLoadSpeedQ.Text = config2.loadSpeedQ + "";
            tbxLoadSpeedT.Text = config2.loadSpeedT + "";
            tbxCtrlCode.Text = config2.ctrlCode + "";
            tbxGateDelay.Text = config2.gateDelay + "";
            cbxAutoUpload.Checked = config2.autoUpload; 

        }

        /// <summary>
        /// 填充穴位参数
        /// </summary>
        private void showScheme()
        {
            string[] frontX = config1.frontX.Split(",".ToCharArray());
            string[] frontY = config1.frontY.Split(",".ToCharArray());
            string[] frontTime = config1.frontTime.Split(",".ToCharArray());
            string[] frontZH = config1.frontZH.Split(",".ToCharArray());
            string[] backX = config1.backX.Split(",".ToCharArray());
            string[] backY = config1.backY.Split(",".ToCharArray());
            string[] backTime = config1.backTime.Split(",".ToCharArray());
            string[] backZH = config1.backZH.Split(",".ToCharArray());
           
            for (int i = 0; i < fxs.Length; i++) { fxs[i].Text = frontX[i]; }
            for (int i = 0; i < fys.Length; i++) { fys[i].Text = frontY[i]; }
            for (int i = 0; i < fTimes.Length; i++) { fTimes[i].Text = frontTime[i]; }
            for (int i = 0; i < fZHs.Length; i++) { Console.WriteLine(frontZH[i]); fZHs[i].Text = frontZH[i]; }
            for (int i = 0; i < bxs.Length; i++) { bxs[i].Text = backX[i]; }
            for (int i = 0; i < bys.Length; i++) { bys[i].Text = backY[i]; }
            for (int i = 0; i < bTimes.Length; i++) { bTimes[i].Text = backTime[i]; }
            for (int i = 0; i < bZHs.Length; i++) { Console.WriteLine(backZH[i]); bZHs[i].Text = backZH[i]; }
        }

        /// <summary>
        /// 将类似的控件进行分组
        /// </summary>
        public void makeTeams()
        {
            fxs = new TextBox[8]; fys = new TextBox[8]; fTimes = new TextBox[8]; fZHs = new TextBox[8];
            bxs = new TextBox[8]; bys = new TextBox[8]; bTimes = new TextBox[8]; bZHs = new TextBox[8];
            fxs[0] = tbxFX1; fys[0] = tbxFY1; fTimes[0] = tbxFTime1; fZHs[0] = tbxFZH1; bxs[0] = tbxBX1; bys[0] = tbxBY1; bTimes[0] = tbxBTime1; bZHs[0] = tbxBZH1;  
            fxs[1] = tbxFX2; fys[1] = tbxFY2; fTimes[1] = tbxFTime2; fZHs[1] = tbxFZH2; bxs[1] = tbxBX2; bys[1] = tbxBY2; bTimes[1] = tbxBTime2; bZHs[1] = tbxBZH2;
            fxs[2] = tbxFX3; fys[2] = tbxFY3; fTimes[2] = tbxFTime3; fZHs[2] = tbxFZH3; bxs[2] = tbxBX3; bys[2] = tbxBY3; bTimes[2] = tbxBTime3; bZHs[2] = tbxBZH3;
            fxs[3] = tbxFX4; fys[3] = tbxFY4; fTimes[3] = tbxFTime4; fZHs[3] = tbxFZH4; bxs[3] = tbxBX4; bys[3] = tbxBY4; bTimes[3] = tbxBTime4; bZHs[3] = tbxBZH4;
            fxs[4] = tbxFX5; fys[4] = tbxFY5; fTimes[4] = tbxFTime5; fZHs[4] = tbxFZH5; bxs[4] = tbxBX5; bys[4] = tbxBY5; bTimes[4] = tbxBTime5; bZHs[4] = tbxBZH5;
            fxs[5] = tbxFX6; fys[5] = tbxFY6; fTimes[5] = tbxFTime6; fZHs[5] = tbxFZH6; bxs[5] = tbxBX6; bys[5] = tbxBY6; bTimes[5] = tbxBTime6; bZHs[5] = tbxBZH6;
            fxs[6] = tbxFX7; fys[6] = tbxFY7; fTimes[6] = tbxFTime7; fZHs[6] = tbxFZH7; bxs[6] = tbxBX7; bys[6] = tbxBY7; bTimes[6] = tbxBTime7; bZHs[6] = tbxBZH7;
            fxs[7] = tbxFX8; fys[7] = tbxFY8; fTimes[7] = tbxFTime8; fZHs[7] = tbxFZH8; bxs[7] = tbxBX8; bys[7] = tbxBY8; bTimes[7] = tbxBTime8; bZHs[7] = tbxBZH8;

            highs = new TextBox[4];
            lows = new TextBox[4];
            tempXZs = new TextBox[4];
            highs[0] = tbxTempH4; lows[0] = tbxTempL4; tempXZs[0] = tbxTempXZ1;
            highs[1] = tbxTempH2; lows[1] = tbxTempL2; tempXZs[1] = tbxTempXZ2;
            highs[2] = tbxTempH3; lows[2] = tbxTempL3; tempXZs[2] = tbxTempXZ3;
            highs[3] = tbxTempH1; lows[3] = tbxTempL1; tempXZs[3] = tbxTempXZ4;

        }

        /// <summary>
        /// 切换方案时刷新config1（界面）
        /// </summary>
        private void refreshConfig(Object sender, EventArgs e)
        {
            ComboBox cbx = (ComboBox)sender;
            new FnConfig().refreshConfig1(config1, cbx.SelectedIndex);
            showScheme();
        }

        /// <summary>
        /// 保存Scheme之前先将其更新
        /// </summary>
        private void refreshScheme()
        {
            int sNo = cbxScheme.SelectedIndex;
            new FnConfig().refreshScheme(scheme, config1, sNo);
        }

        /// <summary>
        /// 检查各输入框的格式1
        /// </summary>
        public bool checkFormat1()
        {
            int x;

            foreach (TextBox tbx in fxs)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 正面X 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in fys)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 正面Y 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in fTimes)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 正面时间 格式有误，请修正"); return false; }
                //要大于零
                else if (x < 0) { MessageBox.Show("参数 正面时间 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in fZHs)
            {
                if (tbx.Text.Trim().Contains(",")) { MessageBox.Show("参数 正面穴位名称 含有不支持的字符，请修正"); return false; }
            }

            foreach (TextBox tbx in bxs)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 背面X 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in bys)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 背面Y 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in bTimes)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 背面时间 格式有误，请修正"); return false; }
                //要大于零
                else if (Int32.Parse(tbx.Text.Trim()) < 0) { MessageBox.Show("参数 背面时间 格式有误，请修正"); return false; }
            }

            foreach (TextBox tbx in bZHs)
            {
                if (tbx.Text.Trim().Contains(",")) { MessageBox.Show("参数 背面穴位名称 含有不支持的字符，请修正"); return false; }
            }

            foreach (TextBox tbx in highs)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 温度上限 格式有误，请修正"); return false; }
                else { 
                    if (x < 30 || x > 100) { MessageBox.Show("参数 温度上限 格式有误，请修正"); return false; }
                }
            }

            foreach (TextBox tbx in lows)
            {
                if (!Int32.TryParse(tbx.Text.Trim(), out x)) { MessageBox.Show("参数 温度下限 格式有误，请修正"); return false; }
                else { 
                    if (x < 30 || x > 100) { MessageBox.Show("参数 温度下限 格式有误，请修正"); return false; } 
                }
            }

            if (!Int32.TryParse(tbxTotalTime.Text.Trim(), out x)) { MessageBox.Show("参数 针灸总时间 格式有误，请修正"); return false; }

            return true;

        }

          /// <summary>
        /// 检查各输入框的格式2
        /// </summary>
        public bool checkFormat2()
        {
            int x;
            double y;

            foreach (TextBox tbx in tempXZs)
            {
                if (!Double.TryParse(tbx.Text.Trim(), out y)) { MessageBox.Show("参数 温度修正 格式有误，请修正"); return false; }
                else { if (y < -5 || y > 5) { MessageBox.Show("参数 温度修正 范围有误，请修正"); return false; } }
            }

            if (!Double.TryParse(tbxTempHC.Text.Trim(), out y)) { MessageBox.Show("参数 温度回差 格式有误，请修正"); return false; }
            else { if (y < 0 || y > 10) { MessageBox.Show("参数 温度回差 格式有误，请修正"); return false; } }

            if (!Double.TryParse(tbxBodyTemp.Text.Trim(), out y)) { MessageBox.Show("参数 体表温度上限 格式有误，请修正"); return false; }
            else { if (y < 10 || y > 50) { MessageBox.Show("参数 体表温度上限 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxDistance.Text.Trim(), out x)) { MessageBox.Show("参数 体表保持距离 格式有误，请修正"); return false; }
            else { if (x < 10 || x > 500) { MessageBox.Show("参数 体表保持距离 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxDelay.Text.Trim(), out x)) { MessageBox.Show("参数 点火延时 格式有误，请修正"); return false; }
            else { if (x < 1 || x > 200) { MessageBox.Show("参数 点火延时 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxHorDGLength.Text.Trim(), out x)) { MessageBox.Show("参数 水平导轨长度 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 65535) { MessageBox.Show("参数 水平导轨长度 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVerDGLength.Text.Trim(), out x)) { MessageBox.Show("参数 垂直导轨长度 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 65535) { MessageBox.Show("参数 垂直导轨长度 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxHorSpeedQ.Text.Trim(), out x)) { MessageBox.Show("参数 水平启停速度-启 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 水平启停速度-启 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxHorSpeedT.Text.Trim(), out x)) { MessageBox.Show("参数 水平启停速度-停 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 水平启停速度-停 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVerSpeedQ.Text.Trim(), out x)) { MessageBox.Show("参数 垂直启停速度-启 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 垂直启停速度-启 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVerSpeedT.Text.Trim(), out x)) { MessageBox.Show("参数 垂直启停速度-停 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 垂直启停速度-停 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVibrateRange.Text.Trim(), out x)) { MessageBox.Show("参数 振动幅度 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 65535) { MessageBox.Show("参数 振动幅度 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVibrateSpeedQ.Text.Trim(), out x)) { MessageBox.Show("参数 振动启停速度-启 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 振动启停速度-启 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxVibrateSpeedT.Text.Trim(), out x)) { MessageBox.Show("参数 振动启停速度-停 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 振动启停速度-停 范围有误，请修正"); return false; } }

            if (!Int32.TryParse(tbxVibrateDelay.Text.Trim(), out x)) { MessageBox.Show("参数 振动延时 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 65535) { MessageBox.Show("参数 振动延时 范围有误，请修正"); return false; } }

            if (!Int32.TryParse(tbxLoadSpeedQ.Text.Trim(), out x)) { MessageBox.Show("参数 装载启停速度-启 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 装载启停速度-启 范围有误，请修正"); return false; } }
            if (!Int32.TryParse(tbxLoadSpeedT.Text.Trim(), out x)) { MessageBox.Show("参数 装载启停速度-停 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 255) { MessageBox.Show("参数 装载启停速度-停 范围有误，请修正"); return false; } }

            if (!Int32.TryParse(tbxCtrlCode.Text.Trim(), out x)) { MessageBox.Show("参数 遥控器编码 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 65535) { MessageBox.Show("参数 遥控器编码 范围有误，请修正"); return false; } }

            if (!Int32.TryParse(tbxGateDelay.Text.Trim(), out x)) { MessageBox.Show("参数 舱门开启延时 格式有误，请修正"); return false; }
            else { if (x < 0 || x > 500) { MessageBox.Show("参数 舱门开启延时 范围有误，请修正"); return false; } }

            return true;
        }

        /// <summary>
        /// 保存PC参数
        /// </summary>
        public void save1(object sender, EventArgs e)
        {
            if (!checkFormat1()) { return; }
            saveConfig1();
            saveScheme();
            MessageBox.Show("保存成功");
        }

        /// <summary>
        /// 保存下位机参数
        /// </summary>
        public void save2(object sender, EventArgs e)
        {
            if (!checkFormat2()) { return; }
            saveConfig2();
            order22();
            MessageBox.Show("保存成功");
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void saveConfig1()
        {
            //给config赋值
            string frontX = fxs[0].Text + "," +
                                fxs[1].Text + "," +
                                fxs[2].Text + "," +
                                fxs[3].Text + "," +
                                fxs[4].Text + "," +
                                fxs[5].Text + "," +
                                fxs[6].Text + "," +
                                fxs[7].Text;

            string frontY = fys[0].Text + "," +
                                fys[1].Text + "," +
                                fys[2].Text + "," +
                                fys[3].Text + "," +
                                fys[4].Text + "," +
                                fys[5].Text + "," +
                                fys[6].Text + "," +
                                fys[7].Text;

            string frontTime = fTimes[0].Text + "," +
                                    fTimes[1].Text + "," +
                                    fTimes[2].Text + "," +
                                    fTimes[3].Text + "," +
                                    fTimes[4].Text + "," +
                                    fTimes[5].Text + "," +
                                    fTimes[6].Text + "," +
                                    fTimes[7].Text;

            string frontZH = fZHs[0].Text + "," +
                                   fZHs[1].Text + "," +
                                   fZHs[2].Text + "," +
                                   fZHs[3].Text + "," +
                                   fZHs[4].Text + "," +
                                   fZHs[5].Text + "," +
                                   fZHs[6].Text + "," +
                                   fZHs[7].Text;

            string backX = bxs[0].Text + "," +
                                bxs[1].Text + "," +
                                bxs[2].Text + "," +
                                bxs[3].Text + "," +
                                bxs[4].Text + "," +
                                bxs[5].Text + "," +
                                bxs[6].Text + "," +
                                bxs[7].Text;

            string backY = bys[0].Text + "," +
                                bys[1].Text + "," +
                                bys[2].Text + "," +
                                bys[3].Text + "," +
                                bys[4].Text + "," +
                                bys[5].Text + "," +
                                bys[6].Text + "," +
                                bys[7].Text;

            string backTime = bTimes[0].Text + "," +
                                    bTimes[1].Text + "," +
                                    bTimes[2].Text + "," +
                                    bTimes[3].Text + "," +
                                    bTimes[4].Text + "," +
                                    bTimes[5].Text + "," +
                                    bTimes[6].Text + "," +
                                    bTimes[7].Text;

            string backZH = bZHs[0].Text + "," +
                                   bZHs[1].Text + "," +
                                   bZHs[2].Text + "," +
                                   bZHs[3].Text + "," +
                                   bZHs[4].Text + "," +
                                   bZHs[5].Text + "," +
                                   bZHs[6].Text + "," +
                                   bZHs[7].Text;

            string tempHighLimit = highs[0].Text + "," +
                                          highs[1].Text + "," +
                                          highs[2].Text + "," +
                                          highs[3].Text;

            string tempLowLimit = lows[0].Text + "," +
                                          lows[1].Text + "," +
                                          lows[2].Text + "," +
                                          lows[3].Text;

            string totalTime = tbxTotalTime.Text;
          
            //autoTurnOn
            bool autoTurnOn = cbxTurnOnAfterLaunch.Checked;

            Console.WriteLine("保存：frontZH: " + frontZH);
            Console.WriteLine("保存：backZH: " + backZH);

            config1.frontX = frontX;
            config1.frontY = frontY;
            config1.frontTime = frontTime;
            config1.frontZH = frontZH;

            config1.backX = backX;
            config1.backY = backY;
            config1.backTime = backTime;
            config1.backZH = backZH;

            config1.tempHighLimit = tempHighLimit;
            config1.tempLowLimit = tempLowLimit;

            Console.WriteLine("上限：" + tempHighLimit);
          
            config1.totalTime = Int32.Parse(totalTime);
            config1.autoTurnOn = autoTurnOn;

            string cfgStr = JsonConvert.SerializeObject(config1);

            new FnFile().toConfig1(cfgStr);

        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void saveConfig2()
        {
            string tempXZ = handleTempForSave(tempXZs[0].Text) + "," +
                                 handleTempForSave(tempXZs[1].Text) + "," +
                                 handleTempForSave(tempXZs[2].Text) + "," +
                                 handleTempForSave(tempXZs[3].Text);
            string tempHC = handleTempForSave(tbxTempHC.Text) + "";
            string bodyTemp = tbxBodyTemp.Text;
            string distance = tbxDistance.Text;
            string delay = tbxDelay.Text;
            // string fanSpeed = tbx.Text;
            string horDGLength = tbxHorDGLength.Text;
            string verDGLength = tbxVerDGLength.Text;
            string horSpeedQ = tbxHorSpeedQ.Text;
            string horSpeedT = tbxHorSpeedT.Text;
            string verSpeedQ = tbxVerSpeedQ.Text;
            string verSpeedT = tbxVerSpeedT.Text;
            string vibrateRange = tbxVibrateRange.Text;
            string vibrateSpeedQ = tbxVibrateSpeedQ.Text;
            string vibrateSpeedT = tbxVibrateSpeedT.Text;
            string vibrateDelay = tbxVibrateDelay.Text;
            string loadSpeedQ = tbxLoadSpeedQ.Text;
            string loadSpeedT = tbxLoadSpeedT.Text;
            string ctrlCode = tbxCtrlCode.Text;
            string gateDelay = tbxGateDelay.Text;

            bool autoUpload = cbxAutoUpload.Checked;

            //温度的特殊处理
            config2.tempXZ = tempXZ;
            config2.tempHC = Int32.Parse(tempHC);
            config2.bodyTemp = handleTempForSave(bodyTemp);
            config2.distance = Int32.Parse(distance);
            config2.delay = Int32.Parse(delay);
            config2.fanSpeed = getFanSpeed();
            config2.horDGLength = Int32.Parse(horDGLength);
            config2.verDGLength = Int32.Parse(verDGLength);
            config2.horSpeedQ = Int32.Parse(horSpeedQ);
            config2.horSpeedT = Int32.Parse(horSpeedT);
            config2.verSpeedQ = Int32.Parse(verSpeedQ);
            config2.verSpeedT = Int32.Parse(verSpeedT);
            config2.vibrateRange = Int32.Parse(vibrateRange);
            config2.vibrateSpeedQ = Int32.Parse(vibrateSpeedQ);
            config2.vibrateSpeedT = Int32.Parse(vibrateSpeedT);
            config2.vibrateDelay = Int32.Parse(vibrateDelay);
            config2.loadSpeedQ = Int32.Parse(loadSpeedQ);
            config2.loadSpeedT = Int32.Parse(loadSpeedT);
            config2.ctrlCode = Int32.Parse(ctrlCode);
            config2.gateDelay = Int32.Parse(gateDelay);

            config2.autoUpload = autoUpload;

            string cfgStr = JsonConvert.SerializeObject(config2);

            new FnFile().toConfig2(cfgStr);

        }

        /// <summary>
        /// 保存穴位方案
        /// </summary>
        public void saveScheme()
        {
            refreshScheme();
            string schemeStr = JsonConvert.SerializeObject(scheme);
            new FnFile().toScheme(schemeStr);
        }

        /// <summary>
        /// 得到风扇速度
        /// </summary>
        public int getFanSpeed()
        {
            if (rbFan0.Checked) { return 0; }
            if (rbFan1.Checked) { return 1; }
            if (rbFan2.Checked) { return 2; }
            if (rbFan3.Checked) { return 3; }

            return 0;
        }

        /// <summary>
        /// 初始化风扇速度
        /// </summary>
        public void initFanSpeed(int fanSpeed)
        {
            switch (fanSpeed)
            {
                case 0: rbFan0.Checked = true; break;
                case 1: rbFan1.Checked = true; break;
                case 2: rbFan2.Checked = true; break;
                case 3: rbFan3.Checked = true; break;
            }
        }

         /// <summary>
        /// 从下位机获取设置参数
        /// </summary>
        public void order21(object sender, EventArgs e)
        {
            send(ORDER.Z21, 21, ORDER.L21);
        }

        /// <summary>
        /// 下发设置参数
        /// </summary>
        public void order22()
        {
            getConfig1();
            getConfig2();

        
            //处理一下ORDER.Z22 , 第7位开始
            byte[] order = new byte[57];
            for (int i = 0; i < 7; i++)
            {
                order[i] = ORDER.Z22[i];
            }

            //1. 组织设置参数
            int[] paramSet = new int[24];
            string[] high = config1.tempHighLimit.Split(",".ToCharArray()),
                        low = config1.tempLowLimit.Split(",".ToCharArray());      
            int h1 = Int32.Parse(high[0]), h2 = Int32.Parse(high[1]), h3 = Int32.Parse(high[2]), h4 = Int32.Parse(high[3]),
                 l1 = Int32.Parse(low[0]),   l2 = Int32.Parse(low[1]),   l3 = Int32.Parse(low[2]),   l4 = Int32.Parse(low[3]);

            string[] tempXZ = config2.tempXZ.Split(",".ToCharArray());
            int t1 = Int32.Parse(tempXZ[0]), t2 = Int32.Parse(tempXZ[1]), t3 = Int32.Parse(tempXZ[2]), t4 = Int32.Parse(tempXZ[3]);

            
            paramSet[0] = h1*256 + l1;
            paramSet[1] = h2*256 + l2;
            paramSet[2] = h3*256 + l3;
            paramSet[3] = h4*256 + l4;

            paramSet[4] = config2.tempHC;

            paramSet[5] = config2.bodyTemp;
            paramSet[6] = config2.distance;
            paramSet[7] = config2.delay;
            paramSet[8] = config2.fanSpeed;
            paramSet[9] =  config2.horDGLength;
            paramSet[10] = config2.verDGLength;
            paramSet[11] = config2.horSpeedQ * 256 + config2.horSpeedT;
            paramSet[12] = config2.verSpeedQ * 256 + config2.verSpeedT;
            paramSet[13] = config2.vibrateRange;
            paramSet[14] = config2.vibrateSpeedQ * 256 + config2.vibrateSpeedT;;
            paramSet[15] = config2.autoUpload?1:0;
            //!温度修正，可能有负数，需要强转
            paramSet[16] = t1;
            paramSet[17] = t2;
            paramSet[18] = t3;
            paramSet[19] = t4;

            paramSet[20] = config2.vibrateDelay;
            paramSet[21] = config2.loadSpeedQ * 256 + config2.loadSpeedT;;
            paramSet[22] = config2.ctrlCode;
            paramSet[23] = config2.gateDelay;

            int start = 7;
            for (int i = 0; i < paramSet.Length; i ++ )
            {
                if(i == 16 || i ==17 || i == 18 || i ==19){
                    if (paramSet[i] < 0)
                    {
                        paramSet[i] = (0xff & paramSet[i]);
                    }
                }

                Console.WriteLine("-Set " + i + " :" + (paramSet[i] / 256) + "   " + (paramSet[i] % 256));
               // Console.WriteLine("-Set " + i + " :" + (Convert.ToByte(paramSet[i] / 256)) + "   " + (Convert.ToByte(paramSet[i] % 256)));

                order[i + start] = Convert.ToByte(paramSet[i] / 256);
                order[i + start + 1] = Convert.ToByte(paramSet[i] % 256);

                Console.WriteLine((i + 7) + ": " + order[i + 7] + "  " +(i + 8) + ": " + order[i + 8]);
                start ++;
            }

            Console.WriteLine("22 SEND:");
            foreach(int i in order){
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }

            send(order, 22, ORDER.L22);
        }

        /// <summary>
        /// 从下位机查询充值信息
        /// </summary>
        public void order31(object sender, EventArgs e)
        {
            Label[] lbls = { lblRemainDays, lblRemainTimes, lblUsedTimes, lblLimitWay};

            for (int i = 0; i < lbls.Length; i++)
            {
                lbls[i].Text = null;
            }

            tbxTUId.Text = null;

            send(ORDER.Z31, 31, ORDER.L31);
        }

        /// <summary>
        /// 清零下位机充值信息
        /// </summary>
        public void order32(object sender, EventArgs e)
        {
            Label[] lbls = { lblRemainDays, lblRemainTimes, lblUsedTimes, lblLimitWay };

            for (int i = 0; i < lbls.Length; i++)
            {
                lbls[i].Text = null;
            }

            tbxTUId.Text = null;

            send(ORDER.Z32, 32, ORDER.L32);
        }

        /// <summary>
        /// 充值
        /// </summary>
        public void order33(object sender, EventArgs e)
        {
            //用充值码填充指令
            //... ...

            int [] tus = new int[6]; //6个充值码输入框
            TextBox[] tbxTUs = { tbxTU1, tbxTU2, tbxTU3, tbxTU4, tbxTU5, tbxTU6 };

            for(int i = 0; i < tbxTUs.Length; i++){
                tbxTUs[i].BackColor = Color.White;
            }

            for(int i = 0; i < tus.Length; i++){
                try { tus[i] = Int32.Parse(tbxTUs[i].Text.Trim()); } //犯了个大错误，数组元素的值是不能被改变的
                catch(Exception ex)
                { MessageBox.Show("激活码格式有误：\r\n" + ex.Message); tbxTUs[i].BackColor = Color.Gold; return; }
            }

            Console.WriteLine("充值码转换后：");

            //转换成4字节byte
            byte[] bytes1 = new byte[4];
            bytes1[3] = (byte) (tus[0] & 0xFF);  
            bytes1[2] = (byte) (tus[0] >> 8 & 0xFF);  
            bytes1[1] = (byte) (tus[0] >> 16 & 0xFF);  
            bytes1[0] = (byte) (tus[0] >> 24 & 0xFF);
            Console.WriteLine(bytes1[0] + " " + bytes1[1] + " " +bytes1[2] + " " +bytes1[3]);

            byte[] bytes2 = new byte[4];
            bytes2[3] = (byte) (tus[1] & 0xFF);  
            bytes2[2] = (byte) (tus[1] >> 8 & 0xFF);  
            bytes2[1] = (byte) (tus[1] >> 16 & 0xFF);  
            bytes2[0] = (byte) (tus[1] >> 24 & 0xFF); 
             Console.WriteLine(bytes2[0] + " " + bytes2[1] + " " +bytes2[2] + " " +bytes2[3]);

            byte[] bytes3 = new byte[4];
            bytes3[3] = (byte) (tus[2] & 0xFF);  
            bytes3[2] = (byte) (tus[2] >> 8 & 0xFF);  
            bytes3[1] = (byte) (tus[2] >> 16 & 0xFF);  
            bytes3[0] = (byte) (tus[2] >> 24 & 0xFF); 
             Console.WriteLine(bytes3[0] + " " + bytes3[1] + " " +bytes3[2] + " " +bytes3[3]);

            byte[] bytes4 = new byte[4];
            bytes4[3] = (byte) (tus[3] & 0xFF);  
            bytes4[2] = (byte) (tus[3] >> 8 & 0xFF);  
            bytes4[1] = (byte) (tus[3] >> 16 & 0xFF);  
            bytes4[0] = (byte) (tus[3] >> 24 & 0xFF);
             Console.WriteLine(bytes4[0] + " " + bytes4[1] + " " +bytes4[2] + " " +bytes4[3]);

            byte[] bytes5 = new byte[4];
            bytes5[3] = (byte)(tus[4] & 0xFF);
            bytes5[2] = (byte)(tus[4] >> 8 & 0xFF);
            bytes5[1] = (byte)(tus[4] >> 16 & 0xFF);
            bytes5[0] = (byte)(tus[4] >> 24 & 0xFF);
             Console.WriteLine(bytes5[0] + " " + bytes5[1] + " " +bytes5[2] + " " +bytes5[3]);

            byte[] bytes6 = new byte[4];
            bytes6[3] = (byte)(tus[5] & 0xFF);
            bytes6[2] = (byte)(tus[5] >> 8 & 0xFF);
            bytes6[1] = (byte)(tus[5] >> 16 & 0xFF);
            bytes6[0] = (byte)(tus[5] >> 24 & 0xFF); 
             Console.WriteLine(bytes6[0] + " " + bytes6[1] + " " +bytes6[2] + " " +bytes6[3]);

             byte[] order = new byte[ORDER.Z33.Length];

            //填充order
            for(int i = 0; i < 5; i++){
                order[i] = ORDER.Z33[i];
            }

            int idx = 0;
            for (int i = 6; i < 10; i++) { order[i] = bytes1[idx]; idx++; } idx = 0;
            for (int i = 10; i < 14; i++) { order[i] = bytes2[idx]; idx++; } idx = 0;
            for (int i = 14; i < 18; i++) { order[i] = bytes3[idx]; idx++; } idx = 0;
            for (int i = 18; i < 22; i++) { order[i] = bytes4[idx]; idx++; } idx = 0;
            for (int i = 22; i < 26; i++) { order[i] = bytes5[idx]; idx++; } idx = 0;
            for (int i = 26; i < 30; i++) { order[i] = bytes6[idx]; idx++; } idx = 0;

            send(order, 33, ORDER.L33);
        }

        /// <summary>
        /// 串口收到下位机返回的数据
        /// </summary>
        public void dataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if(currOrder <= 0){ return; }
            Console.WriteLine("命令" + currOrder + "  【设置】dataReceive事件  期望长度：" + currExpLength);

            while (currOrder > 0 && Global.sp.BytesToRead < currExpLength)
            {
                if (isCommOver) { return; }
                if (!isOrderOn) { return; }; //isOrderOn如果为false，说明本次命令已超时，被停止了
            }
            //数据位完整之后
            int[] resp = new int[currExpLength];
            for (int i = 0; i < currExpLength; i++) { resp[i] = (int)Global.sp.ReadByte(); }

            isOrderOn = false; //数据正常收到并处理完成之后的标记位该表

            Console.WriteLine("命令" + currOrder + "  OK-数据读取完毕 ");

            Global.resp = resp;

            switch (currOrder)
            {
                case 21:
                    this.Invoke(new EventHandler(getResp21));
                    break;

                case 22:
                    this.Invoke(new EventHandler(getResp22));
                    break;

                case 31:
                    this.Invoke(new EventHandler(getResp31));
                    break;

                case 32:
                    this.Invoke(new EventHandler(getResp32));
                    break;

                case 33:
                    this.Invoke(new EventHandler(getResp33));
                    break;
            }
        }

        public void getResp21(object sender, EventArgs e)
        {
            Console.WriteLine("0...");
            int[] resp = Global.resp;
            Console.WriteLine("21 RESP：");
            foreach(int i in resp){
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }

            //showResp(resp);
            Console.WriteLine("RESP21: " + resp.Length);
            Console.WriteLine("1...");
            //把获取的下位机参数显示出来
            int[] param = new int[24];
            param[0] = resp[3]*256 + resp[4];
            param[1] = resp[5]*256 + resp[6];
            param[2] = resp[7]*256 + resp[8];
            param[3] = resp[9]*256 + resp[10];
            param[4] = resp[11]*256 + resp[12];
            param[5] = resp[13]*256 + resp[14];
            param[6] = resp[15]*256 + resp[16];
            param[7] = resp[17]*256 + resp[18];
            param[8] = resp[19]*256 + resp[20];
            param[9] = resp[21]*256 + resp[22];
            param[10] = resp[23]*256 + resp[24];
            param[11] = resp[25]*256 + resp[26];
            param[12] = resp[27]*256 + resp[28];
            param[13] = resp[29]*256 + resp[30];
            param[14] = resp[31]*256 + resp[32];
            param[15] = resp[33]*256 + resp[34];
            param[16] = resp[35]*256 + resp[36];
            param[17] = resp[37]*256 + resp[38];
            param[18] = resp[39]*256 + resp[40];
            param[19] = resp[41]*256 + resp[42];
            //V1.2新加的参数
            param[20] = resp[43]*256 + resp[44];
            param[21] = resp[45]*256 + resp[46];
            param[22] = resp[47]*256 + resp[48];
            param[23] = resp[49]*256 + resp[50];

            Console.WriteLine("2...");
            tbxTempH4.Text = param[0] / 256 + ""; tbxTempL4.Text = param[0] % 256 + "";
            tbxTempH2.Text = param[1] / 256 + ""; tbxTempL2.Text = param[1] % 256 + "";
            tbxTempH3.Text = param[2] / 256 + ""; tbxTempL3.Text = param[2] % 256 + "";
            tbxTempH1.Text = param[3] / 256 + ""; tbxTempL1.Text = param[3] % 256 + "";
            Console.WriteLine("3...");
            tbxTempHC.Text = handleTempForShow(param[4]);
            tbxBodyTemp.Text = handleTempForShow(param[5]);
            tbxDistance.Text = param[6] + "";
            tbxDelay.Text = param[7] + "";
            initFanSpeed(param[8]);
            tbxHorDGLength.Text = param[9] + "";
            tbxVerDGLength.Text = param[10] + "";
            tbxHorSpeedQ.Text = param[11] / 256 + ""; tbxHorSpeedT.Text = param[11] % 256 + "";
            tbxVerSpeedQ.Text = param[12] / 256 + ""; tbxVerSpeedT.Text = param[12] % 256 + "";
            tbxVibrateRange.Text = param[13] + "";
            tbxVibrateSpeedQ.Text = param[14] / 256 + ""; tbxVibrateSpeedT.Text = param[14] % 256 + "";
            cbxAutoUpload.Checked = (param[15] == 1?true:false);
            tbxTempXZ1.Text = handleTempForShow2(param[16]);
            tbxTempXZ2.Text = handleTempForShow2(param[17]);
            tbxTempXZ3.Text = handleTempForShow2(param[18]);
            tbxTempXZ4.Text = handleTempForShow2(param[19]);

            tbxVibrateDelay.Text = param[20] + "";
            tbxLoadSpeedQ.Text = param[21] / 256 + ""; tbxLoadSpeedT.Text = param[21] % 256 + "";
            tbxCtrlCode.Text = param[22] + "";
            tbxGateDelay.Text = param[23] + "";

            Console.WriteLine("4...");
        }

        public void getResp22(object sender, EventArgs e)
        {
            int[] resp = Global.resp;
            Console.WriteLine("22 RESP：");
            foreach (int i in resp)
            {
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }
            //showResp(resp);
        }

        public void getResp31(object sender, EventArgs e)
        {
            int[] resp = Global.resp;
            Console.WriteLine("31 RESP：");
            foreach (int i in resp)
            {
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }

            int info = 0;

            info = resp[3] * 256 + resp[4];
            string msg = "";

            switch (info)
            {
                case LW_UNLIMITED: msg = "无限制"; break;
                case LW_DAYS: msg = "天数"; break;
                case LW_TIMES: msg = "次数"; break;
            }

            if (info == LW_UNLIMITED || info == LW_DAYS || info == LW_TIMES)
            {
                lblLimitWay.Text = msg;
            }
            Console.WriteLine("限制方式:" + info);

            info = resp[5] * 256 + resp[6];
            lblRemainDays.Text = info + "";
            Console.WriteLine("剩余天数统计:" + info);

            info = resp[7] * 256 + resp[8];
            lblRemainTimes.Text = info + "";
            Console.WriteLine("剩余次数统计:" + info);

            info = resp[9] * 256 + resp[10];
            lblUsedTimes.Text = info + "";
            Console.WriteLine("使用次数统计:" + info);

            info = resp[11] * 256 + resp[12];
            Console.WriteLine("设备充值ID:" + info);
            tbxTUId.Text = info + "";

            lblTUMsg.Text = DateTime.Now.ToString("HH:mm:ss") + "  查询完成";

            //showResp(resp);
        }

        public void getResp32(object sender, EventArgs e)
        {
            int[] resp = Global.resp;
            Console.WriteLine("32 RESP：");
            foreach (int i in resp)
            {
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }

            int RESET_OK = 5;
            int resetResult = resp[3] * 256 + resp[4];
            if(resetResult == RESET_OK){
                lblTUMsg.Text = DateTime.Now.ToString("HH:mm:ss") + "  清零成功";
            }

            int info = 0;

            info = resp[3] * 256 + resp[4];
            string msg = "";

            switch (resetResult)
            {
                case LW_UNLIMITED: msg = "无限制"; break;
                case LW_DAYS: msg = "天数"; break;
                case LW_TIMES: msg = "次数"; break;
            }

            if (info == LW_UNLIMITED || info == LW_DAYS || info == LW_TIMES)
            {
                lblLimitWay.Text = msg;
            }

            Console.WriteLine("限制方式:" + info);

            info = resp[5] * 256 + resp[6];
            lblRemainDays.Text = info + "";
            Console.WriteLine("剩余天数统计:" + info);

            info = resp[7] * 256 + resp[8];
            lblRemainTimes.Text = info + "";
            Console.WriteLine("剩余次数统计:" + info);

            info = resp[9] * 256 + resp[10];
            lblUsedTimes.Text = info + "";
            Console.WriteLine("使用次数统计:" + info);

            info = resp[11] * 256 + resp[12];
            Console.WriteLine("设备充值ID:" + info);
            tbxTUId.Text = info + "";

            //showResp(resp);
        }

        public void getResp33(object sender, EventArgs e)
        {
            int[] resp = Global.resp;
            Console.WriteLine("33 RESP：");
            foreach (int i in resp)
            {
                Console.Write(i.ToString("x2").ToUpper() + " ");
            }

            int resetResult = resp[3] * 256 + resp[4];
            string msg = "";
            switch (resetResult)
            {
                case LW_UNLIMITED: msg = "无限制"; break;
                case LW_DAYS: msg = "天数"; break;
                case LW_TIMES: msg = "次数"; break;
                case LW_TU_OK: msg = "充值成功"; break;
                case LW_TU_FAIL: msg = "充值失败"; break;
                case LW_RESET_OK: msg = "清零成功"; break;
            }
            
             lblTUMsg.Text = DateTime.Now.ToString("HH:mm:ss") + "  " + msg;

             int info = 0;

            info = resp[3] * 256 + resp[4];
            if (info == LW_DAYS || info == LW_TIMES)
            {
                lblLimitWay.Text = msg;
            }
           
            Console.WriteLine("限制方式:" + info);

            info = resp[5] * 256 + resp[6];
            lblRemainDays.Text = info + "";
            Console.WriteLine("剩余天数统计:" + info);

            info = resp[7] * 256 + resp[8];
            lblRemainTimes.Text = info + "";
            Console.WriteLine("剩余次数统计:" + info);

            info = resp[9] * 256 + resp[10];
            lblUsedTimes.Text = info + "";
            Console.WriteLine("使用次数统计:" + info);

            info = resp[11] * 256 + resp[12];
            Console.WriteLine("设备充值ID:" + info);
            tbxTUId.Text = info + "";

            //showResp(resp);
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
            Console.WriteLine("showResp...");
            string msg = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 返回：";
           
            for (int i = 0; i < resp.Length; i++)
            {
                string hex = Convert.ToString(resp[i], 16).ToUpper();
                msg += (hex.Length == 1 ? "0" + hex : hex) + " ";
            }
            Console.WriteLine(msg);
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
        /// 处理格式，显示小数点（包含负数的情况）
        /// </summary>
        private string handleTempForShow2(int x)
        {
            if(x > 127){ //大于127说明最高位是1，是负数
                x = x - 256;
            }
            Console.WriteLine("forShow:" + x);
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
        /// 更新密码(设备参数）
        /// </summary>
        private void updatePwd(Object sender , EventArgs e)
        {
            if (tbxPwdOld.Text.Trim().Length == 0) { lblMsg.ForeColor = Color.Red; lblMsg.Text = "请输入原密码"; return; }
            if (tbxPwdNew.Text.Trim().Length == 0) { lblMsg.ForeColor = Color.Red; lblMsg.Text = "请输入新密码"; return; }

            string md5 = new FnString().toMD5(tbxPwdOld.Text.Trim());
            if (md5.Equals(config2.password))
            {
                config2.password = new FnString().toMD5(tbxPwdNew.Text.Trim());
                string cfgStr = JsonConvert.SerializeObject(config2);
                new FnFile().toConfig2(cfgStr);
                lblMsg.ForeColor = Color.Blue;
                lblMsg.Text = "密码已更新";
            }
            else
            {
                lblMsg.ForeColor = Color.Red;
                lblMsg.Text = "原密码不正确"; 
            }

        }

        /// <summary>
        /// 更新密码（使用许可）
        /// </summary>
        private void updatePwd2(Object sender, EventArgs e)
        {
            if (tbxPwdOld2.Text.Trim().Length == 0) { lblMsg2.ForeColor = Color.Red; lblMsg2.Text = "请输入原密码"; return; }
            if (tbxPwdNew2.Text.Trim().Length == 0) { lblMsg2.ForeColor = Color.Red; lblMsg2.Text = "请输入新密码"; return; }

            string md5 = new FnString().toMD5(tbxPwdOld2.Text.Trim());
            if (md5.Equals(config2.password2))
            {
                config2.password2 = new FnString().toMD5(tbxPwdNew2.Text.Trim());
                string cfgStr = JsonConvert.SerializeObject(config2);
                new FnFile().toConfig2(cfgStr);
                lblMsg2.ForeColor = Color.Blue;
                lblMsg2.Text = "密码已更新";
            }
            else
            {
                lblMsg2.ForeColor = Color.Red;
                lblMsg2.Text = "原密码不正确";
            }

        }

        /// <summary>
        /// Tab切换
        /// </summary>
        private void changeTab(object sender, EventArgs e)
        {
            switch(tabCtrl.SelectedIndex){
                case 0: 
                    //do nothing
                    break;
                case 1:
                    applyTarget = TARGET_CONFIG2;
                   
                    if(!config2Access){
                        int source = SOURCE_1;
                        if (Application.OpenForms["FormPassword"] == null)
                        {
                            FormPassword form = new FormPassword(this, source);
                            form.Text = "设备参数 验证";
                            form.Show();
                        }
                        else
                        {
                            FormPassword form = (FormPassword)Application.OpenForms["FormPassword"];
                            form.source = source;
                            form.Text = "设备参数 验证";
                            form.lblMsg.Text = null;
                            form.Show();
                            form.Focus();
                        }
                    }
                    break;
                case 2:
                    applyTarget = TARGET_PERMISSION;
                   
                    if (!perssionAccess) {
                        int source = SOURCE_2;
                        if (Application.OpenForms["FormPassword"] == null)
                        {
                            FormPassword form = new FormPassword(this, source);
                            form.Text = "使用许可 验证";
                            form.Show();
                        }
                        else
                        {
                            FormPassword form = (FormPassword)Application.OpenForms["FormPassword"];
                            form.source = source;
                            form.Text = "使用许可 验证";
                            form.lblMsg.Text = null;
                            form.Show();
                            form.Focus();
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// 重新打开手动模式窗口
        /// </summary>
        public void openFormManual()
        {
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
        /// 关闭窗口
        /// </summary>
        public void closeForm(object sender, EventArgs e)
        {
            haveARest();
            openFormManual();
        }

        /// <summary>
        /// 重新show的时候做点事情
        /// </summary>
        public void showAgain()
        {
            if (Global.sp != null && Global.sp.IsOpen)
            {
                Global.sp.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
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
            if (Global.sp != null && Global.sp.IsOpen)
            {
                Global.sp.DataReceived -= new System.IO.Ports.SerialDataReceivedEventHandler(dataReceived);
                Global.sp.ReadExisting();
                Global.sp.Close();
                Global.sp.Open();
            }
            
            this.Hide();
        }

        /// <summary>
        /// 登录成功
        /// </summary>
        internal void accessOK()
        {
            switch(applyTarget){
                case TARGET_CONFIG2:
                     config2Access = true;
                     tabCtrl.SelectedIndex = 1;
                     
                    break;

                case TARGET_PERMISSION:
                     perssionAccess = true;
                     tabCtrl.SelectedIndex = 2;
                    break;
            }

            showParts();
           
        }

        /// <summary>
        /// 密码正确，进入界面
        /// </summary>
        private void showParts()
        {
            switch (applyTarget)
            {
                case TARGET_CONFIG2:
                    gbxPart1.Show();
                    gbxPart2.Show();
                    gbxPart3.Show();
                    btnSave2.Show();
                    btnClose2.Show();

                    break;

                case TARGET_PERMISSION:
                    panelPerssion.Show();

                    break;
            }


            
        }

  
    }
}
