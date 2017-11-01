using System;
using System.Reflection;

namespace MiniJson
{
    /// <summary>
    /// JSON序列化类
    /// </summary>
    public class JSONSerializeSetting
    {
        /// <summary>
        /// （序列化遇到空值null时回调）控制的处理回调，传入参数：
        /// 1.传入当前序列化的对象实例
        /// 2.当前正在序列化的属性/字段对象（可能为空的）
        /// 处理必须返回结果字符串
        /// </summary>
        public Func<Object, MemberInfo, String> NullValueHandler
        {
            get;
            set;
        }

        /// <summary>
        /// （序列化/反序列化 每一个属性/字段值都会回调）控制的处理回调，传入参数：
        /// 1.传入当前序列化/反序列化的对象实例
        /// 2.当前正在序列化/反序列化的属性/字段对象（可能为空的）
        /// 3.当前序列化/反序列化的值
        /// 返回处理后的值（Object类型）
        /// </summary>
        public Func<Object, MemberInfo, Object, Object> ValueFormatting
        {
            get;
            set;
        }

        /// <summary>
        /// 反序列化创建对象实例的回调方法，传入参数：
        /// 1.传入当前序列化的对象实例
        /// 2.当前反序列化处理的属性对象
        /// 3.当前反序列化处理的属性的期待值（准备把这个值写入到这个属性里）
        /// 4.需要返回的类型（此类型一定符合属性对象的类型的）
        /// </summary>
        public Func<Object, MemberInfo, String, Type> CalculateDeSerializeType
        {
            get;
            set;
        }
    }
}
