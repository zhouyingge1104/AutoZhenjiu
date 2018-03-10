using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AutoZhenJiu.database;
using AutoZhenJiu.entity;
using AutoZhenJiu.fn;
using AutoZhenJiu.api_idcard;

namespace AutoZhenJiu
{
    partial class FormMR : Form
    {
        const int DIRECTION_FRONT = 0;
        const int DIRECTION_BACK = 1;

        public Client client;
        public List<MR> mrs;
        public DBsqlite db;
        public Config1 config1;

        DataGridViewCell[] fxs, fys, fTimes, fZHs, bxs, bys, bTimes, bZHs;

        Order ORDER;
        FnComm fnComm;

        FormAuto formAuto;

        //读身份证相关
        int iRetUSB = 0, iRetCOM = 0;

        public FormMR(FormAuto formAuto)
        {
            InitializeComponent();

            this.formAuto = formAuto;

            init();
            initUI();
         //更新数据库
            updateDB();

            btnGetClientInfo.Click += new EventHandler(getClientInfo);
            btnSave.Click += new EventHandler(save);
            btnAddMR.Click += new EventHandler(addMR);
            btnLogout.Click += new EventHandler(logout);
            btnClose.Click += new EventHandler(closeForm);
            btnLoadParamDFT.Click += new EventHandler(loadParamDFT);

            btnReadCard.Click += new EventHandler(readCard);

            dgvMR.CellDoubleClick += new DataGridViewCellEventHandler (editMR);

            dgvParamF.CellClick += new DataGridViewCellEventHandler(dgvParamFCellClick);
            dgvParamB.CellClick += new DataGridViewCellEventHandler(dgvParamBCellClick);

            tbcParam.SelectedIndexChanged += new EventHandler(changeTab);

            rbIdCard.Click += new EventHandler(changeNoType);
            rbPhone.Click += new EventHandler(changeNoType);

        }

        /// <summary>
        /// 保存本次病历信息
        /// </summary>
        public void init()
        {
            try
            {
                db = new DBsqlite(System.Environment.CurrentDirectory + "\\" + Dict.DB_NAME);
            }catch(Exception ex){
                MessageBox.Show("加载数据库失败\r\n" + ex.Message);
                this.Close();
            }

            ORDER = new Order().init();
            fnComm = new FnComm();

        }

        /// <summary>
        /// 设置界面信息
        /// </summary>
        public void initUI()
        {
            dgvDescF.Rows.Clear();
            dgvDescB.Rows.Clear();

            for (int i = 0; i < 4; i++) {dgvDescF.Rows.Add();}
            dgvDescF.Rows[0].Cells[0].Value = "X";
            dgvDescF.Rows[1].Cells[0].Value = "Y";
            dgvDescF.Rows[2].Cells[0].Value = "时间";
            dgvDescF.Rows[3].Cells[0].Value = "名称";

            for (int i = 0; i < 4; i++) { dgvDescB.Rows.Add(); }
            dgvDescB.Rows[0].Cells[0].Value = "X";
            dgvDescB.Rows[1].Cells[0].Value = "Y";
            dgvDescB.Rows[2].Cells[0].Value = "时间";
            dgvDescB.Rows[3].Cells[0].Value = "名称";

            dgvParamF.Rows.Clear();
            dgvParamB.Rows.Clear();

            //init dgvParam
            for (int i = 0; i < 4; i++ ) //4行
            {
                dgvParamF.Rows.Add();
            }
            foreach(DataGridViewRow row in dgvParamF.Rows){
                foreach(DataGridViewCell cell in row.Cells){
                    cell.ReadOnly = false;
                }
            }

            DataGridViewRow dr = new DataGridViewRow();
           
            for (int i = 0; i < 8; i++)
            {
                DataGridViewButtonCell btnCell = new DataGridViewButtonCell();
                btnCell.Value = "测试";
                btnCell.Style.BackColor = Color.Gray;
                btnCell.Style.ForeColor = Color.White;
                dr.Cells.Add(btnCell);
            }
   
            dgvParamF.Rows.Add(dr);

            //---------------------------------------------------
            //init dgvParam
            for (int i = 0; i < 4; i++) //4行
            {
                dgvParamB.Rows.Add();
            }
            foreach (DataGridViewRow row in dgvParamB.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.ReadOnly = false;
                }
            }

