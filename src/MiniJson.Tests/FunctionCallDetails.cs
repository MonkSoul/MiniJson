using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MiniJson.Tests
{
    /// <summary>
    /// 反射调用方法所需要的相关参数
    /// </summary>
    public class FunctionCallDetails
    {
        /// <summary>
        /// 用于序列化反射的时候使用的
        /// </summary>
        protected FunctionCallDetails()
        {
        }

        /// <summary>
        /// public构造函数，必须传入方法名
        /// </summary>
        /// <param name="functionFullName"></param>
        public FunctionCallDetails(String functionFullName)
        {
            this.FunctionFullName = functionFullName;
        }

        /// <summary>
        /// 方法完整名称（包括命名空间）
        /// </summary>
        public String FunctionFullName
        {
            get;
            protected set;
        }

        /// <summary>
        /// 输入方法的参数数组
        /// 该参数数组支持传入一个FunctionCallDetails
        /// </summary>
        public List<Object> InputParams
        {
            get;
            set;
        }
    }
}
