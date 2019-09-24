using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyCOM
{
    [Guid("031581EF-67B5-4C6C-B900-0B770F6C9E78")]
    [ComVisible(true)]
    public interface IMyStatisticsCOM
    {
        [DispId(1)]
        double GetMean(double[] arr);

        [DispId(2)]
        double GetVar(double[] arr);
    }
}
