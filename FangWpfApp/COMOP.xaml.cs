using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using MsWord = Microsoft.Office.Interop.Word;
using MsExcel= Microsoft.Office.Interop.Excel;
using MyCOM;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Core;
using System.Threading;
using Microsoft.Win32;
using System.Reflection;
using Microsoft.Office.Interop.Excel;

namespace FangWpfApp
{
    // 参考文献
    class Reference : INotifyPropertyChanged
    {
        private string _name;

        public Reference(string name)
        {
            Name = name;
        }


        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
    // word格式的文章
    class WordArticle
    {
        private string title;   // 标题
        private string content; // 正文
        private string header;  // 页眉
        private List<Reference> refList;  // 参考文献

        public string Title { get => title; set => title = value; }
        public string Content { get => content; set => content = value; }
        public string Header { get => header; set => header = value; }
        public List<Reference> RefList { get => refList; set => refList = value; }

        public WordArticle(string title, string content, string header, List<Reference> refList)
        {
            this.title = title;
            this.content = content;
            this.header = header;
            this.refList = refList;
        }

        // 保存到word文件
        public void SaveToFile(string fileName)
        {
            MsWord.Application wordApp = new MsWord.Application();
            //是否显示word程序界面
            wordApp.Visible = false;

            object missing = System.Reflection.Missing.Value;

            Document doc = wordApp.Documents.Add(ref missing, ref missing, ref missing, ref missing);
            doc.Activate();

            int curSectionNum = 1;
            MsWord.Range curRange;

            wordApp.Options.Overtype = false;   // 改写模式
            Selection curSelection = wordApp.Selection;


            Console.WriteLine("正在插入页眉", header);
            if (wordApp.ActiveWindow.ActivePane.View.Type == WdViewType.wdNormalView ||
                wordApp.ActiveWindow.ActivePane.View.Type == WdViewType.wdOutlineView)
            {
                wordApp.ActiveWindow.ActivePane.View.Type = WdViewType.wdPrintView;
            }
            wordApp.ActiveWindow.View.SeekView = WdSeekView.wdSeekCurrentPageHeader;
            wordApp.Selection.HeaderFooter.LinkToPrevious = false;
            wordApp.Selection.HeaderFooter.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            wordApp.Selection.HeaderFooter.Range.Text = header;



            object titleStyle = WdBuiltinStyle.wdStyleHeading1;
            doc.Sections[curSectionNum].Range.Select();
            Console.WriteLine("正在插入章节标题 {0}", title);
            curSelection.TypeText(title);
            curSelection.TypeParagraph();           

            Console.WriteLine("正在插入章节内容 {0}", content);
            curSelection.TypeText(content);
            curSelection.TypeParagraph();

            Console.WriteLine("正在插入艺术字 ");
            // 移动光标到左上
            float leftPosition = (float)wordApp.Selection.Information[
                WdInformation.wdHorizontalPositionRelativeToPage];
            float topPosition = (float)wordApp.Selection.Information[
                WdInformation.wdVerticalPositionRelativeToPage];
            wordApp.ActiveDocument.Shapes.AddTextEffect(
                MsoPresetTextEffect.msoTextEffect29, "By Fang",
                "Arial Black", 20, MsoTriState.msoFalse,
                MsoTriState.msoFalse, 0, 0);

            Console.WriteLine("正在插入参考文献 ");
            curSelection.TypeText("参考文献");
            curSelection.TypeParagraph();
            for (int i = 0; i < refList.Count; i++)
            {
                curSelection.TypeText(string.Format("[{0}] {1}", i, refList[i].Name));
                curSelection.TypeParagraph();
            }

            # region 设置格式
            // 标题格式
            curRange = doc.Sections[curSectionNum].Range.Paragraphs[1].Range;
            curRange.Font.Name = "黑体";
            curRange.Font.Size = 18;        // 小二
            doc.Sections[curSectionNum].Range.Paragraphs[1].Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
            // 正文格式
            curRange = doc.Sections[curSectionNum].Range.Paragraphs[2].Range;
            curRange.Select();
            curRange.Font.Name = "宋体";
            curRange.Font.Size = 12;        // 小四
            // 参考文献格式
            curRange = doc.Sections[curSectionNum].Range.Paragraphs[3].Range;
            curRange.Select();
            curRange.Font.Name = "黑体";
            curRange.Font.Size = 18;        // 小二
            doc.Sections[curSectionNum].Range.Paragraphs[3].Alignment =WdParagraphAlignment.wdAlignParagraphCenter;
            if (refList.Count != 0)
            {
                for (int i = 4; i < doc.Sections[curSectionNum].Range.Paragraphs.Count; i++)
                {
                    curRange = doc.Sections[curSectionNum].Range.Paragraphs[i].Range;
                    curRange.Select();
                    curRange.Font.Name = "宋体";
                    curRange.Font.Size = 12;        // 小四
                }
            }
            # endregion 设置格式

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

    // COM组件操作演示
    public partial class MainWindow
    {
        MsExcel.Application app;
        MsExcel.Workbook wb;
        // excel路径
        string excelPath;
        // excel数据表
        System.Data.DataTable excelDt;

        // 参考文献列表
        private ObservableCollection<Reference> referenceList = new ObservableCollection<Reference>();

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
            // 绑定数据源
            Lb_ReferenceList.ItemsSource = referenceList;
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

        # region 自定义COM
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
        #endregion

        #region wordCOM
        private async void Btn_Word_COM_Click(object sender, RoutedEventArgs e)
        {   
            string filePath = Directory.GetCurrentDirectory() + @"\my_word_article.doc";
            if (File.Exists(filePath))
            {
                try
                {
                    // 删除旧文档
                    File.Delete(filePath);
                }
                catch (IOException)
                {
                    MessageBox.Show("请检查文档是否占用！");
                    return;
                }

            }

            string title = Txb_Title.Text?.Trim();
            string content = Txb_Content.Text;
            string header = Txb_Header.Text?.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("请输入标题！");
                return;
            }
            if (string.IsNullOrEmpty(content))
            {
                MessageBox.Show("请输入内容！");
                return;
            }
            if (string.IsNullOrEmpty(header))
            {
                MessageBox.Show("请输入页眉！");
                return;
            }
            var refList = new List<Reference>(referenceList.ToList());

            WordArticle article = new WordArticle(title, content, header, refList);
            ClearWordInputs();
            // 等待保存word文件
            await SaveWordFile(article, filePath);

            MessageBoxResult msgResult = MessageBox.Show("生成完毕。\n是否打开查看？", "提示",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (msgResult == MessageBoxResult.OK)
            {
                OpenDocFile(filePath);
            }         
        }

        // 异步生成文档并保存
        private async System.Threading.Tasks.Task SaveWordFile(WordArticle article, string path)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                article.SaveToFile(path);
            });
        }

