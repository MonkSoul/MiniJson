using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace MiniJson
{
    /// <summary>
    /// JSON序列化类（执行反序列化操作的）
    /// </summary>
    public class InnerDeSerializer
    {
        #region "字段"

        protected JSONSerializeSetting setting = null; //配置对象
        protected JSONSerializer jsonSerializeHelper = null;

        #endregion

        #region "构造方法"

        public InnerDeSerializer(JSONSerializer jsonSerializeHelper)
        {
            this.jsonSerializeHelper = jsonSerializeHelper;
        }

        public InnerDeSerializer(JSONSerializer jsonSerializeHelper, JSONSerializeSetting setting) :
            this(jsonSerializeHelper)
        {
            this.setting = setting;
        }

        #endregion

        #region "公共方法"

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="str">JSON字符串</param>
        /// <param name="targetType">目标对象类型</param>
        /// <param name="setting">配置对象（会覆盖构造对象时传入的配置对象）</param>
        /// <returns></returns>
        public virtual Object DeSerialize(String str, Type targetType, JSONSerializeSetting setting = null)
        {
            lock (this)
            {
                JSONSerializeSetting setting_backup = this.setting;//backup，先把创建对象传入的配置对象保存下来
                if (setting != null) this.setting = setting;
                Object instance = null;
                try
                {
                    instance = DoDeSerialize(str, targetType);
                }
                catch (Exception ex)
                {
                    throw ex;
                    //Log.Project.LogError("error:", ex);
                }
                this.setting = setting_backup;
                return instance;
            }
        }

        #endregion

        #region "受保护方法"

        /// <summary>
        /// 移除双引号
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected String RemoveQuotation(String str)
        {
            if (String.IsNullOrEmpty(str)) return String.Empty;
            str = str.Trim();
            if (str.StartsWith("\"")) str = str.Substring(1, str.Length - 2);//去掉字符串两边的引号
            return str;
        }

        /// <summary>
        /// 序列化每一个值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected Object DeSerializeValue(String valueStr, Type targetType)
        {
            valueStr = RemoveQuotation(valueStr);//去掉字符串两边的引号
            valueStr = valueStr.Replace("\\\"", "\"");
            valueStr = valueStr.Replace("\\\\", "\\");
            if (targetType == typeof(UInt16))
            {
                return Convert.ToUInt16(valueStr);
            }
            else if (targetType == typeof(UInt32))
            {
                return Convert.ToUInt32(valueStr);
            }
            else if (targetType == typeof(UInt64))
            {
                return Convert.ToUInt64(valueStr);
            }
            else if (targetType == typeof(Int16))
            {
                return Convert.ToInt16(valueStr);
            }
            else if (targetType == typeof(Int32))
            {
                return Convert.ToInt32(valueStr);
            }
            else if (targetType == typeof(Int64))
            {
                return Convert.ToInt64(valueStr);
            }
            else if (targetType == typeof(Single))
            {
                return Convert.ToSingle(valueStr);
            }
            else if (targetType == typeof(String))
            {
                return valueStr;
            }
            else if (targetType == typeof(Boolean))
            {
                return Convert.ToBoolean(valueStr);
            }
            else if (targetType == typeof(Byte))
            {
                return Convert.ToByte(valueStr);
            }
            else if (targetType == typeof(Char))
            {
                return Convert.ToChar(valueStr);
            }
            else if (targetType == typeof(DateTime))
            {
                return Convert.ToDateTime(valueStr);
            }
            else if (targetType == typeof(Decimal))
            {
                return Convert.ToDecimal(valueStr);
            }
            else if (targetType == typeof(Double))
            {
                return Convert.ToDouble(valueStr);
            }
            else if (targetType == typeof(SByte))
            {
                return Convert.ToSByte(valueStr);
            }
            else
            {
                return valueStr;
            }
        }

        /// <summary>
        /// 反序列化对象属性值（字段值）
        /// </summary>
        /// <param name="valueStr"></param>
        /// <param name="memberType"></param>
        protected Object DeSerializeItem(String valueStr, Type memberType)
        {
            Object memberValue = null;
            //下面的判断顺序不能随意调整
            if (IsObjStr(valueStr) || IsArrayStr(valueStr))
            {
                if (typeof(IDictionary).IsAssignableFrom(memberType))//字典类型
                {
                    memberValue = DeSerializeDic(valueStr, memberType);
                }
                else if (memberType.IsArray) //数组也实现了IList接口的，例如：String[]数组也支持IList的，所以数组判断要在IList判断之前
                {
                    memberValue = DeSerializeArray(valueStr, memberType);
                }
                else if (typeof(IList).IsAssignableFrom(memberType) || typeof(IList<>).IsAssignableFrom(memberType)) //表示可按照索引单独访问的对象的(非)泛型集合。
                {
                    memberValue = DeSerializeList(valueStr, memberType);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(memberType))
                {
                    memberValue = DeSerializeIEnumItem(valueStr, memberType);
                }
                else if (memberType.IsClass && memberType != typeof(String))
                {
                    memberValue = DeSerializeToObj(valueStr, memberType);
                }
            }
            else
            {
                memberValue = DeSerializeValue(valueStr, memberType);
            }
            return memberValue;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        protected Object DeSerializeDic(String str, Type memberType)
        {
            if (String.IsNullOrEmpty(str)) return null;
            str = str.Trim();
            str = str.Substring(1, str.Length - 2); //去掉前后的{括号字符
            String[] itemStr = SplitItemStr(str);
            IDictionary dic = (IDictionary)memberType.Assembly.CreateInstance(memberType.FullName);
            //获取字典类型的 值类型
            Type[] genericArcTypes = memberType.GetGenericArguments();
            Type arrayItemType = genericArcTypes[1];
            if (itemStr != null && itemStr.Length > 0)
            {
                for (var i = 0; i < itemStr.Length; i++)
                {
                    String[] nameValue = new String[] { MyString.Left(itemStr[i], ":"), MyString.Right(itemStr[i], ":") };
                    nameValue[0] = nameValue[0].Trim();
                    nameValue[0] = nameValue[0].Substring(1, nameValue[0].Length - 2);//名称去掉双引号
                    nameValue[1] = nameValue[1].Trim();
                    dic.Add(nameValue[0], DeSerializeItem(nameValue[1], arrayItemType));
                }
            }
            return dic;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        protected Object DeSerializeArray(String str, Type memberType)
        {
            if (String.IsNullOrEmpty(str)) return null;
            str = str.Trim();
            str = str.Substring(1, str.Length - 2); //去掉前后的[括号字符
            String[] itemStr = SplitItemStr(str);
            String memberTypeStr = memberType.ToString();
            String arrayItemTypeStr = String.Empty;
            if (memberTypeStr.IndexOf("[") >= 0) arrayItemTypeStr = memberTypeStr.Substring(0, memberTypeStr.Length - 2);
            Type arrayItemType = memberType.Assembly.GetType(arrayItemTypeStr);
            Array array = Array.CreateInstance(arrayItemType, itemStr.Length);
            //获取数组类型
            if (itemStr != null && itemStr.Length > 0)
            {
                for (var i = 0; i < itemStr.Length; i++)
                {
                    array.SetValue(DeSerializeItem(itemStr[i], arrayItemType), i);
                }
            }
            return array;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        protected Object DeSerializeList(String str, Type memberType)
        {
            if (String.IsNullOrEmpty(str)) return null;
            str = str.Trim();
            str = str.Substring(1, str.Length - 2); //去掉前后的[括号字符
            String[] itemStr = SplitItemStr(str);
            IList objList = (IList)memberType.Assembly.CreateInstance(memberType.FullName);
            Type listItemType = typeof(Object);
            if (memberType.IsGenericType) //泛型的处理
            {
                listItemType = memberType.GetGenericArguments()[0];
            }
            if (itemStr != null && itemStr.Length > 0)
            {
                for (var i = 0; i < itemStr.Length; i++)
                {
                    objList.Add(DeSerializeItem(itemStr[i], listItemType));
                }
            }
            return objList;
        }

        /// <summary>
        /// 序列化数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="memberType"></param>
        /// <returns></returns>
        protected Object DeSerializeIEnumItem(String str, Type memberType)
        {
            if (String.IsNullOrEmpty(str)) return null;
            str = str.Trim();
            str = str.Substring(1, str.Length - 2); //去掉前后的[括号字符
            String[] itemStr = SplitItemStr(str);
            ICollection enumInstance = (ICollection)memberType.Assembly.CreateInstance(memberType.FullName);
            Type listItemType = typeof(Object);
            if (memberType.IsGenericType) //泛型的处理
            {
                listItemType = memberType.GetGenericArguments()[0];
            }
            if (itemStr != null && itemStr.Length > 0)
            {
                for (var i = 0; i < itemStr.Length; i++)
                {
                    AddItemToCollection(enumInstance, DeSerializeItem(itemStr[i], listItemType));
                }
            }
            return enumInstance;
        }

        /// <summary>
        /// 添加项到集合中
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="item"></param>
        protected void AddItemToCollection(ICollection collection, Object item)
        {
            if (collection is Queue)
            {
                Queue queue = collection as Queue;
                queue.Enqueue(item);
            }
            else if (collection is Stack)
            {
                Stack stack = collection as Stack;
                stack.Push(item);
            }
        }

        /// <summary>
        /// 判断字符串是否是一个对象
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected Boolean IsObjStr(String str)
        {
            if (String.IsNullOrEmpty(str)) return false;
            str = str.Trim();
            return str.StartsWith("{") && str.EndsWith("}");
        }

        /// <summary>
        /// 判断字符串是否是一个对象
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected Boolean IsArrayStr(String str)
        {
            if (String.IsNullOrEmpty(str)) return false;
            str = str.Trim();
            return str.StartsWith("[") && str.EndsWith("]");
        }

        /// <summary>
        /// 根据字符串划分成每一个属性名值对
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        protected String[] SplitItemStr(String str)
        {
            if (String.IsNullOrEmpty(str)) return null;
            List<String> list = new List<String>();
            StringBuilder charList = new StringBuilder();
            Stack<Char> bracketsQueue = new Stack<Char>();
            for (var i = 0; i < str.Length; i++)
            {
                Char c = str[i];
                if (bracketsQueue.Count > 0 && bracketsQueue.Peek() == '\\')
                {
                    bracketsQueue.Pop();//出栈上一个转义字符
                    charList.Append(c);
                }
                else if (c == '\\') //发现转义字符，下一个字符不要进行判断，直接处理
                {
                    charList.Append(c);
                    bracketsQueue.Push(c);
                }
                else if (bracketsQueue.Count > 0 && bracketsQueue.Peek() == '"' && c != '"') //如果栈顶是分号，则在遇到另外一个分号（结束符）之前，全部直接处理
                {
                    charList.Append(c);
                }
                else if (c == '"') //发现冒号
                {
                    charList.Append(c);
                    if (bracketsQueue.Count > 0 && bracketsQueue.Peek() == '"')
                    {
                        bracketsQueue.Pop();//发现了冒号，则判断栈顶是否是冒号，是的话直接出栈
                    }
                    else
                    {
                        bracketsQueue.Push(c);
                    }
                }
                else if (c == '{' || c == '[') //发现左括号
                {
                    bracketsQueue.Push(c);
                    charList.Append(c);
                }
                else if ((c == '}' && bracketsQueue.Peek() == '{') ||
                       (c == ']' && bracketsQueue.Peek() == '[')) //发现右括号
                {
                    bracketsQueue.Pop();
                    charList.Append(c);
                }
                else if (c == ',')//遇到逗号，则判断是否属于对象内的逗号，是的话则跳过继续收集字符
                {
                    if (bracketsQueue.Count == 0)
                    {
                        list.Add(charList.ToString());
                        charList = new StringBuilder();
                    }
                    else
                    {
                        charList.Append(c);
                    }
                }
                else
                {
                    charList.Append(c);
                }
                if (i == str.Length - 1)
                {
                    list.Add(charList.ToString());
                }
            }
            return list.ToArray();
        }

        /// <summary>
        /// 根据大括号，序列化大括号里面的对象
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        protected Object DeSerializeToObj(String str, Type type)
        {
            if (String.IsNullOrEmpty(str)) return null;
            str = str.Trim();
            str = str.Substring(1, str.Length - 2); //去掉前后的{括号字符
            String[] itemStr = SplitItemStr(str);
            Object instance = type.Assembly.CreateInstance(type.FullName);
            if (itemStr != null && itemStr.Length > 0)
            {
                Dictionary<String, MemberInfo> memberInfoDic = jsonSerializeHelper.GetMemberInfoDic(instance);
                for (var i = 0; i < itemStr.Length; i++)
                {
                    String item = itemStr[i].Trim();
                    String[] nameValue = new String[] { MyString.Left(item, ":"), MyString.Right(item, ":") };
                    nameValue[0] = nameValue[0].Trim();
                    nameValue[0] = nameValue[0].Substring(1, nameValue[0].Length - 2);//名称去掉双引号
                    nameValue[1] = nameValue[1].Trim();
                    if (nameValue[1] == "null") continue; //空值的处理方式，此处可以开放接口给调用方注入
                    if (memberInfoDic.ContainsKey(nameValue[0]))
                    {
                        Type memberType = null;
                        PropertyInfo propertyInfo = null;
                        FieldInfo fieldInfo = null;
                        if (memberInfoDic[nameValue[0]].MemberType == MemberTypes.Property)
                        {
                            propertyInfo = memberInfoDic[nameValue[0]] as PropertyInfo;
                            memberType = propertyInfo.PropertyType;
                        }
                        else if (memberInfoDic[nameValue[0]].MemberType == MemberTypes.Field)
                        {
                            fieldInfo = memberInfoDic[nameValue[0]] as FieldInfo;
                            memberType = fieldInfo.FieldType;
                        }
                        if (this.setting != null && this.setting.CalculateDeSerializeType != null)
                        {
                            Type tempType = this.setting.CalculateDeSerializeType(instance, memberInfoDic[nameValue[0]], nameValue[1]);
                            if (tempType != null) memberType = tempType;
                        }
                        Object memberValue = DeSerializeItem(nameValue[1], memberType);
                        //反序列化前进行值的格式化
                        if (this.setting != null && this.setting.ValueFormatting != null)
                        {
                            memberValue = this.setting.ValueFormatting(instance, propertyInfo == null ? (MemberInfo)fieldInfo : (MemberInfo)propertyInfo, memberValue);
                        }
                        if (propertyInfo != null) propertyInfo.SetValue(instance, memberValue, null);
                        if (fieldInfo != null) fieldInfo.SetValue(instance, memberValue);
                    }
                }
                return instance;
            }
            return instance;
        }

        /// <summary>
        /// 执行反序列化
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type">目标类型</param>
        /// <returns></returns>
        protected Object DoDeSerialize(String str, Type type)
        {
            if (String.IsNullOrEmpty(str)) return null;
            if (IsObjStr(str)) return DeSerializeToObj(str, type);
            else return DeSerializeValue(str, type);
        }

        #endregion
    }
}
