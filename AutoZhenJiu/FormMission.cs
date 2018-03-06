using System;
using System.Collections.Generic;
using System.Collections;
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
    public partial class FormMission : Form
    {
        Button[] btns;
        List<MissionPoint> mps;
        Hashtable xw_btn, xw_time;

        Config1 config1;
        FormAuto formAuto;

        public FormMission(FormAuto formAuto)
        {
            InitializeComponent();

            this.formAuto = formAuto;
            getConfig1();

            init();

            mps = new List<MissionPoint>();

            btnAdd.Click += new EventHandler(addMissionPoint);
            btnRemoveLast.Click += new EventHandler(removeMissionPoint);
            btnOK.Click += new EventHandler(missionOK);
            btnReset.Click += new EventHandler(reset);
            btnClose.Click += new EventHandler(closeForm);

            btnF1.Click += delegate(Object o, EventArgs e) { setNo(btnF1, "F1"); }; btnB1.Click += delegate(Object o, EventArgs e) { setNo(btnB1, "B1"); };
            btnF2.Click += delegate(Object o, EventArgs e) { setNo(btnF2, "F2"); }; btnB2.Click += delegate(Object o, EventArgs e) { setNo(btnB2, "B2"); };
            btnF3.Click += delegate(Object o, EventArgs e) { setNo(btnF3, "F3"); }; btnB3.Click += delegate(Object o, EventArgs e) { setNo(btnB3, "B3"); };
            btnF4.Click += delegate(Object o, EventArgs e) { setNo(btnF4, "F4"); }; btnB4.Click += delegate(Object o, EventArgs e) { setNo(btnB4, "B4"); };
            btnF5.Click += delegate(Object o, EventArgs e) { setNo(btnF5, "F5"); }; btnB5.Click += delegate(Object o, EventArgs e) { setNo(btnB5, "B5"); };
            btnF6.Click += delegate(Object o, EventArgs e) { setNo(btnF6, "F6"); }; btnB6.Click += delegate(Object o, EventArgs e) { setNo(btnB6, "B6"); };
            btnF7.Click += delegate(Object o, EventArgs e) { setNo(btnF7, "F7"); }; btnB7.Click += delegate(Object o, EventArgs e) { setNo(btnB7, "B7"); };
            btnF8.Click += delegate(Object o, EventArgs e) { setNo(btnF8, "F8"); }; btnB8.Click += delegate(Object o, EventArgs e) { setNo(btnB8, "B8"); };

        }

        private void FormMission_Load(object sender, EventArgs e)
        {
            xw_btn = new Hashtable();
            xw_btn.Add("F1", btnF1); xw_btn.Add("B1", btnB1);
            xw_btn.Add("F2", btnF2); xw_btn.Add("B2", btnB2);
            xw_btn.Add("F3", btnF3); xw_btn.Add("B3", btnB3);
            xw_btn.Add("F4", btnF4); xw_btn.Add("B4", btnB4);
            xw_btn.Add("F5", btnF5); xw_btn.Add("B5", btnB5);
            xw_btn.Add("F6", btnF6); xw_btn.Add("B6", btnB6);
            xw_btn.Add("F7", btnF7); xw_btn.Add("B7", btnB7);
            xw_btn.Add("F8", btnF8); xw_btn.Add("B8", btnB8);

            //穴位（按钮）与其对应的时间进行关联
            xw_time = new Hashtable();
            xw_time["F1"] = formAuto.frontTime[0]; xw_time["B1"] = formAuto.backTime[0];
            xw_time["F2"] = formAuto.frontTime[1]; xw_time["B2"] = formAuto.backTime[1];
            xw_time["F3"] = formAuto.frontTime[2]; xw_time["B3"] = formAuto.backTime[2];
            xw_time["F4"] = formAuto.frontTime[3]; xw_time["B4"] = formAuto.backTime[3];
            xw_time["F5"] = formAuto.frontTime[4]; xw_time["B5"] = formAuto.backTime[4];
            xw_time["F6"] = formAuto.frontTime[5]; xw_time["B6"] = formAuto.backTime[5];
            xw_time["F7"] = formAuto.frontTime[6]; xw_time["B7"] = formAuto.backTime[6];
            xw_time["F8"] = formAuto.frontTime[7]; xw_time["B8"] = formAuto.backTime[7];

            Console.WriteLine("配置：" + config1);
        }

        /// <summary>
        /// 按钮文字赋值
        /// </summary>
        private void init()
        {
     
            btns = new Button[16];
            btns[0] = btnF1; btns[1] = btnF2; btns[2] = btnF3; btns[3] = btnF4; btns[4] = btnF5; btns[5] = btnF6; btns[6] = btnF7; btns[7] = btnF8; 
            btns[8] = btnB1; btns[9] = btnB2; btns[10] = btnB3; btns[11] = btnB4; btns[12] = btnB5; btns[13] = btnB6; btns[14] = btnB7; btns[15] = btnB8; 

            //也可用循环
            btnF1.Text = formAuto.frontZH[0]; btnB1.Text = formAuto.backZH[0];
            btnF2.Text = formAuto.frontZH[1]; btnB2.Text = formAuto.backZH[1];
            btnF3.Text = formAuto.frontZH[2]; btnB3.Text = formAuto.backZH[2];
            btnF4.Text = formAuto.frontZH[3]; btnB4.Text = formAuto.backZH[3];
            btnF5.Text = formAuto.frontZH[4]; btnB5.Text = formAuto.backZH[4];
            btnF6.Text = formAuto.frontZH[5]; btnB6.Text = formAuto.backZH[5];
            btnF7.Text = formAuto.frontZH[6]; btnB7.Text = formAuto.backZH[6];
            btnF8.Text = formAuto.frontZH[7]; btnB8.Text = formAuto.backZH[7];

        }

        private bool checkFormat()
        {
            if (tbxNo.Text.Trim().Length == 0)
            {
                MessageBox.Show("请选择穴位");
                return false;
            }

            int x = 0;
            if (!Int32.TryParse(tbxTime.Text, out x))
            {
                MessageBox.Show("时间格式有误");
                return false;
            }
            else if(x <= 0)
            {
                MessageBox.Show("时间必须大于零");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 选中
        /// </summary>
        public void setNo(Button btn, string noReal)
        {
            /*
            if(btnAdd.Enabled == false){
                tbxNo.Text = btn.Text;
                tbxNoReal.Text = noReal;
                btn.Enabled = false; 
                btn.BackColor = Color.Green;

                btnAdd.Enabled = true;
            }
             */

            //逻辑重新组合，每个穴位都可以重复加入任务
            //1. 判断当前按钮是否选中，如果选中，则取消之。如果没选中，就进行处理
            if (btn.BackColor == Color.Green) //如果本来就选中，则取消
            {
                tbxNo.Text = null;
                tbxNoReal.Text = null;
                tbxTime.Text = null;
                btn.BackColor = Color.Gray;

                btnAdd.Enabled = false;
            }
            else //如果本来没选中，则选中
            {
                //当前的选中，其他的取消选中(实现单选的效果)
                foreach(Button temp in btns){
                    if (temp == btn)
                    {
                        tbxNo.Text = btn.Text;
                        tbxNoReal.Text = noReal;
                        tbxTime.Text = (string)xw_time[noReal];
                        btn.BackColor = Color.Green;
                    }
                    else
                    {
                        temp.BackColor = Color.Gray;
                    }
                }

                btnAdd.Enabled = true; 

            }

        }

        /// <summary>
        /// 添加到列表中
        /// </summary>
        public void addMissionPoint(object sender, EventArgs e)
        {
            if(!checkFormat()){ return; }
            MissionPoint mp = new MissionPoint();
            mp.no = tbxNoReal.Text;
            mp.isVibrate = cbxIsVibrate.Checked;
            mp.time = Int32.Parse(tbxTime.Text) * 60;
            mp.timeFixed = mp.time;

            mps.Add(mp);

            //文字信息改为整体刷新
            string info = "";
            foreach (MissionPoint temp in mps)
            {
                info += ((Button)xw_btn[temp.no]).Text + " " + (temp.isVibrate ? "振动" : "一一") + "  " + temp.time / 60 + " 分钟\r\n";
            }
            lblMission.Text = info;
           
        }

        /// <summary>
        /// 从列表中删除最后一个
        /// </summary>
        public void removeMissionPoint(object sender, EventArgs e)
        {
            //找到最近一次的mp和与之对应的btn
            if (mps.Count > 0)
            {
                MissionPoint mp = mps[mps.Count-1];
                mps.Remove(mp);

                //文字信息改为整体刷新
                string info = "";
                foreach (MissionPoint temp in mps)
                {
                    info += ((Button)xw_btn[temp.no]).Text + " " + (temp.isVibrate ? "振动" : "一一") + "  " + temp.time / 60 + " 分钟\r\n";
                }
                lblMission.Text = info;

            }

        }    

        /// <summary>
        /// 添加到列表中
        /// </summary>
        public void missionOK(object sender, EventArgs e)
        {
            Form form = Application.OpenForms["FormAuto"];
            ((FormAuto)form).updatePoints(mps);

            if (mps.Count > 0)
            {
                ((FormAuto)form).lblMission.Text = "已设定";
                ((FormAuto)form).toolTip.SetToolTip(((FormAuto)form).lblMission, lblMission.Text);
            }
            else
            {
                ((FormAuto)form).lblMission.Text = "-";
                ((FormAuto)form).toolTip.SetToolTip(((FormAuto)form).lblMission, null);
            }

            this.Hide();
            ((FormAuto)form).timerWelcome.Start();
        }

        /// <summary>
        /// 隐藏
        /// </summary>
        public void closeForm(object sender, EventArgs e)
        {
            this.Hide();
            Form form = Application.OpenForms["FormAuto"];
            ((FormAuto)form).timerWelcome.Start();
        }

        /// <summary>
        /// 选中
        /// </summary>
        public void reset(object sender, EventArgs e)
        {
            mps.Clear();
            //按钮
            btnAdd.Enabled = false;

            btnF1.Enabled = true; btnF1.BackColor = Color.Gray;
            btnF2.Enabled = true; btnF2.BackColor = Color.Gray;
            btnF3.Enabled = true; btnF3.BackColor = Color.Gray;
            btnF4.Enabled = true; btnF4.BackColor = Color.Gray;
            btnF5.Enabled = true; btnF5.BackColor = Color.Gray;
            btnF6.Enabled = true; btnF6.BackColor = Color.Gray;
            btnF7.Enabled = true; btnF7.BackColor = Color.Gray;
            btnF8.Enabled = true; btnF8.BackColor = Color.Gray;

            btnB1.Enabled = true; btnB1.BackColor = Color.Gray;
            btnB2.Enabled = true; btnB2.BackColor = Color.Gray;
            btnB3.Enabled = true; btnB3.BackColor = Color.Gray;
            btnB4.Enabled = true; btnB4.BackColor = Color.Gray;
            btnB5.Enabled = true; btnB5.BackColor = Color.Gray;
            btnB6.Enabled = true; btnB6.BackColor = Color.Gray;
            btnB7.Enabled = true; btnB7.BackColor = Color.Gray;
            btnB8.Enabled = true; btnB8.BackColor = Color.Gray;

            //...
            tbxNo.Text = ""; tbxNoReal.Text = ""; tbxTime.Text = ""; cbxIsVibrate.Checked = false;

            //lbl
            lblMission.Text = "";
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

    }
}
