﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProtoCore.DSASM;
using ProtoTestFx.TD;

namespace ProtoFFITests
{
    [TestFixture]
    class ArgumentMarshalingTests 
    {
        TestFrameWork theTest = null;
        [SetUp]
        public void Setup()
        {
            theTest = new TestFrameWork();
        }

        [TearDown]
        public void TearDown()
        {
            theTest.CleanUp();
        }

        [Test]
        public void TestReturnIList()
        {
            string code = @"
            
            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "ReturnIList");
            //IList is marshaled as arbitrary rank var array
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeVar, methods[0].ReturnType.Value.UID);
            Assert.AreEqual(Constants.kArbitraryRank, methods[0].ReturnType.Value.rank);
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeInt, args[0].UID);
            Assert.AreEqual(1, args[0].rank); //Expecting it tobe marshaled as 1D array

            theTest.Verify("b", new int[] { 1, 2, 3, 4, 5 });
        }

        [Test]
        public void TestAcceptIEnumerablOfIList()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "AcceptIEnumerablOfIList");
            //IEnumerable<IList> ==> var[]..[]
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeVar, args[0].UID);
            Assert.AreEqual(Constants.kArbitraryRank, args[0].rank); //Expecting it tobe marshaled as arbitrary dimension array

            theTest.Verify("b", new List<object> { new int[] { 1, 2, 3, 4, 5 }, new int[] { 6, 7, 8, 9, 10 } });
        }

        [Test]
        public void TestAcceptIEnumerablOfIListInt()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "AcceptIEnumerablOfIListInt");
            //IEnumerable<IList<int>> ==> int[][]
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeInt, args[0].UID);
            Assert.AreEqual(2, args[0].rank); //Expecting it tobe marshaled as 2D array

            theTest.Verify("b", new List<object> { new int[] { 1, 2, 3, 4, 5 }, new int[] { 6, 7, 8, 9, 10 } });
        }

        [Test]
        public void TestAcceptListOfList()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "AcceptListOfList");
            //List<List<int>> ==> int[][]
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeInt, args[0].UID);
            Assert.AreEqual(2, args[0].rank); //Expecting it tobe marshaled as 2D array

            theTest.Verify("b", new List<object> { new int[] { 1, 2, 3, 4, 5 }, new int[] { 6, 7, 8, 9, 10 } });
        }

        [Test]
        public void TestAccept3DList()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "Accept3DList");
            //List<List<List<int>>> ==> int[][][]
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeInt, args[0].UID);
            Assert.AreEqual(3, args[0].rank); //Expecting it tobe marshaled as 3D array

            theTest.Verify("b", new List<object> { new List<object> { new int[] { 1, 2, 3, 4, 5 } } });
        }

        [Test]
        public void TestReturnListOf5Points()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "ReturnListOf5Points");
            //Verify DummyPoint class is marshaled
            Assert.IsTrue(ProtoCore.DSASM.Constants.kInvalidIndex != theTest.GetClassIndex("DummyPoint"));

            //IList<DummyPoint> ==> DummyPoint[]
            Assert.AreEqual("FFITarget.DummyPoint", methods[0].ReturnType.Value.Name);
            Assert.AreEqual(1, methods[0].ReturnType.Value.rank);

            var args = methods[0].GetArgumentTypes();
            Assert.IsTrue(args == null || args.Count == 0);
            
            theTest.Verify("c", 5);
        }

        [Test]
        public void TestAcceptListOf5PointsReturnAsObject()
        {
            string code = @"

            theTest.RunScriptSource(code);
            var methods = theTest.GetMethods("DummyCollection", "AcceptListOf5PointsReturnAsObject");
            //Verify DummyPoint class is marshaled
            Assert.IsTrue(ProtoCore.DSASM.Constants.kInvalidIndex != theTest.GetClassIndex("DummyPoint"));

            //object is marshaled as arbitrary rank var array
            Assert.AreEqual((int)ProtoCore.PrimitiveType.kTypeVar, methods[0].ReturnType.Value.UID);
            Assert.AreEqual(Constants.kArbitraryRank, methods[0].ReturnType.Value.rank);

            //IEnumerable<DummyPoint> ==> DummyPoint[]
            var args = methods[0].GetArgumentTypes();
            Assert.AreEqual("FFITarget.DummyPoint", args[0].Name);
            Assert.AreEqual(1, args[0].rank); //Expecting it tobe marshaled as 3D array

            theTest.Verify("c", 5);
        }
    }
}