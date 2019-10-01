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
        // 用户文本消息
        public const int USER_TEXT_MSG = 0x0400;

        public FormReceriver()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormSender formSender = new FormSender();
            formSender.hwndTest = this.Handle;
            formSender.Show();
        }



        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case USER_TEXT_MSG:
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

        public static string GetUft8(string unicodeString)
        {
            UTF8Encoding utf8 = new UTF8Encoding();
            Byte[] encodedBytes = utf8.GetBytes(unicodeString);
            String decodedString = utf8.GetString(encodedBytes);
            return decodedString;
        }
    }
}
