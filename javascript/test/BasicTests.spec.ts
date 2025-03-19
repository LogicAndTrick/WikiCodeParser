import { describe, expect, test } from '@jest/globals';
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
    test('ScanTo', () => {
        const st = new State('A B C');
        expect(st.ScanTo('B')).toBe('A ');
        expect(st.ScanTo('C')).toBe('B ');
        expect(st.ScanTo('D')).toBe('C');
    });
    test('SkipWhitespace', () => {
        const st = new State('A B C');
        st.SkipWhitespace();
        expect(st.Index).toBe(0);
        st.Seek(1, false);
        st.SkipWhitespace();
        expect(st.Index).toBe(2);
        st.Seek(1, false);
        st.SkipWhitespace();
        expect(st.Index).toBe(4);
    });
    test('PeekTo', () => {
        let st = new State('A B C');
        expect(st.PeekTo('B')).toBe('A ');
        expect(st.PeekTo('C')).toBe('A B ');
        expect(st.PeekTo('D')).toBeNull();
        st.Seek(2, false);
        expect(st.PeekTo('B')).toBe('');
        expect(st.PeekTo('C')).toBe('B ');
        expect(st.PeekTo('D')).toBeNull();

        st = new State('Hello, [[AAAAA]] [[BB]]');
        expect(st.ScanTo('[[')).toBe('Hello, ');
        st.Seek(2, false);
        expect(st.ScanTo(']]')).toBe('AAAAA');
        st.Seek(3, false);
        expect(st.PeekTo(']]')).toBe('[[BB');
    });
    test('Seek', () => {
        const st = new State('A B C');
        expect(st.Index).toBe(0);
        st.Seek(1, false);
        expect(st.Index).toBe(1);
        st.Seek(1, false);
        expect(st.Index).toBe(2);
        st.Seek(3, false);
        expect(st.Index).toBe(5);
        st.Seek(3, true);
        expect(st.Index).toBe(3);
    });
    test('Peek', () => {
        const st = new State('A B C');
        expect(st.Peek(2)).toBe('A ');
        expect(st.Peek(4)).toBe('A B ');
        expect(st.Peek(6)).toBe('A B C');
        st.Seek(2, false);
        expect(st.Peek(2)).toBe('B ');
        expect(st.Peek(4)).toBe('B C');
        expect(st.Peek(6)).toBe('B C');
    });
    test('Next', () => {
        const st = new State('A B C');
        expect(st.Next()).toBe('A');
        expect(st.Next()).toBe(' ');
        expect(st.Next()).toBe('B');
        expect(st.Next()).toBe(' ');
        expect(st.Next()).toBe('C');
        expect(st.Next()).toBe('\0');
        expect(st.Next()).toBe('\0');
    });
    test('GetToken', () => {
        expect(new State('none').GetToken()).toBeNull();
        expect(new State('[some]').GetToken()).toBe('some');
        expect(new State('[some=thing]').GetToken()).toBe('some');
        expect(new State('[some thing]').GetToken()).toBe('some');
        const st = new State('a [some] b');
        st.ScanTo('[');
        expect(st.Index).toBe(2);
        expect(st.GetToken()).toBe('some');
        expect(st.Index).toBe(2);
    });
});

describe('Basic tests', () => {
    test('test html escaping outside tag', () => {
        const parser = new Parser(new ParserConfiguration());
        const result = parser.ParseResult('1 & 2');
        expect(result.Content).toBeInstanceOf(NodeCollection);
        const leaves = GetLeaves(result.Content);
        expect(leaves.length).toBe(1);
        const node = leaves[0] as PlainTextNode;
        expect(node).toBeInstanceOf(PlainTextNode);
        expect(node.Text).toBe('1 & 2');
        expect(node.ToHtml()).toBe('1 &amp; 2');
        expect(node.ToPlainText()).toBe('1 & 2');
    });
});