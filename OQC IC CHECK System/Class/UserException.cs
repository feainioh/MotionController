using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OQC_IC_CHECK_System
{ /// <summary>
  /// 机台复位
  /// </summary>
    class ResetMachineErr : ApplicationException
    {
        internal ResetMachineErr(string msg)
            : base(msg)
        { }
    }

    /// <summary>
    /// 机台暂停
    /// </summary>
    class PauseMachineErr : ApplicationException
    {
        internal PauseMachineErr(string msg)
            : base(msg)
        { }
    }

    /// <summary>
    /// 拍照失败
    /// </summary>
    class PhotoFail : ApplicationException
    {
        internal PhotoFail(string msg)
            : base(msg)
        { }
    }

    /// <summary>
    /// 异常 是否需要复位
    /// </summary>
    class ErrReset : ApplicationException
    {
        /// <summary>
        /// 该异常是否需要复位
        /// </summary>
        internal readonly bool NeedReset = false;
        internal ErrReset(string msg, bool needreset = false)
            : base(msg)
        {
            this.NeedReset = needreset;
        }
    }
}
