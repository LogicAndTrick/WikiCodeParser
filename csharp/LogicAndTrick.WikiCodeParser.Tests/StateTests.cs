namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
public class StateTests
{
    [TestMethod]
    public void ScanTo()
    {
        var st = new State("A B C");
        Assert.AreEqual(st.ScanTo("B"), "A ");
        Assert.AreEqual(st.ScanTo("C"), "B ");
        Assert.AreEqual(st.ScanTo("D"), "C");
    }

    [TestMethod]
    public void SkipWhitespace()
    {
        var st = new State("A B C");
        st.SkipWhitespace();
        Assert.AreEqual(st.Index, 0);
        st.Seek(1, false);
        st.SkipWhitespace();
        Assert.AreEqual(st.Index, 2);
        st.Seek(1, false);
        st.SkipWhitespace();
        Assert.AreEqual(st.Index, 4);
    }

    [TestMethod]
    public void PeekTo()
    {
        var st = new State("A B C");
        Assert.AreEqual(st.PeekTo("B"), "A ");
        Assert.AreEqual(st.PeekTo("C"), "A B ");
        Assert.AreEqual(st.PeekTo("D"), null);
        st.Seek(2, false);
        Assert.AreEqual(st.PeekTo("B"), "");
        Assert.AreEqual(st.PeekTo("C"), "B ");
        Assert.AreEqual(st.PeekTo("D"), null);
    }

    [TestMethod]
    public void Seek()
    {
        var st = new State("A B C");
        Assert.AreEqual(st.Index, 0);
        st.Seek(1, false);
        Assert.AreEqual(st.Index, 1);
        st.Seek(1, false);
        Assert.AreEqual(st.Index, 2);
        st.Seek(3, false);
        Assert.AreEqual(st.Index, 5);
        st.Seek(3, true);
        Assert.AreEqual(st.Index, 3);
    }

    [TestMethod]
    public void Peek()
    {
        var st = new State("A B C");
        Assert.AreEqual(st.Peek(2), "A ");
        Assert.AreEqual(st.Peek(4), "A B ");
        Assert.AreEqual(st.Peek(6), "A B C");
        st.Seek(2, false);
        Assert.AreEqual(st.Peek(2), "B ");
        Assert.AreEqual(st.Peek(4), "B C");
        Assert.AreEqual(st.Peek(6), "B C");
    }

    [TestMethod]
    public void Next()
    {
        var st = new State("A B C");
        Assert.AreEqual(st.Next(), 'A');
        Assert.AreEqual(st.Next(), ' ');
        Assert.AreEqual(st.Next(), 'B');
        Assert.AreEqual(st.Next(), ' ');
        Assert.AreEqual(st.Next(), 'C');
        Assert.AreEqual(st.Next(), '\0');
        Assert.AreEqual(st.Next(), '\0');
    }

    [TestMethod]
    public void GetToken()
    {
        Assert.AreEqual(new State("none").GetToken(), null);
        Assert.AreEqual(new State("[some]").GetToken(), "some");
        Assert.AreEqual(new State("[some=thing]").GetToken(), "some");
        Assert.AreEqual(new State("[some thing]").GetToken(), "some");
        var st = new State("a [some] b");
        st.ScanTo("[");
        Assert.AreEqual(st.Index, 2);
        Assert.AreEqual(st.GetToken(), "some");
        Assert.AreEqual(st.Index, 2);
    }
}