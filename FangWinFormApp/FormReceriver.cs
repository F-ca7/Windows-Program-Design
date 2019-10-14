using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FangWinFormApp.WinAPI;

namespace FangWinFormApp
{
    public partial class FormReceriver : Form
    {

        public FormReceriver()
        {
            InitializeComponent();
        }



        // 打开发送子窗口
        private void Btn_Open_Sender(object sender, EventArgs e)
        {
            FormSender formSender = new FormSender();
            formSender.ClearTextEvent += ClearTxb;
            formSender.hwndTest = this.Handle;
            formSender.Show();
        }

        // 清空文本框
        private void ClearTxb()
        {
            Txb_Receiver.Text = "";
        }


        // 重载消息处理
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case USER_TEXT_MSG:
                    // 用户文本消息
                    COPYDATASTRUCT mystr = new COPYDATASTRUCT();
                    Type mytype = mystr.GetType();
                    mystr = (COPYDATASTRUCT)m.GetLParam(mytype);
                    string str = mystr.lpData;
                    Txb_Receiver.AppendText(str + "\n");

                    break;
                default:
                    // 系统处理其它消息
                    base.DefWndProc(ref m);
                    break;
            }
        }

        private void FormReceriver_Load(object sender, EventArgs e)
        {

        }
    }
}
