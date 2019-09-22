using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FangWpfApp
{
    public partial class MainWindow
    {

        private void Btn_Tracert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Txb_Trace_Target.Text))
            {
                MessageBox.Show("请输入目标地址", "提示");
                return;
            }
            Process process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            // 是否使用外壳程序   
            process.StartInfo.UseShellExecute = false;
            // 是否在新窗口中启动该进程的值   
            process.StartInfo.CreateNoWindow = true;
            // 重定向输入输出流  
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            // 禁用按钮
            Btn_Start_Tracert.IsEnabled = false;
            // tracert命令
            string strCmd = "tracert -h 5 "+Txb_Trace_Target.Text;
            try
            {
                process.Start();
                process.StandardInput.WriteLine(strCmd);
                process.StandardInput.WriteLine("exit");
                Console.WriteLine("开始执行");
                process.OutputDataReceived += (s, _e) => AppendResult(_e.Data);
                // 退出时的回调函数，恢复按钮
                process.Exited += (s, _e) => Btn_Start_Tracert.Dispatcher.BeginInvoke(new Action(() => Btn_Start_Tracert.IsEnabled=true));
                process.EnableRaisingEvents = true;
                process.BeginOutputReadLine();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // 异步更新结果
        private void AppendResult(string data)
        {
             Txb_Trace_Result.Dispatcher.BeginInvoke(new Action(() => Txb_Trace_Result.AppendText(data+"\n")));
        }

    }
}