        // 打开doc文件
        private void OpenDocFile(string filePath)
        {
            var wordApp = new MsWord.Application();
            Object filename = filePath;
            Object confirmConversions = Type.Missing;
            Object readOnly = Type.Missing;
            Object addToRecentFiles = Type.Missing;
            Object passwordDocument = Type.Missing;
            Object passwordTemplate = Type.Missing;
            Object revert = Type.Missing;
            Object writePasswordDocument = Type.Missing;
            Object writePasswordTemplate = Type.Missing;
            Object format = Type.Missing;
            Object encoding = Type.Missing;
            Object visible = Type.Missing;
            Object openConflictDocument = Type.Missing;
            Object openAndRepair = Type.Missing;
            Object documentDirection = Type.Missing;
            Object noEncodingDialog = Type.Missing;

            for (int i = 1; i <= wordApp.Documents.Count; i++)
            {
                String str = wordApp.Documents[i].FullName.ToString();
                if (str == filename.ToString())
                {
                    MessageBox.Show("请勿重复打开该文档");
                    return;
                }
            }
            try
            {
                wordApp.Documents.Open(ref filename,
                    ref confirmConversions, ref readOnly, ref addToRecentFiles,
                    ref passwordDocument, ref passwordTemplate, ref revert,
                    ref writePasswordDocument, ref writePasswordTemplate,
                    ref format, ref encoding, ref visible, ref openConflictDocument,
                    ref openAndRepair, ref documentDirection, ref noEncodingDialog
                    );
                wordApp.Visible = true;
            }
            catch (Exception)
            {
                MessageBox.Show("打开Word文档出错");
            }
        }

