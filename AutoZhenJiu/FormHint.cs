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
    public partial class FormHint : Form
    {
        public FormHint()
        {
            InitializeComponent();

            this.MinimizeBox = false;
            lblMsg.Click += new EventHandler(hide);
            
        }

        /// <summary>
        /// 关闭
        /// </summary>
        private void hide(object sender, EventArgs e)
        {
            this.Hide();
        }

      
    }
}
