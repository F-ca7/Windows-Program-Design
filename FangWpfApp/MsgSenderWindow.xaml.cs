﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static FangWinFormApp.WinAPI;

namespace FangWpfApp
{
    /// <summary>
    /// MsgSenderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MsgSenderWindow : Window
    {
        // 用户文本消息
        const int WM_COPYDATA = 0x004A;
        public IntPtr hWnd;

        [DllImport("user32.dll")]
        public static extern void SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            ref COPYDATASTRUCT lParam);


        public MsgSenderWindow()
        {
            InitializeComponent();
        }

        // 发送消息
        private void Btn_Send_Msg_click(object sender, RoutedEventArgs e)
        {
            string text = Txb_Send_Msg.Text;
            if (hWnd!=null)
            {
                COPYDATASTRUCT cds;
                cds.lpData = text;
                cds.dwData = (IntPtr)100;
                byte[] arr = Encoding.UTF8.GetBytes(text);
                Console.WriteLine("向进程{1}发送{0}\n", text, hWnd.ToInt32());
                cds.cbData = arr.Length + 1;
                // 同步发送消息
                // 异步发送存在字符串指针空间回收问题
                SendMessage(hWnd, WM_COPYDATA, IntPtr.Zero, ref cds);
            }
            Txb_Send_Msg.Text = "";
        }

        // 通过进程名找到窗口句柄
        private void FindWndHandlerByName(string name)
        {

            Process[] procs = Process.GetProcesses();
            foreach (Process p in procs)
            {
                if (p.ProcessName.Equals(name))
                {
                    // 获取目标进程句柄
                    hWnd = p.MainWindowHandle;
                }
            }
        }
    }
}
