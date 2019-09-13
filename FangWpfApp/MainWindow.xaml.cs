using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DllSharpSort;
namespace FangWpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        int[] arr = { 3, 2, 1, 7, 8, 9 };

        [DllImport((@"../../../Release/DllCppSort.dll"), EntryPoint = "MergeSort")]
        public static extern void MergeSort(int[] arr, int size);

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Btn_Sort_Click(object sender, RoutedEventArgs e)
        {
            Sort.BubbleSort(arr);
            PrintArr(arr);
        }

        private void PrintArr(int[] arr)
        {
            foreach(int i in arr)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();
        }


        private void Btn_Sort2_Click(object sender, RoutedEventArgs e)
        {
            MergeSort(arr, arr.Length);
            PrintArr(arr);

        }
    }
}
