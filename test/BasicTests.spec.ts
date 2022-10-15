import { assert } from 'chai';
import { describe } from 'mocha';
import { INode } from '../src/Nodes/INode';
import { NodeCollection } from '../src/Nodes/NodeCollection';
import { PlainTextNode } from '../src/Nodes/PlainTextNode';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';

/**
 * 
    [TestMethod]
    public void TestHtmlEscapingOutsideTag()
    {
        var parser = new Parser(new ParserConfiguration());
        var result = parser.ParseResult("1 & 2");
        Assert.IsInstanceOfType(result.Content, typeof(NodeCollection));
        var leaves = GetLeaves(result.Content);
        Assert.AreEqual(1, leaves.Count);
        var node = leaves[0];
        Assert.IsInstanceOfType(node, typeof(PlainTextNode));
        var ptnode = (PlainTextNode)node;
        Assert.AreEqual("1 & 2", ptnode.Text);
        Assert.AreEqual("1 &amp; 2", ptnode.ToHtml());
        Assert.AreEqual("1 & 2", ptnode.ToPlainText());
    }
 */
function GetLeavesRecursive(list : INode[], node: INode) : void
{
    const children = node.GetChildren();
    if (children.length == 0)
    {
        list.push(node);
    }
    else
    {
        for (const child of children) GetLeavesRecursive(list, child);
    }
}
    
function GetLeaves(root : INode) : INode[] {
    const list : INode[] = [];
    GetLeavesRecursive(list, root);
    return list;
}

describe('Basic tests', () => {
    it('test html escaping outside tag', () => {
        const parser = new Parser(ParserConfiguration.Default());
        const result = parser.ParseResult('1 & 2');
        assert.isTrue(result.Content instanceof NodeCollection);
        const leaves = GetLeaves(result.Content);
        assert.equal(leaves.length, 1);
        const node = leaves[0] as PlainTextNode;
        assert.isTrue(node instanceof PlainTextNode);
        assert.equal(node.Text, '1 & 2');
        assert.equal(node.ToHtml(), '1 &amp; 2');
        assert.equal(node.ToPlainText(), '1 & 2');
    });
});