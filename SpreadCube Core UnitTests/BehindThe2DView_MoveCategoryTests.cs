using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadCube_Core;

namespace SpreadCube_Core_UnitTests;

[TestClass]
public class BehindThe2DView_MoveCategoryTests
{
    [TestMethod]
    public void CanMoveToEmpty()
    {
        List<string> hoCat = new() { "Names" };
        List<string> vCat = new();
        List<string> hiCat = new();
        BehindThe2DView core = new(hoCat, vCat, hiCat);

        Test(CategoryListType.Horizontal, CategoryListType.Vertical, hoCat, hiCat);
        Test(CategoryListType.Vertical, CategoryListType.Hidden, hoCat, vCat);
        Test(CategoryListType.Hidden, CategoryListType.Horizontal, vCat, hiCat);

        void Test(CategoryListType origin, CategoryListType receiver, List<string> empty1, List<string> empty2)
        {
            var categoryName = "Names";
            core.MoveCategory(categoryName, origin, receiver, 0);
            var receiving = receiver switch
            {
                CategoryListType.Horizontal => hoCat,
                CategoryListType.Vertical => vCat,
                CategoryListType.Hidden => hiCat,
                _ => throw new Exception("This should not happen, ever")
            };
            Assert.AreEqual(1, receiving.Count);
            Assert.AreEqual(categoryName, receiving[0]);
            Assert.IsTrue(!empty1.Any());
            Assert.IsTrue(!empty2.Any());
        }
    }

    [TestMethod]
    public void CanMoveToNonEmpty()
    {
        List<string> hCat = new() { "Ages", "Names" }; ;
        List<string> vCat = new() { "Classes", "Teachers" };
        BehindThe2DView core = new(hCat, vCat, new List<string>());
        core.MoveCategory("Names", CategoryListType.Horizontal, CategoryListType.Vertical, 1);

        Assert.AreEqual(1, hCat.Count);
        Assert.AreEqual(3, vCat.Count);
        Assert.AreEqual("Ages", hCat[0]);
        CollectionAssert.AreEqual(new List<string>() { "Classes", "Names", "Teachers" }, vCat);
    }

    [TestMethod]
    public void MovingToSamePositionDoesNothing()
    {
        List<string> hCat = new() { "Ages", "Names" }; ;
        BehindThe2DView core = new(hCat, new List<string>(), new List<string>());

        core.MoveCategory("Names", CategoryListType.Horizontal, CategoryListType.Horizontal, 1);
        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Ages", "Names" }, hCat);

        core.MoveCategory("Names", CategoryListType.Horizontal, CategoryListType.Horizontal, 2);
        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Ages", "Names" }, hCat);

        core.MoveCategory("Ages", CategoryListType.Horizontal, CategoryListType.Horizontal, 0);
        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Ages", "Names" }, hCat);

        core.MoveCategory("Ages", CategoryListType.Horizontal, CategoryListType.Horizontal, 1);
        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Ages", "Names" }, hCat);
    }

    [TestMethod]
    public void MovingForwardInSameWorks()
    {
        List<string> hCat = new() { "Ages", "Names" }; ;
        BehindThe2DView core = new(hCat, new List<string>(), new List<string>());
        core.MoveCategory("Ages", CategoryListType.Horizontal, CategoryListType.Horizontal, 2);

        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Names", "Ages" }, hCat);
    }

    [TestMethod]
    public void MovingBackwardInSameWorks()
    {
        List<string> hCat = new() { "Ages", "Names" }; ;
        BehindThe2DView core = new(hCat, new List<string>(), new List<string>());
        core.MoveCategory("Names", CategoryListType.Horizontal, CategoryListType.Horizontal, 0);

        Assert.AreEqual(2, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Names", "Ages" }, hCat);
    }

    [TestMethod]
    public void MovingForwardInSameWorks_LargerList()
    {
        List<string> hCat = new() { "One", "Two", "Three", "Four" }; ;
        BehindThe2DView core = new(hCat, new List<string>(), new List<string>());

        core.MoveCategory("One", CategoryListType.Horizontal, CategoryListType.Horizontal, 2);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Two", "One", "Three", "Four" }, hCat);

        core.MoveCategory("One", CategoryListType.Horizontal, CategoryListType.Horizontal, 4);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Two", "Three", "Four", "One" }, hCat);

        core.MoveCategory("Two", CategoryListType.Horizontal, CategoryListType.Horizontal, 4);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Three", "Four", "One", "Two" }, hCat);
    }

    [TestMethod]
    public void MovingBackwardInSameWorks_LargerList()
    {
        List<string> hCat = new() { "One", "Two", "Three", "Four" }; ;
        BehindThe2DView core = new(hCat, new List<string>(), new List<string>());

        core.MoveCategory("Two", CategoryListType.Horizontal, CategoryListType.Horizontal, 0);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Two", "One", "Three", "Four" }, hCat);

        core.MoveCategory("Three", CategoryListType.Horizontal, CategoryListType.Horizontal, 1);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Two", "Three", "One", "Four" }, hCat);

        core.MoveCategory("Four", CategoryListType.Horizontal, CategoryListType.Horizontal, 0);
        Assert.AreEqual(4, hCat.Count);
        CollectionAssert.AreEqual(new List<string>() { "Four", "Two", "Three", "One" }, hCat);
    }
}