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
using Microsoft.Win32;

namespace FangWpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        // 数组大小
        const int ARR_SIZE = 6;
        int[] arr;

        const string APP_NAME = "FangWpfApp";

        [DllImport((@"../../../Release/DllCppSort.dll"), EntryPoint = "MergeSort")]
        public static extern void MergeSort(int[] arr, int size);

        // dll创建注册表项
        [DllImport((@"advapi32.dll"))]
        private static extern int RegCreateKeyEx(
            uint hKey,          
            string lpSubKey,    
            uint Reserved,      
            string lpClass,    
            uint dwOptions,     
            uint samDesired,       
            uint lpSecurityAttributes,  
            ref uint phkResult,    
            ref uint lpdwDisposition     
        );

        // 注册表项赋值
        [DllImport((@"advapi32.dll"))]
        private static extern int RegSetValueEx(
          uint hKey,           
          string lpValueName,   
          uint Reserved,
          uint dwType,         
          [MarshalAs(UnmanagedType.LPStr)]      
          string lpData,     
          uint cbData           
        );

        public MainWindow()
        {
            InitializeComponent();
            GenerateRandomArray();
        }



        private void Btn_MyDLL_Click(object sender, RoutedEventArgs e)
        {
            HideAllDLLGrids();
            Grid_MyDLL.Visibility = Visibility.Visible;
        }


        private void Btn_RegDLL_Click(object sender, RoutedEventArgs e)
        {
            HideAllDLLGrids();
            Grid_RegDLL.Visibility = Visibility.Visible;
        }

        private void HideAllDLLGrids()
        {
            Grid_MyDLL.Visibility = Visibility.Hidden;
            Grid_RegDLL.Visibility = Visibility.Hidden;
        }

        # region 自定义DLL
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


        #endregion


        # region DLL操作注册表
        private void Btn_Create_Reg_Click(object sender, RoutedEventArgs e)
        {
            uint result = 0;
            uint ret = 0;
            RegCreateKeyEx(RegConst.HKEY_CURRENT_USER, APP_NAME, 
                0, "REG_SZ", RegConst.REG_OPTION_VOLATILE,
                RegConst.KEY_ALL_ACCESS, 0, ref result, ref ret);
            if (result == 0)
            {
                MessageBox.Show("创建注册表项失败");
            }
            else
            {
                MessageBox.Show("创建注册表项成功");
            }
            string val = Txb_Reg_Value.Text;
            byte[] arr = Encoding.UTF8.GetBytes(val);
            int flag = RegSetValueEx(RegConst.HKEY_CURRENT_USER, APP_NAME,
                0, (uint)RegistryValueKind.String, val, (uint)(arr.Length) + 1);
            if (flag != 0)
            {
                MessageBox.Show("写入注册表项失败");
            }
            else
            {
                MessageBox.Show("写入注册表项成功");
            }
            Txb_Reg_Value.Text = "";
        }



        #endregion

    }
}
