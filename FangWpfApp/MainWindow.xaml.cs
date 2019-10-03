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
using DMSkin.WPF;

namespace FangWpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        const int ARR_SIZE = 6;
        int[] arr;

        [DllImport((@"../../../Release/DllCppSort.dll"), EntryPoint = "MergeSort")]
        public static extern void MergeSort(int[] arr, int size);

        public MainWindow()
        {
            InitializeComponent();
            GenerateRandomArray();
        }

        // 生成随机数组
        private void GenerateRandomArray()
        {
            arr = new int[ARR_SIZE];
            Random random = new Random();
            for(int i = 0; i < ARR_SIZE; i++)
            {
                arr[i] = random.Next(100);
            }
            OriginArrayTxt.Text = ArrayToStr(arr);
        }

        private void Btn_GenerateArr_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomArray();
        }


        // 调用C# Dll
        private void Btn_Sort_Click(object sender, RoutedEventArgs e)
        {
            Sort.BubbleSort(arr);
            PrintArr(arr);
            ShowArrayResult();
        }

        // 调用C++ Dll
        private void Btn_Sort2_Click(object sender, RoutedEventArgs e)
        {
            MergeSort(arr, arr.Length);
            PrintArr(arr);
            ShowArrayResult();
        }

        // 数组格式化字符串
        private String ArrayToStr<T>(T[] arr)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < arr.Length - 1; i++)
            {
                sb.Append(arr[i]);
                sb.Append(", ");
            }
            if (arr.Length > 0)
            {
                sb.Append(arr[arr.Length - 1]).Append("]");
            }
            else
            {
                sb.Append("]");
            }
            return sb.ToString();
        }

        // 显示结果
        private void ShowArrayResult()
        {
            ResultArrayTxt.Text = ArrayToStr(arr);
        }


        private void PrintArr(int[] arr)
        {
            foreach(int i in arr)
            {
                Console.Write("{0} ", i);
            }
            Console.WriteLine();
        }

    }
}
