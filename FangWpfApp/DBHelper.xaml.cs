using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FangWpfApp
{
    class Table : INotifyPropertyChanged
    {
        private string _name;

        public Table(string name)
        {
            Name = name;
        }

        public string Name { get { return _name; }
            set
            { _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Name"));
            } }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
    // 电影评论的实体类
    class Comment
    {
        int movieId;
        string content;
        string username;

        public Comment(int movieId, string content, string username)
        {
            MovieId = movieId;
            Content = content;
            Username = username;
        }

        public int MovieId { get => movieId; set => movieId = value; }
        public string Content { get => content; set => content = value; }
        public string Username { get => username; set => username = value; }
    }

    public partial class MainWindow
    {
        // 格式化连接
        const string CONN_FORMAT = "server=localhost;User Id={0};password={1};Database={2}";
        // 获取当前数据库所有表名
        const string SQL_GET_TABLENAME = "select table_name tableName from information_schema.tables " +
            "where table_schema = (select database());";

        MySqlDataAdapter mda;
        DataTable dt;

        private MySqlConnection conn;
        // 数据库表名
        private ObservableCollection<Table> tableList = new ObservableCollection<Table>();
        // 一页的大小
        private const int PAGE_SIZE = 20;
        // 当前页
        private int curPage = 1;
        // 当前表名
        private string curTableName;
        // 当前字段名
        private string[] curFieldNames;

        // 显示MySQL面板
        private void Btn_Mysql_Click(object sender, RoutedEventArgs e)
        {
            HideAllDBGrids();
            Grid_Mysql.Visibility = Visibility.Visible;
            // 绑定数据源
            Lv_Table_Name.ItemsSource = tableList;
        }

        // 连接MySQL数据库
        private void Btn_Connect_Mysql_Click(object sender, RoutedEventArgs e)
        {
            string username = Txb_Username.Text, pwd = Txb_Pwd.Password, dbName = Txb_DBName.Text;
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(dbName))
            {
                MessageBox.Show("请完整填写用户名密码");
                return;
            }
            try
            {
                conn = new MySqlConnection(string.Format(CONN_FORMAT, username, pwd, dbName));
                conn.Open();
                MessageBox.Show("连接成功！");

            }
            catch (Exception)
            {
                MessageBox.Show("连接失败！");
                return;
            }

            LoadAllTableNames();           
        }

        // 获取所有表名
        private void LoadAllTableNames()
        {
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = SQL_GET_TABLENAME;
            MySqlDataAdapter mda = new MySqlDataAdapter(cmd);

            DataSet ds = new DataSet();
            mda.Fill(ds);
            tableList.Clear();
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                string name = r["tableName"].ToString();
                tableList.Add(new Table(name));
            }
        }

        // 双击表名载入对应表
        private void Lv_Table_Name_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (conn == null)
            {
                return;
            }
            object o = Lv_Table_Name.SelectedItem;
            if (o == null)
                return;
            // 获取点击的表名
            TextBlock item = e.Device.Target as TextBlock;
            Table table = o as Table;
            curTableName = table.Name;
            curPage = 0;
            LoadTable(curTableName);
        }

        // 加载对应数据表
        private void LoadTable(string tableName)
        {
            string sql = string.Format("select * from `{0}` limit {1}, {2}", 
                tableName, curPage * PAGE_SIZE, PAGE_SIZE);

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            mda = new MySqlDataAdapter(cmd);
            MySqlCommandBuilder commandBuilder = new MySqlCommandBuilder(mda);
            mda.InsertCommand = commandBuilder.GetInsertCommand(true) 
                as MySqlCommand;
            mda.DeleteCommand = commandBuilder.GetDeleteCommand();
            mda.UpdateCommand = commandBuilder.GetUpdateCommand(true) 
                as MySqlCommand;
            dt = new DataTable();
            mda.Fill(dt);         
            if (curPage == 0)
            {
                // 更新字段名
                curFieldNames = GetFieldNamesInTable(dt);
            }
            Dg_table.ItemsSource = dt.DefaultView;
            Dg_table.AutoGenerateColumns = true;
        }

        // 获取字段名
        private string[] GetFieldNamesInTable(DataTable dt)
        {
            string[] strColumns = null;
            if (dt.Columns.Count > 0)
            {
                int columnNum = dt.Columns.Count;
                strColumns = new string[columnNum];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    strColumns[i] = dt.Columns[i].ColumnName;
                }
            }
            return strColumns;
        }


        private void Dg_table_Save(object sender, RoutedEventArgs e)
        {           
            try
            {
                mda.Update(dt);
                MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("写回数据库失败！"+ex.ToString());
            }
        }




        // 上一页
        private void Btn_Prev_Page_Click(object sender, RoutedEventArgs e)
        {
            if (curPage == 0)
            {
                return;
            }
            curPage--;
            LoadTable(curTableName);

        }
        // 下一页
        private void Btn_Next_Page_Click(object sender, RoutedEventArgs e)
        {
            curPage++;
            LoadTable(curTableName);
        }

        private void HideAllDBGrids()
        {
            Grid_Mysql.Visibility = Visibility.Hidden;
        }
    }
}
