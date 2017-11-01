using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MiniJson
{
    /// <summary>
    /// JSON序列化类（执行序列化操作的）
    /// </summary>
    public class InnerSerializer
    {
        #region "字段"

        protected List<Object> visitedObj = new List<Object>(); //保存遍历过的对象（以防出现无限递归）
        protected Object currentSerializeObj = null;//当前正在序列化的对象实例
        protected JSONSerializeSetting setting = null; //配置对象
        protected JSONSerializer jsonSerializeHelper = null;

        #endregion

        #region "构造方法"

        public InnerSerializer(JSONSerializer jsonSerializeHelper)
        {
            this.jsonSerializeHelper = jsonSerializeHelper;
        }

        public InnerSerializer(JSONSerializer jsonSerializeHelper, JSONSerializeSetting setting) :
            this(jsonSerializeHelper)
        {
            this.setting = setting;
        }

        #endregion

        #region "公共方法"

        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="target"></param>
        /// <param name="setting">配置对象（会覆盖构造对象时传入的配置对象）</param>
        /// <returns></returns>
        public virtual String Serialize(Object target, JSONSerializeSetting setting = null)
        {
            lock (this)
            {
                JSONSerializeSetting setting_backup = this.setting;//backup，先把创建对象传入的配置对象保存下来
                if (setting != null) this.setting = setting;
                this.currentSerializeObj = target;
                String json = String.Empty;
                try
                {
                    json = DoSerialize(target);
                }
                catch (Exception ex)
                {
                    throw ex;
                    //MyLog.MakeLog(ex);
                }
                this.Reset();//重置对象值
                this.setting = setting_backup;
                return json;
            }
        }

        #endregion

        #region "受保护方法"

        /// <summary>
        /// 重置对象
        /// </summary>
        protected void Reset()
        {
            this.currentSerializeObj = null;
            //typeProperties = new Dictionary<Type, PropertyInfo[]>();//类型属性缓冲区
            //typeFields = new Dictionary<Type, FieldInfo[]>();//类型字段缓冲区
            visitedObj = new List<Object>(); //保存遍历过的对象（以防出现无限递归）
        }

        /// <summary>
        /// 序列化每一项
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected String SerializeItem(MemberInfo memberInfo, Object value)
        {
            //在这里增加一个回调处理属性值的方法
            value = FormatValue(memberInfo, value);
            if (value == null) return NullValue(memberInfo);
            Type valueType = value.GetType();//需要序列化的值的类型
            if (valueType.IsValueType || valueType == typeof(String)) //值类型或者是String类型
            {
                return this.SerializeValue(memberInfo, value);
            }
            else if (value is IDictionary) //字典的处理方式
            {
                return this.SerializeDic(memberInfo, value);
            }
            else if (value is IEnumerable) //数组的处理方式
            {
                return this.SerializeArray(memberInfo, value);
            }
            else if (valueType.IsArray) //数组的处理方式
            {
                return this.SerializeArray(memberInfo, value);
            }
            else //对象的处理办法
            {
                //记录已经遍历过的对象
                if (visitedObj.Contains(value)) return String.Empty;
                else
                {
                    visitedObj.Add(value);
                    return this.SerializeObj(value);
                }
            }
        }

        /// <summary>
        /// 处理所有的属性/字段
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="memberInfos"></param>
        protected String DoWithObjMember(Object obj, MemberInfo[] memberInfos)
        {
            if (memberInfos == null || memberInfos.Length == 0) return String.Empty;
            StringBuilder tempSB = new StringBuilder();
            Int32 k = 0;
            for (var i = 0; i < memberInfos.Length; i++)
            {
                PropertyInfo property = memberInfos[i] as PropertyInfo;
                FieldInfo field = memberInfos[i] as FieldInfo;
                Object value = property == null ? field.GetValue(obj) : property.GetValue(obj, null);
                Type type = property == null ? field.FieldType : property.PropertyType;
                //执行序列化操作
                String json = SerializeItem(memberInfos[i], value);
                if (!String.IsNullOrEmpty(json))
                {
                    tempSB.AppendFormat("{0}\"{1}\":{2}", k > 0 ? "," : "", memberInfos[i].Name, json);
                    k++;
                }
            }
            //最终再加上对象的完整类型名，以供反序列化使用（反序列化的时候会根据这个类型名去创建对象实例然后执行反序列化）
            Type objType = obj.GetType();
            String objectTypeStr = String.Format("{0},{1}", objType.FullName, objType.Assembly.FullName);
            //对字符串进行序列化
            objectTypeStr = SerializeValue(null, objectTypeStr);
            tempSB.AppendFormat("{0}\"{1}\":{2}", k > 0 ? "," : "", JSONSerializer.ObjectTypeKey, objectTypeStr);
            return tempSB.ToString();
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="info"></param>
        /// <param name="genericObj">泛型对象</param>
        /// <returns></returns>
        protected String SerializeDic(MemberInfo info, Object genericObj)
        {
            StringBuilder sb = new StringBuilder();
            IDictionary dic = genericObj as IDictionary;
            if (dic != null)
            {
                sb.Append("{");
                ICollection keys = dic.Keys;
                if (keys.Count > 0)
                {
                    Int32 i = 0;
                    foreach (var key in keys)
                    {
                        Object value = dic[key];
                        String json = SerializeItem(info, value);
                        if (!String.IsNullOrEmpty(json))
                        {
                            sb.AppendFormat("{0}\"{1}\":{2}", i > 0 ? "," : "", key.ToString(), json);
                            i++;
                        }
                    }
                }
                sb.Append("}");
            }
            return sb.ToString();
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="info"></param>
        /// <param name="array"></param>
        /// <returns></returns>
        protected String SerializeArray(MemberInfo info, Object array)
        {
            if (array == null) return NullValue(info);
            IEnumerable enumArray = array as IEnumerable;
            StringBuilder tempSB = new StringBuilder();
            tempSB.Append("[");
            Int32 i = 0;
            foreach (Object item in enumArray)
            {
                if (i > 0) tempSB.Append(",");
                tempSB.Append(SerializeItem(info, item));
                i++;
            }
            tempSB.Append("]");
            return tempSB.ToString();
        }

        /// <summary>
        /// 序列化值
        /// </summary>
        /// <param name="info">当前格式化的属性对象</param>
        /// <param name="value">需要序列化的值</param>
        protected String SerializeValue(MemberInfo info, Object value)
        {
            if (value == null) return NullValue(info);
            Type t = value.GetType();
            //回调委托处理值（这段代码移到了SerializeItem()方法中）
            //value = FormatValue(info, value);
            //进行序列化
            if (value != null)
            {
                String valueStr = value.ToString();
                valueStr = valueStr.Replace("\\", "\\\\");
                valueStr = valueStr.Replace("\"", "\\\"");
                if (t.IsValueType && t != typeof(Char)) return valueStr;
                else return "\"" + valueStr + "\"";
            }
            else
            {
                if (t.IsValueType) return "\"\"";
                else return "\"null\"";
            }
        }

        /// <summary>
        /// 格式化null
        /// </summary>
        /// <param name="info">当前格式化的属性/字段对象</param>
        /// <returns></returns>
        protected String NullValue(MemberInfo info)
        {
            //通过委托处理控制的情况
            if (this.setting != null && this.setting.NullValueHandler != null) return this.setting.NullValueHandler(this.currentSerializeObj, info);
            //默认的返回值
            return "null";
        }

        /// <summary>
        /// 执行序列化
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected String DoSerialize(Object target)
        {
            if (target == null) return NullValue(null);
            Type type = target.GetType();
            //如果是值类型或者字符串类型，都直接序列化值即可。否则进行序列化对象
            if (type.IsValueType || type == typeof(String)) return SerializeValue(null, target);
            else return SerializeObj(target);
        }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="target"></param>
        protected String SerializeObj(Object target)
        {
            if (target == null) return NullValue(null);
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            visitedObj.Add(target);//记录所有入队的对象（标识访问过了）
            //执行序列化对象
            PropertyInfo[] properties = jsonSerializeHelper.GetProperties(target);
            FieldInfo[] fields = jsonSerializeHelper.GetFields(target);
            List<MemberInfo> memberInfoList = new List<MemberInfo>();
            memberInfoList.AddRange(properties);
            memberInfoList.AddRange(fields);
            String json = DoWithObjMember(target, memberInfoList.ToArray());
            sb.Append(json);
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// 格式化值
        /// </summary>
        /// <param name="info">属性/字段的对象</param>
        /// <param name="value">属性/字段值</param>
        /// <returns></returns>
        protected Object FormatValue(MemberInfo info, Object value)
        {
            if (this.setting != null && this.setting.ValueFormatting != null)
            {
                Delegate[] delegates = this.setting.ValueFormatting.GetInvocationList();
                for (var i = 0; i < delegates.Length; i++)
                {
                    try
                    {
                        value = delegates[i].DynamicInvoke(this.currentSerializeObj, info, value);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                        //MyLog.MakeLog(ex);
                    }
                }
            }
            return value;
        }

        #endregion
    }
}
