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
    public partial class FormPassword : Form
    {

        Config2 config2;
        FormConfig formConfig;
        public int source;

        public FormPassword(FormConfig formConfig, int source)
        {
            InitializeComponent();

            this.formConfig = formConfig;
            this.source = source;
            getConfig2();

            btnOK.Click += new EventHandler(checkPwd);
        }

        private void FormPassword_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 验证密码
        /// </summary>
        public void checkPwd(Object sender, EventArgs e)
        {
            if (tbxPwd.Text.Trim().Length == 0)
            {
                lblMsg.Text = "请输入密码";
                return;
            }
            else
            {
                string md5 = new FnString().toMD5(tbxPwd.Text.Trim());

                string expPasword = "";

                switch(source){
                    case FormConfig.SOURCE_1: 
                        expPasword = config2.password;
                        if (md5.Equals(expPasword))
                        {
                            formConfig.accessOK();
                            this.Close();
                        }
                        else
                        {
                            lblMsg.Text = "密码错误";
                        }

                        break;
                    case FormConfig.SOURCE_2: 
                        expPasword = config2.password2;
                        if (md5.Equals(expPasword))
                        {
                            formConfig.accessOK();
                            this.Close();
                        }
                        else
                        {
                            lblMsg.Text = "密码错误";
                        }

                        break;
                }

              
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

    }
}
