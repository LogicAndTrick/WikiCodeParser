import { assert } from 'chai';
import { describe } from 'mocha';
import { INode } from '../src/Nodes/INode';
import { NodeCollection } from '../src/Nodes/NodeCollection';
import { PlainTextNode } from '../src/Nodes/PlainTextNode';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';
import { State } from '../src/State';

function GetLeavesRecursive(list: INode[], node: INode): void {
    const children = node.GetChildren();
    if (children.length == 0) {
        list.push(node);
    } else {
        for (const child of children) GetLeavesRecursive(list, child);
    }
}

function GetLeaves(root: INode): INode[] {
    const list: INode[] = [];
    GetLeavesRecursive(list, root);
    return list;
}

describe('State tests', () => {
    it('ScanTo', () => {
        const st = new State('A B C');
        assert.equal(st.ScanTo('B'), 'A ');
        assert.equal(st.ScanTo('C'), 'B ');
        assert.equal(st.ScanTo('D'), 'C');
    });
    it('SkipWhitespace', () => {
        const st = new State('A B C');
        st.SkipWhitespace();
        assert.equal(st.Index, 0);
        st.Seek(1, false);
        st.SkipWhitespace();
        assert.equal(st.Index, 2);
        st.Seek(1, false);
        st.SkipWhitespace();
        assert.equal(st.Index, 4);
    });
    it('PeekTo', () => {
        const st = new State('A B C');
        assert.equal(st.PeekTo('B'), 'A ');
        assert.equal(st.PeekTo('C'), 'A B ');
        assert.equal(st.PeekTo('D'), null);
        st.Seek(2, false);
        assert.equal(st.PeekTo('B'), '');
        assert.equal(st.PeekTo('C'), 'B ');
        assert.equal(st.PeekTo('D'), null);
    });
    it('Seek', () => {
        const st = new State('A B C');
        assert.equal(st.Index, 0);
        st.Seek(1, false);
        assert.equal(st.Index, 1);
        st.Seek(1, false);
        assert.equal(st.Index, 2);
        st.Seek(3, false);
        assert.equal(st.Index, 5);
        st.Seek(3, true);
        assert.equal(st.Index, 3);
    });
    it('Peek', () => {
        const st = new State('A B C');
        assert.equal(st.Peek(2), 'A ');
        assert.equal(st.Peek(4), 'A B ');
        assert.equal(st.Peek(6), 'A B C');
        st.Seek(2, false);
        assert.equal(st.Peek(2), 'B ');
        assert.equal(st.Peek(4), 'B C');
        assert.equal(st.Peek(6), 'B C');
    });
    it('Next', () => {
        const st = new State('A B C');
        assert.equal(st.Next(), 'A');
        assert.equal(st.Next(), ' ');
        assert.equal(st.Next(), 'B');
        assert.equal(st.Next(), ' ');
        assert.equal(st.Next(), 'C');
        assert.equal(st.Next(), '\0');
        assert.equal(st.Next(), '\0');
    });
    it('GetToken', () => {
        assert.equal(new State('none').GetToken(), null);
        assert.equal(new State('[some]').GetToken(), 'some');
        assert.equal(new State('[some=thing]').GetToken(), 'some');
        assert.equal(new State('[some thing]').GetToken(), 'some');
        const st = new State('a [some] b');
        st.ScanTo('[');
        assert.equal(st.Index, 2);
        assert.equal(st.GetToken(), 'some');
        assert.equal(st.Index, 2);
    });
});

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