            dr = new DataGridViewRow();
           
            for (int i = 0; i < 8; i++)
            {
                DataGridViewButtonCell btnCell = new DataGridViewButtonCell();
                btnCell.Value = "测试";
                btnCell.Style.BackColor = Color.Gray;
                btnCell.Style.ForeColor = Color.White;
                dr.Cells.Add(btnCell);
            }
            dgvParamB.Rows.Add(dr);

            makeTeams();

            if (client != null)
            {
                tbxIdCardNo.Text = client.idCardNo;
                tbxPhoneNo.Text = client.phoneNo;
                tbxName.Text = client.name;
                tbxAge.Text = client.age + "";
                tbxHeight.Text = client.height;
                tbxWeight.Text = client.weight;
                cbxGender.SelectedIndex = client.gender.Equals(Dict.MALE) ? 0 : 1;

                if (client.param != null)
                {
                    showClientParam();
                }
                else
                {
                    loadParamDFT(null, null);
                }

            }
            else
            {
                loadParamDFT(null, null);
            }

           
     
        }

        private void FormMR_Load(object sender, EventArgs e)
        {
            //身份证读卡相关
            try
            {

                int iPort;
                for (iPort = 1001; iPort <= 1016; iPort++)
                {
                    //Console.WriteLine("iPort:" + iPort);
                    iRetUSB = CVRSDK.CVR_InitComm(iPort);
                    if (iRetUSB == 1)
                    {
                        break;
                    }
                }
                if (iRetUSB != 1)
                {
                    for (iPort = 1; iPort <= 4; iPort++)
                    {
                        iRetCOM = CVRSDK.CVR_InitComm(iPort);
                        if (iRetCOM == 1)
                        {
                            break;
                        }
                    }
                }

                if ((iRetCOM == 1) || (iRetUSB == 1))
                {
                    lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " 初始化成功！";
                }
                else
                {
                    lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " 初始化失败！";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        public void makeTeams()
        {
            fxs = new DataGridViewCell[8]; fys = new DataGridViewCell[8]; fTimes = new DataGridViewCell[8]; fZHs = new DataGridViewCell[8];
            bxs = new DataGridViewCell[8]; bys = new DataGridViewCell[8]; bTimes = new DataGridViewCell[8]; bZHs = new DataGridViewCell[8];
          
            for (int i = 0; i < 8; i++ )
            {
                fxs[i] = dgvParamF.Rows[0].Cells[i];
                fys[i] = dgvParamF.Rows[1].Cells[i];
                fTimes[i] = dgvParamF.Rows[2].Cells[i];
                fZHs[i] = dgvParamF.Rows[3].Cells[i];
            }

            for (int i = 0; i < 8; i++)
            {
                bxs[i] = dgvParamB.Rows[0].Cells[i];
                bys[i] = dgvParamB.Rows[1].Cells[i];
                bTimes[i] = dgvParamB.Rows[2].Cells[i];
                bZHs[i] = dgvParamB.Rows[3].Cells[i];
            }
            
        }

        public void dgvParamFCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == 4)
            { //如果是在按钮这一行
                testParam(e.ColumnIndex, DIRECTION_FRONT);
            }

        }

        public void dgvParamBCellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex == 4){ //如果是在按钮这一行
                testParam(e.ColumnIndex, DIRECTION_BACK);
            }
            
        }

        /// <summary>
        /// 测试参数是否正确
        /// </summary>
        public void testParam(int colIndex, int direction)
        {
            //MessageBox.Show(colIndex + "");
            int x = 0, y = 0;
            //根据colIndex
            switch(direction){
                case DIRECTION_FRONT:
                    if (dgvParamF.Rows[0].Cells[colIndex].Value != null && dgvParamF.Rows[1].Cells[colIndex].Value != null)
                    {
                      
                        if (Int32.TryParse(dgvParamF.Rows[0].Cells[colIndex].Value.ToString().Trim(), out x)) { }
                        else { MessageBox.Show("参数格式有误"); return; }
                        if (Int32.TryParse(dgvParamF.Rows[1].Cells[colIndex].Value.ToString().Trim(), out y)) { }
                        else { MessageBox.Show("参数格式有误"); return; }
                    }
                    else
                    { MessageBox.Show("未设置参数"); return; }

                    break;

                case DIRECTION_BACK:

                    if (dgvParamB.Rows[0].Cells[colIndex].Value != null && dgvParamB.Rows[1].Cells[colIndex].Value != null)
                    {
                       
                        if (Int32.TryParse(dgvParamB.Rows[0].Cells[colIndex].Value.ToString().Trim(), out x)) { }
                        else { MessageBox.Show("参数格式有误"); return; }
                        if (Int32.TryParse(dgvParamB.Rows[1].Cells[colIndex].Value.ToString().Trim(), out y)) { }
                        else { MessageBox.Show("参数格式有误"); return; }
                    }
                    else
                    { MessageBox.Show("未设置参数"); return; }

                    break;
            }

            

            order18Direct(x, y);
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
        /// 加载默认的穴位参数
        /// </summary>
        public void loadParamDFT(object sender, EventArgs e)
        {
            getConfig1();
            showDFTParam();
        }

        /// <summary>
        /// 填充穴位参数
        /// </summary>
        private void showDFTParam()
        {
            string[] frontX = config1.frontX.Split(",".ToCharArray());
            string[] frontY = config1.frontY.Split(",".ToCharArray());
            string[] frontTime = config1.frontTime.Split(",".ToCharArray());
            string[] frontZH = config1.frontZH.Split(",".ToCharArray());
            string[] backX = config1.backX.Split(",".ToCharArray());
            string[] backY = config1.backY.Split(",".ToCharArray());
            string[] backTime = config1.backTime.Split(",".ToCharArray());
            string[] backZH = config1.backZH.Split(",".ToCharArray());

            for (int i = 0; i < fxs.Length; i++) { fxs[i].Value = frontX[i]; }
            for (int i = 0; i < fys.Length; i++) { fys[i].Value = frontY[i]; }
            for (int i = 0; i < fTimes.Length; i++) { fTimes[i].Value = frontTime[i]; }
            for (int i = 0; i < fZHs.Length; i++) { fZHs[i].Value = frontZH[i]; }
            for (int i = 0; i < bxs.Length; i++) { bxs[i].Value = backX[i]; }
            for (int i = 0; i < bys.Length; i++) { bys[i].Value = backY[i]; }
            for (int i = 0; i < bTimes.Length; i++) { bTimes[i].Value = backTime[i]; }
            for (int i = 0; i < bZHs.Length; i++) { bZHs[i].Value = backZH[i]; }
             
        }

        /// <summary>
        /// 填充穴位参数
        /// </summary>
        private void showClientParam()
        {
            try
            {
                JObject param = JObject.Parse(client.param);
               
                string[] frontX = ((string)param["frontX"]).Split(",".ToCharArray());
                string[] frontY = ((string)param["frontY"]).Split(",".ToCharArray());
                string[] frontTime = ((string)param["frontTime"]).Split(",".ToCharArray());
                string[] frontZH = ((string)param["frontZH"]).Split(",".ToCharArray());
                string[] backX = ((string)param["backX"]).Split(",".ToCharArray());
                string[] backY = ((string)param["backY"]).Split(",".ToCharArray());
                string[] backTime = ((string)param["backTime"]).Split(",".ToCharArray());
                string[] backZH = ((string)param["backZH"]).Split(",".ToCharArray());

                for (int i = 0; i < fxs.Length; i++) { fxs[i].Value = frontX[i]; }
                for (int i = 0; i < fys.Length; i++) { fys[i].Value = frontY[i]; }
                for (int i = 0; i < fTimes.Length; i++) { fTimes[i].Value = frontTime[i]; }
                for (int i = 0; i < fZHs.Length; i++) { fZHs[i].Value = frontZH[i]; }
                for (int i = 0; i < bxs.Length; i++) { bxs[i].Value = backX[i]; }
                for (int i = 0; i < bys.Length; i++) { bys[i].Value = backY[i]; }
                for (int i = 0; i < bTimes.Length; i++) { bTimes[i].Value = backTime[i]; }
                for (int i = 0; i < bZHs.Length; i++) { bZHs[i].Value = backZH[i]; }

            }catch(Exception e){
                MessageBox.Show("载入客户参数出错：\r\n" + e.Message);
            }

            

        }

         /// <summary>
        /// 读身份证
        /// </summary>
        public void readCard(object sender, EventArgs e)
        {
            //不管能否读出，界面先切换
            rbIdCard.Checked = true; panelIdCard.BackColor = Color.SandyBrown;
            rbPhone.Checked = false; panelPhone.BackColor = Color.Transparent;

            try
            {
                if ((iRetCOM == 1) || (iRetUSB == 1))
                {

                    int authenticate = CVRSDK.CVR_Authenticate();
                    if (authenticate == 1)
                    {
                        int readContent = CVRSDK.CVR_Read_Content(4);
                        if (readContent == 1)
                        {
                            lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " 读卡成功！";
                            fillData();
                        }
                        else
                        {
                            lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " 读卡失败";
                        }
                    }
                    else
                    {
                        MessageBox.Show("未放卡或卡片放置不正确");
                    }
                }
                else
                {
                    MessageBox.Show("初始化失败！");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

         /// <summary>
        /// 切换号码类型
        /// </summary>
        public void changeNoType(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            if(rb == rbIdCard){
                rbIdCard.Checked = true; panelIdCard.BackColor = Color.SandyBrown;
                rbPhone.Checked = false; panelPhone.BackColor = Color.Transparent;
            }
            if (rb == rbPhone)
            {
                rbIdCard.Checked = false; panelIdCard.BackColor = Color.Transparent;
                rbPhone.Checked = true; panelPhone.BackColor = Color.SandyBrown;
            }
        }

        /// <summary>
        /// 根据身份证号查询客户信息
        /// </summary>
        public void getClientInfo(object sender, EventArgs e)
        {
            if(rbIdCard.Checked){
                client = db.getClientByIdCardNo(tbxIdCardNo.Text.Trim());
            }
            if (rbPhone.Checked)
            {
                client = db.getClientByPhoneNo(tbxPhoneNo.Text.Trim());
            }

            if(client != null){
                initUI();
                lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " √";
                getMRs();
                btnAddMR.Enabled = true;

                //2018.2.27
                Global.client = client;

                formAuto.init();

            }else{
                clearUI();
                lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") +  " 未查询到客户信息";
            }
        }

        /// <summary>
        /// 根据身份证号查询客户信息(和身份证读取配套）
        /// </summary>
        public void getClientInfoAfterReadCard(object sender, EventArgs e)
        {
            client = db.getClientByIdCardNo(tbxIdCardNo.Text.Trim());
            if (client != null)
            {
                lblMsgQueryClient.Text = DateTime.Now.ToString("HH:mm:ss") + " √";
                getMRs();
                btnAddMR.Enabled = true;

                //2018.2.27
                Global.client = client;
                formAuto.init();
            }
        }

        /// <summary>
        /// 保存本次病历信息
        /// </summary>
        public void save(object sender, EventArgs e)
        {
            if(!checkFormat()){ return; }
            string msg = "";
            //1. 判断该身份证号在系统中是否已存在？
            if(rbIdCard.Checked){
                client = db.getClientByIdCardNo(tbxIdCardNo.Text.Trim());
            }
            if (rbPhone.Checked)
            {
                client = db.getClientByPhoneNo(tbxPhoneNo.Text.Trim());
            }
            

            string addTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (client == null)
            {
                client = new Client();
            }

            client.name = tbxName.Text.Trim();
            client.gender = cbxGender.SelectedIndex == 0 ? Dict.MALE : Dict.FEMALE;
            //根据身份证号取出出生年月
            client.age = Int32.Parse(tbxAge.Text.Trim());
            client.height = tbxHeight.Text.Trim();
            client.weight = tbxWeight.Text.Trim();
            client.idCardNo = tbxIdCardNo.Text.Trim();
            client.phoneNo = tbxPhoneNo.Text.Trim();
            client.addTime = addTime;

            client.param = makeParamContent();
            Console.WriteLine("参数：" + client.param);

            if (client.id < 0){
                 if (db.saveClient(client)) { msg += "客户信息保存成功  "; } else { msg += "客户信息保存失败  "; }
            }
            else{
                if (db.updateClient(client)) { msg += "客户信息更新成功  "; } else { msg += "客户信息更新失败  "; }
            }
           
            client = db.getClientByIdCardNo(client.idCardNo);

            Global.client = client;

            formAuto.init();

            btnAddMR.Enabled = true;
          
            lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;

        }

        /// <summary>
        /// 组织好param字段的内容
        /// </summary>
        public string makeParamContent(){

            //给config赋值
            string frontX = fxs[0].Value + "," +
                                fxs[1].Value + "," +
                                fxs[2].Value + "," +
                                fxs[3].Value + "," +
                                fxs[4].Value + "," +
                                fxs[5].Value + "," +
                                fxs[6].Value + "," +
                                fxs[7].Value;

            string frontY = fys[0].Value + "," +
                                fys[1].Value + "," +
                                fys[2].Value + "," +
                                fys[3].Value + "," +
                                fys[4].Value + "," +
                                fys[5].Value + "," +
                                fys[6].Value + "," +
                                fys[7].Value;

            string frontTime = fTimes[0].Value + "," +
                                    fTimes[1].Value + "," +
                                    fTimes[2].Value + "," +
                                    fTimes[3].Value + "," +
                                    fTimes[4].Value + "," +
                                    fTimes[5].Value + "," +
                                    fTimes[6].Value + "," +
                                    fTimes[7].Value;

            string frontZH = fZHs[0].Value + "," +
                                   fZHs[1].Value + "," +
                                   fZHs[2].Value + "," +
                                   fZHs[3].Value + "," +
                                   fZHs[4].Value + "," +
                                   fZHs[5].Value + "," +
                                   fZHs[6].Value + "," +
                                   fZHs[7].Value;

            string backX = bxs[0].Value + "," +
                                bxs[1].Value + "," +
                                bxs[2].Value + "," +
                                bxs[3].Value + "," +
                                bxs[4].Value + "," +
                                bxs[5].Value + "," +
                                bxs[6].Value + "," +
                                bxs[7].Value;

            string backY = bys[0].Value + "," +
                                bys[1].Value + "," +
                                bys[2].Value + "," +
                                bys[3].Value + "," +
                                bys[4].Value + "," +
                                bys[5].Value + "," +
                                bys[6].Value + "," +
                                bys[7].Value;

            string backTime = bTimes[0].Value + "," +
                                    bTimes[1].Value + "," +
                                    bTimes[2].Value + "," +
                                    bTimes[3].Value + "," +
                                    bTimes[4].Value + "," +
                                    bTimes[5].Value + "," +
                                    bTimes[6].Value + "," +
                                    bTimes[7].Value;

            string backZH =  bZHs[0].Value + "," +
                                   bZHs[1].Value + "," +
                                   bZHs[2].Value + "," +
                                   bZHs[3].Value + "," +
                                   bZHs[4].Value + "," +
                                   bZHs[5].Value + "," +
                                   bZHs[6].Value + "," +
                                   bZHs[7].Value;

            JObject param = new JObject();
            param.Add("frontX", frontX);
            param.Add("frontY", frontY);
            param.Add("frontTime", frontTime);
            param.Add("frontZH", frontZH);
            param.Add("backX", backX);
            param.Add("backY", backY);
            param.Add("backTime", backTime);
            param.Add("backZH", backZH);

            return JsonConvert.SerializeObject(param);

        }

        /// <summary>
        /// 获取病历列表
        /// </summary>
        public void getMRs()
        {
            dgvMR.Rows.Clear();            

            if(client != null){
                mrs = db.getMRsByClientId(client.id);
                for (int i = 0; i < mrs.Count; i++ )
                {
                    dgvMR.Rows.Add();
                    MR mr = mrs[i];
                    DataGridViewRow row = dgvMR.Rows[dgvMR.Rows.Count - 1];
                    row.HeaderCell.Value = mr.id; 
                    row.Cells[0].Value = (i+1);
                    row.Cells[1].Value = mr.date1;
                    row.Cells[2].Value = mr.mr;
                    row.Cells[3].Value = mr.doctor;
 
                }
                
               
            }
        }


        /// <summary>
        /// 弹出编辑病历的对话框（添加）
        /// </summary>
        public void addMR(object sender, EventArgs e)
        {
            if (Application.OpenForms["FormEditMR"] == null)
            {
                FormEditMR form = new FormEditMR();
                form.formMR = this;
                form.client = client;
                form.mr = null;
                form.db = db;
                form.Show();
            }
           
        }

        /// <summary>
        /// 弹出编辑病历的对话框（修改）
        /// </summary>
        public void editMR(object sender, DataGridViewCellEventArgs e)
        {

            DataGridViewRow row = dgvMR.SelectedRows[0];

            string mrId = row.HeaderCell.Value + "";
            MR mr = db.getMR(mrId);

            Console.WriteLine("mr1:" + mr.mr);

            if (Application.OpenForms["FormEditMR"] == null)
            {
                FormEditMR form = new FormEditMR();
                form.formMR = this;
                form.client = client;
                form.mr = mr;
                form.db = db;
                form.Show();
            }

        }

        /// <summary>
        /// 登出（清空当前客户信息)
        /// </summary>
        public void logout(object sender, EventArgs e)
        {
            clearUI();
        }

        /// <summary>
        /// 清空界面信息
        /// </summary>
        public void clearUI()
        {
            //tbxIdCardNo.Text = null;
            tbxName.Text = null;
            tbxAge.Text = null;
            tbxHeight.Text = null;
            tbxWeight.Text = null;
            cbxGender.SelectedIndex = 0;
            lblMsgQueryClient.Text = null;
            lblCRUDStatus.Text = null;
            dgvMR.Rows.Clear();

            int rowIndex = 0;
            foreach (DataGridViewRow row in dgvParamF.Rows)
            {
               if(rowIndex != 4){
                   foreach (DataGridViewCell cell in row.Cells)
                   {
                       cell.Value = "";
                   }
               }
               rowIndex ++;
            }

            rowIndex = 0;
            foreach (DataGridViewRow row in dgvParamB.Rows)
            {
                if (rowIndex != 4)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        cell.Value = "";
                    }
                }
                rowIndex++;
            }

        }

        /// <summary>
        /// 检查内容格式
        /// </summary>
        public bool checkFormat()
        {
            //个人信息

            double x;
            if (tbxName.Text.Trim().Length == 0) { MessageBox.Show("请输入姓名"); return false; }
            if (!Double.TryParse(tbxAge.Text, out x)) { MessageBox.Show("年龄数据有误，请修正"); return false; }
            if (!Double.TryParse(tbxHeight.Text, out x)) { MessageBox.Show("身高数据有误，请修正"); return false; }
            if (!Double.TryParse(tbxWeight.Text, out x)) { MessageBox.Show("体重数据有误，请修正"); return false; }
            //身份证号：15位（数字）或18位（17位数字+第18位数字或X）, 整体匹配的话首尾加\A和\z

            string regex = "";
            if(rbIdCard.Checked){
                regex = @"\A[1-9]\d{14}(\d{2}[0-9X])*\z";
                if (!Regex.IsMatch(tbxIdCardNo.Text.Trim(), regex))
                {
                    MessageBox.Show("身份证号有误，请修正"); return false;
                }
            }

            if (rbPhone.Checked)
            {
                regex = "^1[34578]\\d{9}$";
                if (!Regex.IsMatch(tbxPhoneNo.Text.Trim(), regex))
                {
                    MessageBox.Show("手机号有误，请修正"); return false;
                }
            }


           

            //穴位参数

            int y;

            foreach (DataGridViewCell cell in fxs)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 正面X 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in fys)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 正面Y 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in fTimes)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 正面时间 格式有误，请修正"); return false; }
                //要大于零
                else if (y < 0) { MessageBox.Show("参数 正面时间 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in fZHs)
            {
                if (cell.Value.ToString().Trim().Contains(",")) { MessageBox.Show("参数 正面穴位名称 含有不支持的字符，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in bxs)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 背面X 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in bys)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 背面Y 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in bTimes)
            {
                if (!Int32.TryParse(cell.Value.ToString().Trim(), out y)) { MessageBox.Show("参数 背面时间 格式有误，请修正"); return false; }
                //要大于零
                else if (y < 0) { MessageBox.Show("参数 背面时间 格式有误，请修正"); return false; }
            }

            foreach (DataGridViewCell cell in bZHs)
            {
                if (cell.Value.ToString().Trim().Contains(",")) { MessageBox.Show("参数 背面穴位名称 含有不支持的字符，请修正"); return false; }
            }

            return true;
        }

        /// <summary>
        /// Tab切换
        /// </summary>
        private void changeTab(object sender, EventArgs e)
        {
            switch (tbcParam.SelectedIndex)
            {
                case 0:
                    btnLoadParamDFT.Visible = true;
                    btnAddMR.Visible = false;
                    break;
                case 1:
                     btnLoadParamDFT.Visible = false;
                    btnAddMR.Visible = true;
                    break;
               
            }
        }

        /// <summary>
        /// 收集本次穴位信息
        /// </summary>
        public string getPosition()
        {
            string position = "";
            position += cbxf1.Checked?"1":"0";
            position += cbxf2.Checked ? "1" : "0";
            position += cbxf3.Checked ? "1" : "0";
            position += cbxf4.Checked ? "1" : "0";
            position += cbxf5.Checked ? "1" : "0";
            position += cbxf6.Checked ? "1" : "0";
            position += cbxf7.Checked ? "1" : "0";
            position += cbxf8.Checked ? "1" : "0";
            position += ",";
            position += cbxb1.Checked ? "1" : "0";
            position += cbxb2.Checked ? "1" : "0";
            position += cbxb3.Checked ? "1" : "0";
            position += cbxb4.Checked ? "1" : "0";
            position += cbxb5.Checked ? "1" : "0";
            position += cbxb6.Checked ? "1" : "0";
            position += cbxb7.Checked ? "1" : "0";
            position += cbxb8.Checked ? "1" : "0";

            return position;
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

        //串口通信部分-Start*********************************************************************

        private void beginSend(byte[] order, int orderNo, int expLength)
        {
            fnComm.sendOrder(order);
        }

        public void send(byte[] order, int orderNo, int expLength)
        {
            new Thread(new ThreadStart(() => beginSend(order, orderNo, expLength))).Start();

        }

        //串口通信部分-End*********************************************************************

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void closeForm(object sender, EventArgs e)
        {
            this.Close();
        }



        public void fillData()
        {
            try
            {
              //pictureBox1.ImageLocation = Application.StartupPath + "\\zp.bmp";
                byte[] name = new byte[30];
                int length = 30;
                CVRSDK.GetPeopleName(ref name[0], ref length);
                //MessageBox.Show();
                byte[] number = new byte[30];
                length = 36;
                CVRSDK.GetPeopleIDCode(ref number[0], ref length);
                byte[] people = new byte[30];
                length = 3;
                CVRSDK.GetPeopleNation(ref people[0], ref length);
                byte[] validtermOfStart = new byte[30];
                length = 16;
                CVRSDK.GetStartDate(ref validtermOfStart[0], ref length);
                byte[] birthday = new byte[30];
                length = 16;
                CVRSDK.GetPeopleBirthday(ref birthday[0], ref length);
                byte[] address = new byte[30];
                length = 70;
                CVRSDK.GetPeopleAddress(ref address[0], ref length);
                byte[] validtermOfEnd = new byte[30];
                length = 16;
                CVRSDK.GetEndDate(ref validtermOfEnd[0], ref length);
                byte[] signdate = new byte[30];
                length = 30;
                CVRSDK.GetDepartment(ref signdate[0], ref length);
                byte[] sex = new byte[30];
                length = 3;
                CVRSDK.GetPeopleSex(ref sex[0], ref length);

                byte[] samid = new byte[32];
                CVRSDK.CVR_GetSAMID(ref samid[0]);

                /*
                lblAddress.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(address).Replace("\0", "").Trim();
                lblSex.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(sex).Replace("\0", "").Trim();
                lblBirthday.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(birthday).Replace("\0", "").Trim();
                lblDept.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(signdate).Replace("\0", "").Trim();
                lblIdCard.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(number).Replace("\0", "").Trim();
                lblName.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(name).Replace("\0", "").Trim();
                lblNation.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(people).Replace("\0", "").Trim();
                label11.Text = "安全模块号：" + System.Text.Encoding.GetEncoding("GB2312").GetString(samid).Replace("\0", "").Trim();
                lblValidDate.Text = System.Text.Encoding.GetEncoding("GB2312").GetString(validtermOfStart).Replace("\0", "").Trim() + "-" + System.Text.Encoding.GetEncoding("GB2312").GetString(validtermOfEnd).Replace("\0", "").Trim();
                */

                string address1 = System.Text.Encoding.GetEncoding("GB2312").GetString(address).Replace("\0", "").Trim();
                string sex1 = System.Text.Encoding.GetEncoding("GB2312").GetString(sex).Replace("\0", "").Trim();
                string birthday1 = System.Text.Encoding.GetEncoding("GB2312").GetString(birthday).Replace("\0", "").Trim();
                string signdate1 = System.Text.Encoding.GetEncoding("GB2312").GetString(signdate).Replace("\0", "").Trim();
                string number1 = System.Text.Encoding.GetEncoding("GB2312").GetString(number).Replace("\0", "").Trim();
                string name1 = System.Text.Encoding.GetEncoding("GB2312").GetString(name).Replace("\0", "").Trim();
                string people1 = System.Text.Encoding.GetEncoding("GB2312").GetString(people).Replace("\0", "").Trim();
                string samid1 = System.Text.Encoding.GetEncoding("GB2312").GetString(samid).Replace("\0", "").Trim();
                string validtermOfStart1 = System.Text.Encoding.GetEncoding("GB2312").GetString(validtermOfStart).Replace("\0", "").Trim() + "-" + System.Text.Encoding.GetEncoding("GB2312").GetString(validtermOfEnd).Replace("\0", "").Trim();

                tbxIdCardNo.Text = number1;
                tbxName.Text = name1;

                if (sex1.Equals("男")){ cbxGender.SelectedIndex = 0; }
                if (sex1.Equals("女")){ cbxGender.SelectedIndex = 1; }

                getClientInfoAfterReadCard(null, null);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 更新数据库（主要是结构上）
        /// </summary>
        public void updateDB()
        {
            //【！可以考虑加入数据库版本号，来决定是否要更新当前版本】

            string sql = "";
            /*
            sql = "insert into t_mr (clientId, mr, addTime, date1, doctor) values (12, '帕金森', '2018-02-21 20:19:43', '','王宝强')";
            try { 
                db.execute(sql);
                db.execute(sql);
                db.execute(sql);
                db.execute(sql);
                db.execute(sql); 
            }
            catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }
            */

            //V1  18.2.21 t_client表增加字段：“性别”，同时更新之前未设置过性别字段的记录
            /*
            sql = "alter table t_client add column gender varchar(5);";
            try { db.execute(sql); } catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            sql = "update t_client set gender = 'm' where gender is null;";
            try { db.execute(sql); } catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            sql = "update t_mr set position = '00000000,00000000' where position is null;";
            try { db.execute(sql); }
            catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            sql = "update t_mr set param = '' where param is null;";
            try { db.execute(sql); }
            catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            sql = "update t_mr set addTime = '' where addTime is null;";
            try { db.execute(sql); }
            catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            //V2  18.2.21 t_mr表增加字段：“参数”
            sql = "alter table t_mr add column param varchar(600);";
            try { db.execute(sql); } catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }

            //V3 18.2.22 t_client
            sql = "update t_client set age = 1 where age is null;";
            try { db.execute(sql); }
            catch (Exception ex) { Console.WriteLine("更新数据库出错：" + ex.Message); }
            */

            //V4 18.3.10 t_client
            sql = "alter table t_client add column phoneNo varchar(15);";
            try { 
                db.execute(sql); 
                sql = "update t_client set phoneNo = '' where phoneNo is null";
                db.execute(sql); 
            }
            catch (Exception ex) { 
                Console.WriteLine("更新数据库出错：" + ex.Message);
                try {
                    sql = "update t_client set phoneNo = '' where phoneNo is null";
                    db.execute(sql); 
                }
                catch (Exception ex2)
                {
                    Console.WriteLine("填充数据出错：" + ex2.Message);
                }
            }
        }

       

    }
}
