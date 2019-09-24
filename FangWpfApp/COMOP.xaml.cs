using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MsWord = Microsoft.Office.Interop.Word;
using MyCOM;

namespace FangWpfApp
{
    // COM组件操作演示
    public partial class MainWindow
    {
        // 显示自定义COM的演示面板
        private void Btn_MyCOM_Click(object sender, RoutedEventArgs e)
        {
            HideAllCOMGrids();
            Grid_MyCOM.Visibility = Visibility.Visible;
        }

        // 显示Word的演示面板
        private void Btn_Show_Word(object sender, RoutedEventArgs e)
        {
            HideAllCOMGrids();
            Grid_Word.Visibility = Visibility.Visible;
        }

        // 显示Excel的演示面板
        private void Btn_Show_Excel(object sender, RoutedEventArgs e)
        {
            HideAllCOMGrids();
            Grid_Excel.Visibility = Visibility.Visible;
        }

        // 隐藏所有面板
        private void HideAllCOMGrids()
        {
            Grid_Word.Visibility = Visibility.Hidden;
            Grid_Excel.Visibility = Visibility.Hidden;
            Grid_MyCOM.Visibility = Visibility.Hidden;
        }

        // 计算统计量
        private void Btn_Calc_Stats_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Txb_Arr_Input.Text))
            {
                MessageBox.Show("请输入数组", "Alert");
                return;
            }
            string[] elements = Txb_Arr_Input.Text.Split(' ');
            double[] arr = new double[elements.Length];
            for(int i = 0; i < elements.Length; i++)
            {
                try
                {
                    arr[i] = double.Parse(elements[i]);
                }
                catch (FormatException)
                {
                    MessageBox.Show("请输入正确格式的浮点数", "Error");
                    return;
                }
            }
            MyStatisticsCOM myStatistics = new MyStatisticsCOM();
            double mean = myStatistics.GetMean(arr);
            double var = myStatistics.GetVar(arr);
            Lbl_Mean_Result.Content = mean.ToString("0.##");
            Lbl_Var_Result.Content = var.ToString("0.##");

        }

        private void Btn_Word_COM_Click(object sender, RoutedEventArgs e)
        {   
            string fileName = Directory.GetCurrentDirectory() + @"\my_word.doc";
            if (File.Exists(fileName))
            {
                // 删除旧文档
                File.Delete(fileName);
            }
            MsWord.Application wordApp = new MsWord.Application();
            //是否显示word程序界面
            wordApp.Visible = false;

            object missing = System.Reflection.Missing.Value;

            MsWord.Document doc = wordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            doc.Activate();

            int curSectionNum = 1;
            MsWord.Range curRange;

            wordApp.Options.Overtype = false;   // 改写模式
            MsWord.Selection curSelection = wordApp.Selection;

            object titleStyle = MsWord.WdBuiltinStyle.wdStyleHeading1;
            doc.Sections[curSectionNum].Range.Select();
            Console.WriteLine("正在插入word标题");
            curSelection.TypeText(Txb_Title.Text);
            curSelection.TypeParagraph();

            Console.WriteLine("正在插入word内容");
            curSelection.TypeText(Txb_Content.Text);
            curSelection.TypeParagraph();

            # region 设置格式
            // 标题格式
            curRange = doc.Sections[curSectionNum].Range.Paragraphs[1].Range;
            curRange.set_Style(ref titleStyle);
            doc.Sections[curSectionNum].Range.Paragraphs[1].Alignment = MsWord.WdParagraphAlignment.wdAlignParagraphCenter;
            // 正文格式
            curRange = doc.Sections[curSectionNum].Range.Paragraphs[2].Range;
            curRange.Select();
            curRange.Font.Name = "宋体";
            curRange.Font.Size = 14;
            # endregion 设置格式

            //doc.Fields[1].Update();
            // 保存文档
            object saveName = fileName;
            doc.SaveAs2(ref saveName);
            doc.Close();
            // 释放COM资源
            System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
            doc = null;
            wordApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            wordApp = null;

        }



    }
}
