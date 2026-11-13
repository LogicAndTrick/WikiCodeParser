namespace LogicAndTrick.WikiCodeParser.Tests;

[TestClass]
[TestCategory("Basic")]
public class StateTests
{
    [TestMethod]
    public void ScanTo()
    {
        var st = new State(0, 0, "A B C");
        Assert.AreEqual("A ", st.ScanTo("B"));
        Assert.AreEqual("B ", st.ScanTo("C"));
        Assert.AreEqual("C", st.ScanTo("D"));
    }

    [TestMethod]
    public void SkipWhitespace()
    {
        var st = new State(0, 0, "A B C");
        st.SkipWhitespace();
        Assert.AreEqual(0, st.Index);
        st.Seek(1, false);
        st.SkipWhitespace();
        Assert.AreEqual(2, st.Index);
        st.Seek(1, false);
        st.SkipWhitespace();
        Assert.AreEqual(4, st.Index);
    }

    [TestMethod]
    public void PeekTo()
    {
        var st = new State(0, 0, "A B C");
        Assert.AreEqual("A ", st.PeekTo("B"));
        Assert.AreEqual("A B ", st.PeekTo("C"));
        Assert.AreEqual(null, st.PeekTo("D"));
        st.Seek(2, false);
        Assert.AreEqual("", st.PeekTo("B"));
        Assert.AreEqual("B ", st.PeekTo("C"));
        Assert.AreEqual(null, st.PeekTo("D"));
    }

    [TestMethod]
    public void Seek()
    {
        var st = new State(0, 0, "A B C");
        Assert.AreEqual(0, st.Index);
        st.Seek(1, false);
        Assert.AreEqual(1, st.Index);
        st.Seek(1, false);
        Assert.AreEqual(2, st.Index);
        st.Seek(3, false);
        Assert.AreEqual(5, st.Index);
        st.Seek(3, true);
        Assert.AreEqual(3, st.Index);
    }

    [TestMethod]
    public void Peek()
    {
        var st = new State(0, 0, "A B C");
        Assert.AreEqual("A ", st.Peek(2));
        Assert.AreEqual("A B ", st.Peek(4));
        Assert.AreEqual("A B C", st.Peek(6));
        st.Seek(2, false);
        Assert.AreEqual("B ", st.Peek(2));
        Assert.AreEqual("B C", st.Peek(4));
        Assert.AreEqual("B C", st.Peek(6));
    }

    [TestMethod]
    public void Next()
    {
        var st = new State(0, 0, "A B C");
        Assert.AreEqual('A', st.Next());
        Assert.AreEqual(' ', st.Next());
        Assert.AreEqual('B', st.Next());
        Assert.AreEqual(' ', st.Next());
        Assert.AreEqual('C', st.Next());
        Assert.AreEqual('\0', st.Next());
        Assert.AreEqual('\0', st.Next());
    }

    [TestMethod]
    public void GetToken()
    {
        Assert.AreEqual(null, new State(0, 0, "none").GetToken());
        Assert.AreEqual("some", new State(0, 0, "[some]").GetToken());
        Assert.AreEqual("some", new State(0, 0, "[some=thing]").GetToken());
        Assert.AreEqual("some", new State(0, 0, "[some thing]").GetToken());
        var st = new State(0, 0, "a [some] b");
        st.ScanTo("[");
        Assert.AreEqual(2, st.Index);
        Assert.AreEqual("some", st.GetToken());
        Assert.AreEqual(2, st.Index);
    }

    [TestMethod]
    public void LineNumber1()
    {
        var st = new State(1, 0, "A\nB\nC");
        Assert.AreEqual(1, st.SourceLine);
        Assert.AreEqual(0, st.SourceColumn);

        Assert.AreEqual('A', st.Next());
        Assert.AreEqual(1, st.SourceLine);
        Assert.AreEqual(1, st.SourceColumn);

        Assert.AreEqual('\n', st.Next());
        Assert.AreEqual(2, st.SourceLine);
        Assert.AreEqual(0, st.SourceColumn);

        Assert.AreEqual('B', st.Next());
        Assert.AreEqual(2, st.SourceLine);
        Assert.AreEqual(1, st.SourceColumn);

        Assert.AreEqual('\n', st.Next());
        Assert.AreEqual(3, st.SourceLine);
        Assert.AreEqual(0, st.SourceColumn);

        Assert.AreEqual('C', st.Next());
        Assert.AreEqual(3, st.SourceLine);
        Assert.AreEqual(1, st.SourceColumn);
    }

    [TestMethod]
    public void LineNumber2()
    {
        var st = new State(5, 15, "A\nB\nC");
        Assert.AreEqual(5, st.SourceLine);
        Assert.AreEqual(15, st.SourceColumn);

        Assert.AreEqual('A', st.Next());
        Assert.AreEqual(5, st.SourceLine);
        Assert.AreEqual(16, st.SourceColumn);

        Assert.AreEqual('\n', st.Next());
        Assert.AreEqual(6, st.SourceLine);
        Assert.AreEqual(0, st.SourceColumn);

        Assert.AreEqual('B', st.Next());
        Assert.AreEqual(6, st.SourceLine);
        Assert.AreEqual(1, st.SourceColumn);

        Assert.AreEqual('\n', st.Next());
        Assert.AreEqual(7, st.SourceLine);
        Assert.AreEqual(0, st.SourceColumn);

        Assert.AreEqual('C', st.Next());
        Assert.AreEqual(7, st.SourceLine);
        Assert.AreEqual(1, st.SourceColumn);
    }
}