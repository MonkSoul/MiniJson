using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace MiniJson.Tests
{
    [TestClass]
    public class SerializeTest
    {
        #region "测试类"

        class TestObject1
        {
            public Queue<List<String>> QueueAA
            {
                get;
                set;
            }

            public Dictionary<String, String> ABCD
            {
                get;
                set;
            }

            public List<TestObject2> TestList
            {
                get;
                set;
            }

            public List<Dictionary<String, String>> ABCD123
            {
                get;
                set;
            }

            public Char AAAA = 'A';

            //[JsonConverter(typeof(MaskConverter))]
            public String Name
            {
                get;
                set;
            }

            public String[] ZFC
            {
                get;
                set;
            }

            public TestObject2[] Name2
            {
                get;
                set;
            }

            public Int32 KKKKK
            {
                get { return kkkkk; }
                set { kkkkk = 0; }
            }

            public Int32 kkkkk = 11;
            private Int32 kkkkk2 = 11;
            public String LLLL = "3333";
            //public Int32 LL =  999;

            //public TestObject TestObj = new TestObject() {  aaa = 1000};

            public Int32 aaa
            {
                get;
                set;
            }

            public TestObject1 Self
            {
                get;
                set;
            }
        }

        class TestObject2
        {
            public String TestProperty
            {
                get;
                set;
            }
        }

        class BObj
        {
            public String AA
            {
                get;
                set;
            }

            public String BB
            {
                get;
                set;
            }

            public BObj InnerObj;
        }

        #endregion

        #region "测试普通值数据，非对象"

        [TestMethod]
        public void TestMethod_value1()
        {
            Object obj = new String('a', 10);
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<String>(s1, s2);
        }

        [TestMethod]
        public void TestMethod_value2()
        {
            Object obj = 1;
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<String>(s1, s2);
        }


        [TestMethod]
        public void TestMethod_value3()
        {
            Object obj = '1';
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<String>(s1, s2);
        }

        #endregion

        #region "测试普通对象"

        [TestMethod]
        public void TestMethod_Obj1()
        {
            TestObject1 obj = new TestObject1()
            {
                Name = "ddd"
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObject1 deSerializeObj1 = JsonConvert.DeserializeObject<TestObject1>(s1);
            Assert.AreEqual<String>(deSerializeObj1.Name, obj.Name);
        }

        #endregion

        #region "测试对象指引递归的情况"

        [TestMethod]
        public void TestMethod_Obj2()
        {
            TestObject1 obj = new TestObject1()
            {
                Name = "ddd"
            };
            obj.Self = obj;
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            //String s2 = JsonConvert.SerializeObject(obj); (json.net递归会报错）
            //Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            //TestObject1 deSerializeObj1 = JsonConvert.DeserializeObject<TestObject1>(s1);
            TestObject1 deSerializeObj1 = json.DeSerialize<TestObject1>(s1);
            Assert.AreEqual<String>(deSerializeObj1.Name, obj.Name);
        }

        #endregion

        #region "测试复杂对象（嵌套集合）"

        [TestMethod]
        public void TestMethod_Collection()
        {
            TestObject1 obj = new TestObject1();
            Dictionary<String, String> dic = new Dictionary<string, string>();
            dic.Add("ddd", "kkkk");
            dic.Add("ccc", "bbbb");
            dic.Add("bbb", "aaaa");
            obj.ABCD = dic;
            List<TestObject2> obj2List = new List<TestObject2>();
            obj2List.Add(new TestObject2()
            {
                TestProperty = "ddd"
            });
            obj.TestList = obj2List;
            List<Dictionary<String, String>> dicList = new List<Dictionary<string, string>>();
            dicList.Add(dic);
            obj.ABCD123 = dicList;
            Queue<List<String>> kkk = new Queue<List<string>>();
            List<String> kkkList1 = new List<string>();
            List<String> kkkList2 = new List<string>();
            List<String> kkkList3 = new List<string>();
            kkkList1.Add("1");
            kkkList1.Add("2");
            kkkList1.Add("3");
            kkkList1.Add("4");
            kkk.Enqueue(kkkList1);
            kkk.Enqueue(kkkList2);
            kkk.Enqueue(kkkList3);
            kkk.Enqueue(null);
            //obj.QueueAA = kkk; json.net反序列化会报错，所以不能添加这个属性
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObject1 deSerializeObj1 = JsonConvert.DeserializeObject<TestObject1>(s2);
            Assert.AreEqual<Int32>(deSerializeObj1.TestList.Count, obj.TestList.Count);
        }

        #endregion

        #region "测试特殊字符"

        [TestMethod]
        public void TestSpecialChar()
        {
            //String inputStr = "\"";
            String inputStr = ",}][}}{{{{}]][][][][[]]]][[,,{}.,{},[]<{}<,}";

            BObj bobj = new BObj();
            bobj.AA = inputStr;
            bobj.BB = inputStr;
            bobj.InnerObj = new BObj() { AA = inputStr, BB = inputStr };
            String json = JsonConvert.SerializeObject(bobj);
            JSONSerializer serialize = new JSONSerializer();
            String json2 = serialize.Serialize(bobj);
            //Assert.AreEqual<String>(json, json2);

            BObj tempBobj = JsonConvert.DeserializeObject<BObj>(json);
            BObj tempBobj2 = serialize.DeSerialize<BObj>(json2, null);
            Assert.AreEqual<String>(tempBobj2.AA, inputStr);
            Assert.AreEqual<String>(tempBobj2.BB, inputStr);
            Assert.AreEqual<String>(tempBobj2.InnerObj.AA, inputStr);
            Assert.AreEqual<String>(tempBobj2.InnerObj.BB, inputStr);

            Assert.AreEqual<String>(tempBobj.AA, inputStr);
            Assert.AreEqual<String>(tempBobj.BB, inputStr);
            Assert.AreEqual<String>(tempBobj.InnerObj.AA, inputStr);
            Assert.AreEqual<String>(tempBobj.InnerObj.BB, inputStr);
        }

        [TestMethod]
        public void TestSpecialChar2()
        {
            String inputStr = ",,,,]]][[[}}{{{}{,}[]\"\\";

            String json = JsonConvert.SerializeObject(inputStr);
            JSONSerializer serialize = new JSONSerializer();
            String json2 = serialize.Serialize(inputStr);
            Assert.AreEqual<String>(json, json2);

            String tempStr2 = serialize.DeSerialize<String>(json, null);
            Assert.AreEqual<String>(tempStr2, inputStr);
        }

        #endregion
    }
}
