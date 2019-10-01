using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FangWinFormApp
{
    public class WinAPI
    {
        /// <summary>
        /// 使用COPYDATASTRUCT来传递字符串
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;   // 传入自定义的数据，只能是4字节整数
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;   // 消息字符串
        }

    }
}
