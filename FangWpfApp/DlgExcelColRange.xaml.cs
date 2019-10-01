using System;
using System.Collections.Generic;
using System.Linq;
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

namespace FangWpfApp
{
    /// <summary>
    /// DlgExcelColRange.xaml 的交互逻辑
    /// </summary>
    public partial class DlgExcelColRange : Window
    {
        public delegate void SendMessage(int startCol, int endCol);
        public SendMessage sendMessage;

        public DlgExcelColRange()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            int start, end;
            try
            {
                start = int.Parse(Txb_Col_Start.Text);
                end = int.Parse(Txb_Col_End.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正整数");
                return;
            }
            if (end < start)
            {
                MessageBox.Show("起始应小于等于结束");
                return;
            }
            sendMessage(start, end);
            Close();
        }
    }
}
