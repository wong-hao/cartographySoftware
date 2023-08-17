using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Plugin.DCDProcess
{
    

    public enum ResultState
    {
        Ok,//正常执行完
        Cancel,//取消执行
        Failed//遇到异常
    }
    /// <summary>
    /// 执行操作结果信息
    /// </summary>
    public struct ResultMessage
    {
        /// <summary>
        /// 处理状态
        /// </summary>
        public ResultState stat;
        /// <summary>
        /// 提示消息,约定如果状态为Failed,msg为失败提示消息
        /// </summary>
        public string msg;
        /// <summary>
        /// 自定义返回参数
        /// </summary>
        public object info;
    }
}
