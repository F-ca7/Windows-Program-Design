using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyCOM
{
    [Guid("B702AA0E-8DB2-4AD9-8451-CACF80E7115D")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Description("统计工具")]
    public class MyStatisticsCOM : IMyStatisticsCOM
    {
        // 返回数组平均值
        public double GetMean(double[] arr)
        {
            double sum = 0d;
            foreach(double d in arr)
            {
                sum += d;
            }
            return sum / arr.Length;
        }

        // 返回数组方差
        public double GetVar(double[] arr)
        {
            double var = 0d, avg = arr.Average();
            foreach (double d in arr)
            {
                var += (d - avg) * (d + avg);
            }
            return var / arr.Length;
        }
    }
}
