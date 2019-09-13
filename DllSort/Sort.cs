using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DllSharpSort
{
    public class Sort
    {
        /// <summary>
        /// 冒泡排序, 从小到大
        /// </summary>
        public static void BubbleSort(int[] array)
        {
            // 一轮中是否发生交换
            bool flag = false;
            for (int i = 0; i < array.Length - 1; i++)
            {
                for(int j = 0; j < array.Length - i - 1; j++)
                {
                    // 前大于后，交换
                    if (array[j] > array[j + 1])
                    {
                        Swap(ref array[j], ref array[j+1]);
                        flag = true;
                    }
                }
                if (!flag)
                    break;
            }

        }
        private static void Swap<T>(ref T a, ref T b)
        {
            T t = a;
            a = b;
            b = t;
        }
    }
}
