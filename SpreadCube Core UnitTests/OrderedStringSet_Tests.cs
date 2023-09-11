using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadCube_Core;

namespace SpreadCube_Core_UnitTests;

[TestClass]
public class OrderedStringSet_Tests
{
    [TestMethod]
    public void Add_addingTheSameValueTwice_doesNotChangeTheList()
    {
        OrderedStringSet oss1 = new()
        {
            "one",
            "two",
            "three"
        };

        OrderedStringSet oss2 = new()
        {
            "one",
            "two",
            "three"
        };

        CollectionAssert.AreEqual(oss1, oss2);
        oss1.Add("two");
        CollectionAssert.AreEqual(oss1, oss2);
    }

    [TestMethod]
    public void CanGetFirst()
    {
        OrderedStringSet oss = new()
        {
            "one",
            "two",
            "three"
        };

        Assert.AreEqual("one", oss.First());
    }
}