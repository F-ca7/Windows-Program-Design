using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FangWpfApp
{
    // 通信演示
    public partial class MainWindow
    {
        // 管道命名
        const string PIPE_NAME = "CSPipeFang";

        // 命名管道
        NamedPipeClientStream pipeClient = null;
        NamedPipeServerStream pipeServer = null;
        StreamWriter sw = null;
        StreamReader sr = null;

        // 显示重定向的演示面板
        private void Btn_Show_Redirect(object sender, RoutedEventArgs e)
        {
            HideAllIPCGrids();
            Grid_Redirect.Visibility = Visibility.Visible;
        }

        // 显示管道的演示面板
        private void Btn_Show_Pipe(object sender, RoutedEventArgs e)
        {
            HideAllIPCGrids();
            Grid_Pipe.Visibility = Visibility.Visible;
            // 初始化管道
            pipeServer = new NamedPipeServerStream(PIPE_NAME, PipeDirection.InOut, 1, 
                PipeTransmissionMode.Message, PipeOptions.Asynchronous);
            pipeClient = new NamedPipeClientStream("localhost", PIPE_NAME, PipeDirection.InOut, 
                PipeOptions.Asynchronous, TokenImpersonationLevel.None);
            ConnectPipe();
            WaitForMessage();
        }

        // Server开启线程等待接收消息
        private void WaitForMessage()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                pipeServer.WaitForConnection();
                while (true)
                {
                    try
                    {
                        string line = sr.ReadLine();
                        // 异步更新节目
                        Txb_Server.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            Txb_Server.AppendText(DateTime.Now.ToUniversalTime().ToString() + "\n");
                            Txb_Server.AppendText(line + "\n");
                        }));
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("{0}", ex);
                    }
                }
            });
        }

        // 连接管道
        private void ConnectPipe()
        {
            try
            {
                pipeClient.Connect(5000);
                sw = new StreamWriter(pipeClient);
                sr = new StreamReader(pipeServer);
                sw.AutoFlush = true;
                MessageBox.Show("连接成功！", "Success");
            }
            catch (Exception)
            {
                MessageBox.Show("连接超时。", "Error");
            }
        }

        // 发送消息到客户端
        private void Btn_Send_Msg_Click(object sender, RoutedEventArgs e)
        {
            if (sw != null)
            {
                sw.WriteLine(this.Txb_Client.Text);
                Txb_Client.Text = "";
            }
            else
            {
                MessageBox.Show("未建立连接。");
            }
        }

        private void HideAllIPCGrids()
        {
            Grid_Redirect.Visibility = Visibility.Hidden;
            Grid_Pipe.Visibility = Visibility.Hidden;
            pipeClient = null;
            pipeServer = null;
            if (sw != null)
            {
                sw.Close();
            }
        }

        private void Btn_Tracert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Txb_Trace_Target.Text))
            {
                MessageBox.Show("请输入目标地址", "提示");
                return;
            }
            // tracert命令
            string strCmd = "tracert -h 5 " + Txb_Trace_Target.Text;
            RedirectCMD(strCmd);          
        }


        private void Btn_Getmac_Click(object sender, RoutedEventArgs e)
        {
            // getmac命令
            string strCmd = "getmac";
            RedirectCMD(strCmd);
        }

        private void Btn_Shutdown_Click(object sender, RoutedEventArgs e)
        {
            // shutdown命令
            string strCmd = "shutdown";
            RedirectCMD(strCmd);
        }


        // 调用CMD命令并重定向
        private void RedirectCMD(string command)
        {
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
            try
            {
                process.Start();
                process.StandardInput.WriteLine(command);
                process.StandardInput.WriteLine("exit");
                //  Console.WriteLine("开始执行");
                process.OutputDataReceived += (s, _e) => AppendResult(_e.Data);
                // 退出时的回调函数，恢复按钮
                process.Exited += (s, _e) => Btn_Start_Tracert.Dispatcher.BeginInvoke(new Action(() => Btn_Start_Tracert.IsEnabled = true));
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
