using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace FangWpfApp
{



    public class AlertEvent
    {
        // 警报事件回调
        public delegate void AlertEventHandler(object s, EventArgs e);
        // 事件声明
        public event AlertEventHandler Alert;
        public event AlertEventHandler StopAlert;

        public virtual void OnStartTriggered(EventArgs e)
        {
            //触发开始事件
            Alert?.Invoke(this, e);
        }
        public virtual void OnStopTriggered(EventArgs e)
        {
            //触发结束事件
            StopAlert?.Invoke(this, e);
        }


        public override string ToString()
        {
            return "警报事件";
        }
    }

    // 事件定义触发处理
    public partial class MainWindow
    {
        AlertEvent alertEvent = new AlertEvent();
        Timer alertTimer = new Timer();

        // 添加事件到事件队列
        private void DMSkinWindow_Loaded(object sender, RoutedEventArgs e)
        {
            alertTimer.Enabled = false;
            alertTimer.Interval = 1000;

            alertTimer.Elapsed += new ElapsedEventHandler(ShowAlert);

            alertEvent.Alert += new AlertEvent.AlertEventHandler(StartAlert);
            alertEvent.StopAlert += new AlertEvent.AlertEventHandler(StopAlert);

        }

        // 开启警报
        private void StartAlert(object s, EventArgs e)
        {
            Btn_Trigger_Alert.IsEnabled = false;
            Btn_Stop_Alert.IsEnabled = true;
            AppendEvtTime();
            Txb_Evt.AppendText("触发了警报！\n\n");
            alertTimer.Start();
        }
        // 关闭警报
        private void StopAlert(object s, EventArgs e)
        {
            Btn_Trigger_Alert.IsEnabled = true;
            Btn_Stop_Alert.IsEnabled = false;
            AppendEvtTime();
            Txb_Evt.AppendText("警报结束。\n\n");
            alertTimer.Stop();
        }

        private void Btn_Trigger_Alarm_Click(object sender, RoutedEventArgs e)
        {
            alertEvent.OnStartTriggered(e);
        }


        private void Btn_Stop_Alarm_Click(object sender, RoutedEventArgs e)
        {
            alertEvent.OnStopTriggered(e);
        }

        private void ShowAlert(object sender, ElapsedEventArgs e)
        {
            MessageBox.Show("警报！警报！请立即关闭");
        }

        // 向事件提示文本框附加时间
        private void AppendEvtTime()
        {
            Txb_Evt.AppendText(DateTime.Now.ToUniversalTime().ToString() + "\n");
        }



    }
}
