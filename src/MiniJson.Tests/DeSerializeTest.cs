using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

namespace MiniJson.Tests
{
    [TestClass]
    public class DeSerializeTest
    {
        #region "测试类"

        class TestObj
        {
            public TestObj2[] TestArray4
            {
                get;
                set;
            }

            public String[] TestArray
            {
                get;
                set;
            }

            public String[][] TestArray2
            {
                get;
                set;
            }

            public String[,] TestArray3
            {
                get;
                set;
            }

            public List<String> AA
            {
                get;
                set;
            }

            public ArrayList BB
            {
                get;
                set;
            }

            public String CC;

            public Dictionary<String, TestObj2> DD
            {
                get;
                set;
            }

            public Dictionary<String, String> EE
            {
                get;
                set;
            }

            public Dictionary<String, Int32> FF
            {
                get;
                set;
            }
        }

        class TestObj2
        {
            public String Name;
        }

        #endregion

        #region "测试普通属性"

        [TestMethod]
        public void TestMethod_Obj()
        {
            TestObj obj = new TestObj()
            {
                CC = "ddd"
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<String>(deSerializeObj1.CC, obj.CC);
        }

        [TestMethod]
        public void TestMethod_Obj2()
        {
            TestObj obj = new TestObj()
            {
                CC = ""
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<String>(deSerializeObj1.CC, obj.CC);
            Assert.AreEqual<String>(deSerializeObj1.CC, "");
        }

        [TestMethod]
        public void TestMethod_Obj3()
        {
            TestObj obj = new TestObj()
            {
                CC = null
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<String>(deSerializeObj1.CC, obj.CC);
            Assert.AreEqual<String>(deSerializeObj1.CC, null);
        }

        #endregion

        #region "测试List<T>"

        [TestMethod]
        public void TestMethod_ListT()
        {
            TestObj obj = new TestObj();
            List<String> aa = new List<string>();
            aa.Add("lijian");
            obj.AA = aa;
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<Int32>(deSerializeObj1.AA.Count, 1);
            Assert.AreEqual<String>(deSerializeObj1.AA[0], "lijian");
        }

        #endregion

        #region "测试List"

        [TestMethod]
        public void TestMethod_List()
        {
            TestObj obj = new TestObj();
            ArrayList aa = new ArrayList();
            aa.Add("lijian");
            obj.BB = aa;
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<Int32>(deSerializeObj1.BB.Count, 1);
            Assert.AreEqual<String>(deSerializeObj1.BB[0].ToString(), "lijian");
        }

        #endregion

        #region "测试数组"

        [TestMethod]
        public void TestMethod_Array()
        {
            TestObj obj = new TestObj()
            {
                TestArray = new String[] { "aa", "bb" }
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<Int32>(deSerializeObj1.TestArray.Length, 2);
            Assert.AreEqual<String>(deSerializeObj1.TestArray[0], "aa");
        }

        [TestMethod]
        public void TestMethod_Array2()
        {
            TestObj obj = new TestObj()
            {
                TestArray = new String[] { "aa", "bb" },
                TestArray2 = new String[][] { new String[] { "aa", "bb" }, new String[] { "aa", "bb" } },
                //TestArray3 = new String[,] { { "aa", "bb" }, { "cc", "dd" } }
                TestArray4 = new TestObj2[] { new TestObj2() { Name = "lijian" } }
            };
            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<Int32>(deSerializeObj1.TestArray.Length, 2);
            Assert.AreEqual<Int32>(deSerializeObj1.TestArray2.Length, 2);
            Assert.AreEqual<String>(deSerializeObj1.TestArray2[0][1], "bb");
            Assert.AreEqual<String>(deSerializeObj1.TestArray[0], "aa");
            Assert.AreEqual<String>(deSerializeObj1.TestArray4[0].Name, "lijian");
        }

        #endregion

        #region "测试Dictionary"

        [TestMethod]
        public void TestMethod_Dictionary()
        {
            TestObj obj = new TestObj();
            Dictionary<String, TestObj2> dic1 = new Dictionary<String, TestObj2>();
            dic1.Add("1", new TestObj2() { Name = "name1" });
            dic1.Add("2", new TestObj2() { Name = "name2" });
            obj.DD = dic1;

            JSONSerializer json = new JSONSerializer();
            String s1 = json.Serialize(obj);
            String s2 = JsonConvert.SerializeObject(obj);
            Assert.AreEqual<Int32>(s1.Length, s2.Length);
            //返序列化回来的校验（断言）
            TestObj deSerializeObj1 = json.DeSerialize<TestObj>(s1);
            Assert.AreEqual<Int32>(deSerializeObj1.DD.Count, 2);
            Assert.AreEqual<String>(deSerializeObj1.DD["1"].Name, "name1");
        }

        #endregion

        #region "不通过的按钮测试"

        public void Error1()
        {
        }

        #endregion

        #region "没有无参构造函数的测试"

        [TestMethod]
        public void TestMethod_NoParameter()
        {
            FunctionCallDetails innerDetails = new FunctionCallDetails("GetTextResourceString");
            innerDetails.InputParams = new List<object>();
            innerDetails.InputParams.Add("client");
            innerDetails.InputParams.Add(null);

            FunctionCallDetails details = new FunctionCallDetails("OpenForm");
            details.InputParams = new List<object>();
            details.InputParams.Add("iHotel.WinForm.SysSetting.RoomManager.RoomType");
            details.InputParams.Add(innerDetails);

            JSONSerializer serializer = new JSONSerializer();
            String json = serializer.Serialize(details);
            FunctionCallDetails details2 = serializer.DeSerialize<FunctionCallDetails>(json);
            Assert.IsNotNull(details2);
            Assert.AreEqual<String>(details2.FunctionFullName, "OpenForm");
            FunctionCallDetails innerDetails2 = details2.InputParams[1] as FunctionCallDetails;
            Assert.IsNotNull(innerDetails2);
            Assert.AreEqual<String>(innerDetails2.FunctionFullName, "GetTextResourceString");
        }

        #endregion
    }
}
