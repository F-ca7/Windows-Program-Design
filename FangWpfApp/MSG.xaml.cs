using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using static FangWinFormApp.WinAPI;

namespace FangWpfApp
{
    // wpf消息传递演示
    public partial class MainWindow
    {
        const int WM_COPYDATA = 0x004A;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                IntPtr handle = hwndSource.Handle;
                hwndSource.AddHook(new HwndSourceHook(WndProc));
            }
        }

        // 打开发送窗口
        private void Btn_Open_Sender(object sender, RoutedEventArgs e)
        {
            MsgSenderWindow msgSender = new MsgSenderWindow();
            msgSender.hWnd = new WindowInteropHelper(this).Handle;
            msgSender.Show();
            // Console.WriteLine("当前进程 {0}", new WindowInteropHelper(this).Handle.ToInt32());
        }

        // 处理消息
        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_COPYDATA:
                    COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(COPYDATASTRUCT));
                    string str = cds.lpData;
                    Txb_Receiver.AppendText(str + "\n");
                    handled = true;
                    break;
            }
            return hwnd;
        }

        // 清空接受到的消息
        private void Btn_Clear_Received_Msg(object sender, RoutedEventArgs e)
        {
            Txb_Receiver.Text = "";
        }
    }
}
