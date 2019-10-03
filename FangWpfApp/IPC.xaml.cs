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
        // 缓冲区大小
        const int BUFF_SIZE = 6;
        // 总生产数量
        const int TOTAL_PRODUCT = 20;
        // 信号量
        Semaphore empty;
        Semaphore full;
        Mutex mutex = new Mutex();
        // 已经生产的数量
        int producedCnt = 0;    

        // 隐藏所有面板
        private void HideAllIPCGrids()
        {
            Grid_Redirect.Visibility = Visibility.Hidden;
            Grid_Pipe.Visibility = Visibility.Hidden;
            Grid_Sem.Visibility = Visibility.Hidden;
            pipeClient = null;
            pipeServer = null;
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
            if (sr != null)
            {
                sr.Close();
                sr = null;
            }
        }

        #region 信号量同步
        // 显示信号量同步的演示面板
        private void Btn_Show_Sem(object sender, RoutedEventArgs e)
        {
            HideAllIPCGrids();
            Grid_Sem.Visibility = Visibility.Visible;
        }

        // 开始模拟生产者消费者同步
        private void Btn_Start_Sem_Click(object sender, RoutedEventArgs e)
        {
            int producerCnt, consumerCnt;
            try
            {
                Txb_Sem_Result.AppendText(string.Format("缓冲区大小为{0}, 总生产个数为{1}\n", BUFF_SIZE, TOTAL_PRODUCT));
                producerCnt = int.Parse(Txb_Producer_Cnt.Text);
                consumerCnt = int.Parse(Txb_Consumer_Cnt.Text);
                empty = new Semaphore(BUFF_SIZE, BUFF_SIZE);
                full = new Semaphore(0, BUFF_SIZE);
                
                for(int i = 0; i < producerCnt; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Produce));
                    thread.Name = "生产者" + i;
                    thread.Start(thread.Name);
                }
                for (int i = 0; i < consumerCnt; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Consume));
                    thread.Name = "消费者" + i;
                    thread.Start(thread.Name);
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("请输入正整数！");
            }
        }
        // 生产者生产
        private void Produce(object obj)
        {
            while (true)
            {
                mutex.WaitOne();
                if (producedCnt >= TOTAL_PRODUCT)
                {
                    AppendSemResult(string.Format("---------------\n达到生产目标{0}。{1}结束\n", TOTAL_PRODUCT, obj.ToString()));
                    mutex.ReleaseMutex();
                    return;
                }
                // 未生产完，继续
                AppendSemResult(string.Format("---------------\n目前已经生产{0}个产品\n", producedCnt));
                producedCnt++;
                mutex.ReleaseMutex();
                empty.WaitOne();
                Thread.Sleep(1000);
                AppendSemResult(string.Format("---------------\n{0}生产完成\n", obj.ToString()));
                full.Release();
            }
        }
        // 消费者消费
        private void Consume(object obj)
        {
            while (true)
            {
                mutex.WaitOne();
                if (producedCnt >= TOTAL_PRODUCT)
                {
                    AppendSemResult(string.Format("---------------\n达到生产目标{0}。{1}结束\n", TOTAL_PRODUCT, obj.ToString()));
                    mutex.ReleaseMutex();
                    return;
                }
                mutex.ReleaseMutex();
                full.WaitOne();
                Thread.Sleep(1000);
                AppendSemResult(string.Format("---------------\n{0}消费完成\n", obj.ToString()));
                empty.Release();
            }

        }

        // 异步更新结果
        private void AppendSemResult(string data)
        {
            Txb_Sem_Result.Dispatcher.BeginInvoke(new Action(() =>
            {
                Txb_Sem_Result.AppendText(data + "\n");
                Txb_Sem_Result.ScrollToEnd();
            }));         
        }

        #endregion

        #region 命名管道

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
                    catch (Exception)
                    {
                        return;
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
        #endregion

        #region 重定向

        // 显示重定向的演示面板
        private void Btn_Show_Redirect(object sender, RoutedEventArgs e)
        {
            HideAllIPCGrids();
            Grid_Redirect.Visibility = Visibility.Visible;
        }

        // tracert命令
        private void Btn_Tracert_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Txb_Trace_Target.Text))
            {
                MessageBox.Show("请输入目标地址", "提示");
                return;
            }
            string strCmd = "tracert -h 5 " + Txb_Trace_Target.Text;
            RedirectCMD(strCmd);          
        }

        // getmac命令
        private void Btn_Getmac_Click(object sender, RoutedEventArgs e)
        {
            string strCmd = "getmac";
            RedirectCMD(strCmd);
        }

        // shutdown命令
        private void Btn_Shutdown_Click(object sender, RoutedEventArgs e)
        {
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
        #endregion
    }
}
