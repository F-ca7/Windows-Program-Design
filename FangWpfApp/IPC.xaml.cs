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
using System.Windows.Controls;
using System.Windows.Media;

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
        const int MAX_BUFF_SIZE = 12;
        int BUFF_SIZE = MAX_BUFF_SIZE;
        const int COLS = 4;
        const int ROWS = 3;
        int inIdx = 0;
        int outIdx = 0;
        Label[] lblBuffers;
        SolidColorBrush emptyBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));
        SolidColorBrush fullBg = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
        // 初始标签
        const string INIT_CONTENT = "空";
        const string FULL_CONTENT = "有";
        // 总生产数量
        const int TOTAL_PRODUCT = 20;
        // 信号量
        Semaphore empty;
        Semaphore full;
        // 访问生产总量的互斥锁
        Mutex totalCntMutex = new Mutex();
        // 访问放入取出的互斥锁
        Mutex inSlotMutex = new Mutex();
        Mutex outSlotMutex = new Mutex();
        // 已经预定生产的数量
        int producingCnt = 0;    
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
            try
            {
                CreateBufferGrid();
            }catch(Exception)
            {
                MessageBox.Show("缓冲区个数应小于12且为正整数！");
                return;
            }
            int producerCnt, consumerCnt;
            try
            {
                Txb_Sem_Result.AppendText(string.Format("缓冲区大小为{0}, 生产目标个数为{1}\n", BUFF_SIZE, TOTAL_PRODUCT));
                producerCnt = int.Parse(Txb_Producer_Cnt.Text);
                consumerCnt = int.Parse(Txb_Consumer_Cnt.Text);
                empty = new Semaphore(BUFF_SIZE, BUFF_SIZE);
                full = new Semaphore(0, BUFF_SIZE);
                
                for(int i = 0; i < producerCnt; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Produce))
                    {
                        Name = "生产者" + i
                    };
                    thread.Start(thread.Name);
                }
                for (int i = 0; i < consumerCnt; i++)
                {
                    Thread thread = new Thread(new ParameterizedThreadStart(Consume))
                    {
                        Name = "消费者" + i
                    };
                    thread.Start(thread.Name);
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("请输入正整数！");
            }            
        }

        // 动态创建缓冲区格子
        private void CreateBufferGrid()
        {
            try
            {
                BUFF_SIZE = int.Parse(Txb_Buffer.Text);
            }
            catch (Exception e)
            {
                // 异常由外层处理
                throw e;
            }
            if(BUFF_SIZE> MAX_BUFF_SIZE)
            {
                throw new ArgumentException("缓冲数超过最大值");
            }
            lblBuffers = new Label[BUFF_SIZE];
            // 单个格子的大小
            double width = Grid_Buffers.Width / COLS;
            double height = Grid_Buffers.Height / ROWS;
            int row, col;
            for (int i = 0; i < BUFF_SIZE; i++)
            {
                row = i / COLS;
                col = i % COLS;
                Label l = new Label
                {
                    Content = INIT_CONTENT,
                    Width = width,
                    Height = height,
                    Background = emptyBg
                };
                Grid_Buffers.Children.Add(l);
                Grid.SetRow(l, row);
                Grid.SetColumn(l, col);
                lblBuffers[i] = l;
            }
        }

        // 生产者生产
        private void Produce(object obj)
        {
            while (true)
            {
                totalCntMutex.WaitOne();
                if (producingCnt >= TOTAL_PRODUCT)
                {
                    AppendCommonSemResult(string.Format("---------------\n达到预定生产目标{0}。{1}结束\n", TOTAL_PRODUCT, obj.ToString()));
                    totalCntMutex.ReleaseMutex();
                    return;
                }
                // 未生产完，继续
                //AppendSemResult(string.Format("---------------\n目前已经生产{0}个产品\n", producedCnt));              
                producingCnt++;
                totalCntMutex.ReleaseMutex();
                empty.WaitOne();
                // 模拟生产延时
                Thread.Sleep(2000);
                // 放入缓冲区时才获取锁
                inSlotMutex.WaitOne();
                AppendProSemResult(string.Format("---------------\n{0}生产完成\n", obj.ToString()));
                inSlotMutex.ReleaseMutex();
                // 通知生产好了
                full.Release();
            }
        }
        // 消费者消费
        private void Consume(object obj)
        {
            while (true)
            {
                totalCntMutex.WaitOne();
                if (producedCnt >= TOTAL_PRODUCT)
                {
                    AppendCommonSemResult(string.Format("---------------\n达到预定生产目标{0}。{1}结束\n", TOTAL_PRODUCT, obj.ToString()));
                    totalCntMutex.ReleaseMutex();
                    return;
                }
                totalCntMutex.ReleaseMutex();
                full.WaitOne();
                outSlotMutex.WaitOne();
                AppendConSemResult(string.Format("---------------\n{0}开始消费\n", obj.ToString()));
                // 这里先取再耗时消费
                outSlotMutex.ReleaseMutex();
                // 模拟消费延时
                Thread.Sleep(1000);
                AppendCommonSemResult(string.Format("---------------\n{0}消费完成\n", obj.ToString()));
                // 通知有空位
                empty.Release();
            }

        }

        // 异步更新生产者结果
        private void AppendProSemResult(string data)
        {
            Txb_Sem_Result.Dispatcher.BeginInvoke(new Action(() =>
            {
                producedCnt++;
                lblBuffers[inIdx % BUFF_SIZE].Background = fullBg;
                lblBuffers[inIdx % BUFF_SIZE].Content = FULL_CONTENT;
                inIdx++;
                Txb_Sem_Result.AppendText(data + "\n");
                Txb_Sem_Result.ScrollToEnd();
                Lbl_Produced_Cnt.Content = "" + producedCnt;
                
            }));         
        }
        
        
        // 异步更新消费者结果
        private void AppendConSemResult(string data)
        {
            Txb_Sem_Result.Dispatcher.BeginInvoke(new Action(() =>
            {
                lblBuffers[outIdx % BUFF_SIZE].Background = emptyBg;
                lblBuffers[outIdx % BUFF_SIZE].Content = INIT_CONTENT;
                outIdx++;
                Txb_Sem_Result.AppendText(data + "\n");
                Txb_Sem_Result.ScrollToEnd();
                Lbl_Produced_Cnt.Content = "" + producedCnt;               
            }));         
        }



        // 异步更新通用结果
        private void AppendCommonSemResult(string data)
        {
            Txb_Sem_Result.Dispatcher.BeginInvoke(new Action(() =>
            {
                Txb_Sem_Result.AppendText(data + "\n");
                Txb_Sem_Result.ScrollToEnd();
                Lbl_Produced_Cnt.Content = "" + producedCnt;
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
                        // 异步更新UI
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
