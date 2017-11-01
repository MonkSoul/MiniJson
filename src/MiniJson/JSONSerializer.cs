using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MiniJson
{
    /// <summary>
    /// JSON序列化类
    /// </summary>
    public class JSONSerializer
    {
        #region "常量"

        /// <summary>
        /// 附加到对象序列化的字符串中，对象类型对应的name
        /// </summary>
        internal const String ObjectTypeKey = "_$$_MiniJson_ObjectType_$$_";

        #endregion

        #region "字段"

        protected static Dictionary<Type, PropertyInfo[]> typeProperties = new Dictionary<Type, PropertyInfo[]>();//类型属性缓冲区
        protected static Dictionary<Type, FieldInfo[]> typeFields = new Dictionary<Type, FieldInfo[]>();//类型字段缓冲区

        protected InnerSerializer serialize = null;//序列化对象
        protected InnerDeSerializer deserialize = null;//反序列化对象
        protected JSONSerializeSetting setting = null; //配置对象

        #endregion

        #region "构造方法"

        public JSONSerializer() 
        { }

        public JSONSerializer(JSONSerializeSetting setting)
        {
            this.setting = setting;
        }

        public JSONSerializer(InnerSerializer serialize, InnerDeSerializer deserialize, JSONSerializeSetting setting) :
            this(setting)
        {
            this.serialize = serialize;
            this.deserialize = deserialize;
        }

        #endregion

        #region "公共方法"

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="target"></param>
        /// <param name="setting">配置对象（会覆盖构造对象时传入的配置对象）</param>
        /// <returns></returns>
        public String Serialize(Object target, JSONSerializeSetting setting = null)
        {
            if (this.serialize == null) 
            {
                this.serialize = new InnerSerializer(this, setting);
            }
            return this.serialize.Serialize(target, setting);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str">JSON字符串</param>
        /// <param name="targetType">目标对象类型</param>
        /// <param name="setting">配置对象（会覆盖构造对象时传入的配置对象）</param>
        /// <returns></returns>
        public Object DeSerialize(String str, Type targetType, JSONSerializeSetting setting = null)
        {
            if (this.deserialize == null)
            {
                this.deserialize = new InnerDeSerializer(this, setting);
            }
            return this.deserialize.DeSerialize(str, targetType, setting);
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str">JSON字符串</param>
        /// <param name="setting">配置对象（会覆盖构造对象时传入的配置对象）</param>
        /// <returns></returns>
        public T DeSerialize<T>(String str, JSONSerializeSetting setting = null)
        {
            return (T)DeSerialize(str, typeof(T), setting);
        }

        #endregion

        #region "受保护方法"

        /// <summary>
        /// 获取对象的所有属性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal FieldInfo[] GetFields(Object obj)
        {
            Type t = obj.GetType();
            if (typeFields.ContainsKey(t)) return typeFields[t];
            else
            {
                FieldInfo[] fields = t.GetFields();
                typeFields.Add(t, fields);
                return fields;
            }
        }

        /// <summary>
        /// 获取对象的所有属性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal PropertyInfo[] GetProperties(Object obj)
        {
            Type t = obj.GetType();
            if (typeProperties.ContainsKey(t)) return typeProperties[t];
            else
            {
                PropertyInfo[] properties = t.GetProperties();
                typeProperties.Add(t, properties);
                return properties;
            }
        }

        /// <summary>
        /// 获取对象的所有属性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal Dictionary<String, MemberInfo> GetMemberInfoDic(Object obj)
        {
            MemberInfo[] properties = GetProperties(obj);
            MemberInfo[] fields = GetFields(obj);
            Dictionary<String, MemberInfo> infoDic = new Dictionary<String, MemberInfo>();
            if (properties != null && properties.Length > 0)
            {
                for (var i = 0; i < properties.Length; i++)
                    infoDic.Add(properties[i].Name, properties[i]);
            }
            if (fields != null && fields.Length > 0)
            {
                for (var i = 0; i < fields.Length; i++)
                    infoDic.Add(fields[i].Name, fields[i]);
            }
            return infoDic;
        }

        #endregion
    }
}
