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
    public partial class FormSender : Form
    {
        public IntPtr hwndTest;
        public int IwndTest;
        public IntPtr hwndfrmTest;
        // 用户文本消息
        public const int USER_TEXT_MSG = 0x0400;


        [DllImport("user32.dll")]
        public static extern void SendMessage(
            IntPtr hWnd, 
            int msg, 
            int wParam, 
            ref COPYDATASTRUCT lParam);


        public FormSender()
        {
            InitializeComponent();
        }

        private void FormSender_Shown(object sender, EventArgs e)
        {

        }

        // 发送消息
        private void Btn_Send_Msg_Click(object sender, EventArgs e)
        {
            string text = Txb_Sender.Text;
            COPYDATASTRUCT cds;
            cds.lpData = text;
            cds.dwData = (IntPtr)100;
            byte[] arr = Encoding.UTF8.GetBytes(text);
            Console.WriteLine("{0}\n{1}", text, arr.Length);
            cds.cbData = arr.Length + 1;
            // 同步发送消息
            // 异步发送存在字符串指针空间回收问题
            SendMessage(hwndTest, USER_TEXT_MSG, 0, ref cds);
            Txb_Sender.Text = "";
        }

        private void FormSender_Load(object sender, EventArgs e)
        {

        }
    }
}
