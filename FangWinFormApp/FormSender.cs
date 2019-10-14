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

        public delegate void ClearTextHandler();
        // 定义清空文本框事件
        public event ClearTextHandler ClearTextEvent;

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


        // 清空接受者文本框
        private void Btn_Clear_Receiver(object sender, EventArgs e)
        {
            ClearTextEvent();
        }



    }
}