        // 清空所有输入框的内容
        private void ClearWordInputs()
        {
            Txb_Reference.Text = "";
            Txb_Title.Text = "";
            Txb_Content.Text = "";
            Txb_Header.Text = "";
            referenceList.Clear();
        }


        // 添加参考文献
        private void Btn_Add_Ref(object sender, RoutedEventArgs e)
        {
            string refName = Txb_Reference.Text?.Trim();
            if (string.IsNullOrEmpty(refName))
            {
                MessageBox.Show("请输入参考文献！");
                return;
            }
            referenceList.Add(new Reference(refName));
            Txb_Reference.Text = "";
        }
        #endregion wordCOM

        # region ExcelCOM
        // 加载Excel表格
        private async void Btn_Load_Excel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Title = "载入Excel表格",
                CheckPathExists = true,
                CheckFileExists = true,
                Filter = "Excel files (*.xlsx)|*.xlsx",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };
            if (dlg.ShowDialog() == true)
            {
                excelPath = dlg.FileName;              
                await LoadExcel(dlg.FileName);
            }
        }

        // 异步加载Excel
        private async System.Threading.Tasks.Task LoadExcel(string filepath)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                Pb_Loading.Dispatcher.BeginInvoke(new System.Action(() => Pb_Loading.Visibility = Visibility.Visible));
                ShowExcelInDatagrid(filepath);
                Pb_Loading.Dispatcher.BeginInvoke(new System.Action(() => Pb_Loading.Visibility = Visibility.Hidden));
            });
        }

        // 在datagrid显示表格
        private void ShowExcelInDatagrid(string filepath)
        {
            excelDt = new System.Data.DataTable();
            try
            {
                object oMissing = System.Reflection.Missing.Value;
                app = new MsExcel.Application();
                wb = app.Workbooks.Open(filepath, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing,
                 oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                // 取得第一个工作薄
                Worksheet ws = (Worksheet)app.Worksheets.get_Item(1);
                int rows = ws.UsedRange.Rows.Count;
                int columns = ws.UsedRange.Columns.Count;
                for (int i = 1; i <= rows; i++)
                {
                    System.Data.DataRow dr = excelDt.NewRow();
                    for (int j = 1; j <= columns; j++)
                    {
                        MsExcel.Range range = ws.Range[app.Cells[i, j], app.Cells[i, j]];
                        range.Select();
                        //读取列头
                        if (i == 1)                                 
                        {
                            string colName = app.ActiveCell.Text.ToString();
                            //是否存在重复列名
                            if (excelDt.Columns.Contains(colName))                    
                            {
                                excelDt.Columns.Add(colName + j);
                            }
                            else { excelDt.Columns.Add(colName); }
                        }
                        dr[j - 1] = app.ActiveCell.Text.ToString();
                    }
                    if (i != 1)
                    {
                        excelDt.Rows.Add(dr);
                    }
                }
                Dg_Excel.Dispatcher.BeginInvoke(new System.Action(() => Dg_Excel.ItemsSource = excelDt.DefaultView));
                ws = null;
            }
            catch (Exception e)
            {
                MessageBox.Show("打开Excel失败。请检查是否文件被占用 "+e.ToString());
            }
        }

        // 点击向Excel添加图表
        private void Btn_Add_Chart_Click(object sender, RoutedEventArgs e)
        {
            if (app == null)
            {
                MessageBox.Show("请先载入表格");
                return;
            }
            DlgExcelColRange dlgExcelColRange = new DlgExcelColRange();
            dlgExcelColRange.sendMessage += AddChartHandler;
            dlgExcelColRange.ShowDialog();
        }

        // Excel列范围输入窗口的回调
        private void AddChartHandler(int startCol, int EndCol)
        {
            try
            {
                AddChartToExcel(startCol, EndCol);
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("输入列不合法");
                return;
            }
            MessageBox.Show("添加图表成功");
        }

        // 向Excel添加图表
        /// <param name="startCol">起始列</param>
        /// <param name="EndCol">结束列</param>
        private void AddChartToExcel(int startCol, int EndCol)
        {
            wb.Charts.Add(Type.Missing, Type.Missing, 1, Type.Missing);
            Worksheet ws = (Worksheet)app.Worksheets.get_Item(1);
            int rows = ws.UsedRange.Rows.Count;
            int cols = ws.UsedRange.Columns.Count;
            // 注意Excel下标从1开始
            // 不是0
            if(startCol>EndCol || EndCol > cols)
            {
                throw new ArgumentOutOfRangeException();
            }
            string rangeStr = string.Format("{0}1:{1}{2}",
                (char)('A'+ startCol - 1), (char)('A'+EndCol-1), rows);
            MsExcel.Range chartRange = ws.Range[rangeStr];


            wb.ActiveChart.ChartType = MsExcel.XlChartType.xlLineMarkers;
            wb.ActiveChart.SetSourceData(chartRange, MsExcel.XlRowCol.xlColumns);
            wb.ActiveChart.Location(XlChartLocation.xlLocationAsObject, ws.Name);
            // 设置图表大小
            wb.ActiveChart.ChartArea.Width = 600;
            wb.ActiveChart.ChartArea.Height = 300;
        }

        // 保存Excel
        private void Btn_Save_Excel_Click(object sender, RoutedEventArgs e)
        {
            if (app == null)
            {
                MessageBox.Show("请先载入表格");
                return;
            }
            int rowCount = excelDt.Rows.Count;
            int colCount = excelDt.Columns.Count;
            int colIndex = 0;
            Worksheet ws = (Worksheet)app.Worksheets.get_Item(1);
            app.DisplayAlerts = false;
            ws.Cells.Clear();
            foreach (System.Data.DataColumn col in excelDt.Columns)
            {
                colIndex++;
                ws.Cells[1, colIndex] = col.ColumnName;
            }

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < colCount; j++)
                {
                    ws.Cells[i+2, j+1] = excelDt.Rows[i][j];
                }

            }
            SetExcelBorders(3);
            int suffix = excelPath.LastIndexOf(".xlsx");
            // 新文件名
            object filename = excelPath.Substring(0, suffix) + "_new.xlsx";
            wb.SaveAs(filename);
            wb.Close(false, Missing.Value, Missing.Value);
            MessageBox.Show("保存成功");
            Dg_Excel.ItemsSource = null;
            // 释放COM对象  
            app.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
            app = null;
            wb = null;
            GC.Collect();
        }

        // 设置边框
        /// <param name="weight">宽度</param>
        private void SetExcelBorders(int weight)
        {
            if (app == null)
            {
                return;
            }
            // 取得第一个工作薄
            Worksheet ws = (MsExcel.Worksheet)app.Worksheets.get_Item(1);
            int rows = ws.UsedRange.Rows.Count;
            int columns = ws.UsedRange.Columns.Count;
            string rangeStr = string.Format("A1:{0}{1}", 
                (char)('A'+ columns-1), rows);
            // Console.WriteLine(rangeStr);
            MsExcel.Range range = ws.Range[rangeStr];
            range.Borders[MsExcel.XlBordersIndex.xlInsideHorizontal].Weight = weight;
            range.Borders[MsExcel.XlBordersIndex.xlInsideVertical].Weight = weight;
            range.Borders[MsExcel.XlBordersIndex.xlEdgeRight].Weight = weight;
            range.Borders[MsExcel.XlBordersIndex.xlEdgeBottom].Weight = weight;
        }


        #endregion
    }
}
