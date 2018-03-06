using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using AutoZhenJiu.database;
using AutoZhenJiu.entity;

namespace AutoZhenJiu
{
      partial class FormEditMR : Form
    {
        public FormMR formMR;

        public Client client;
        public MR mr;
        public DBsqlite db;

        public FormEditMR()
        {
            InitializeComponent();

            //init(client, mr, db);
           

            btnSave.Click += new EventHandler(save);
            btnDelete.Click += new EventHandler(delete);
            btnClose.Click += new EventHandler(closeForm);
        }

        private void FormEditMR_Load(object sender, EventArgs e)
        {
            //必须在load方法中执行，不然速度太快
            initUI();
        }

        /// <summary>
        /// 设置界面信息
        /// </summary>
        public void initUI()
        {
            if (mr != null)
            {
                dtpDate.Text = mr.date1;
                rtbxMR.Text = mr.mr;
                tbxDoctor.Text = mr.doctor;
            }
           
        }

        /// <summary>
        /// 保存本次病历信息
        /// </summary>
        public void save(object sender, EventArgs e)
        {
            if(!checkFormat()){ return; }
            string msg = "";
            
            if(mr == null){
                mr = new MR();
                mr.addTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            mr.clientId = client.id;
            mr.mr = rtbxMR.Text.Trim();
            mr.date1 = dtpDate.Text;
            mr.doctor = tbxDoctor.Text.Trim();

            if(mr.id < 0){
                if (db.saveMR(mr)) {
                    msg += "病历保存成功";
                    formMR.lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                    formMR.getMRs();
                    this.Close();
                } else {
                    msg += "病历保存失败"; 
                }
            }else{
                if (db.updateMR(mr)) { 
                    msg += "病历更新成功";
                    formMR.lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                    formMR.getMRs();
                    this.Close();
                } else { 
                    msg += "病历更新失败"; 
                }
            }

            lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;

        }

        /// <summary>
        /// 删除本条信息
        /// </summary>
        public void delete(object sender, EventArgs e)
        {
            if(mr != null){
                MessageBoxButtons btn = MessageBoxButtons.OKCancel;
                DialogResult dr = MessageBox.Show("确定要删除吗?", "提示", btn);

                if (dr == DialogResult.OK)//如果点击“确定”按钮
                {
                    string msg = "";
                    if (db.deleteMR(mr))
                    {
                        msg += "病历删除成功";
                        formMR.lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                        formMR.getMRs();
                        this.Close();
                    }
                    else
                    {
                        msg += "病历删除失败";
                    }
                    lblCRUDStatus.Text = DateTime.Now.ToString("HH:mm:ss") + " " + msg;
                 }
                
            }
           
        }

        /// <summary>
        /// 检查内容格式
        /// </summary>
        public bool checkFormat()
        {
            double x;
            if (rtbxMR.Text.Trim().Length == 0) { MessageBox.Show("请输入病历内容"); return false; }
 
            return true;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public void closeForm(object sender, EventArgs e)
        {
            this.Close();
        }

        

      

    }
}
