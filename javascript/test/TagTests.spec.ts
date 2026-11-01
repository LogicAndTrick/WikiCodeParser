import { describe, expect, test } from '@jest/globals';
import { CodeTag, QuoteTag, Tag } from '../src';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';
import { sanitise } from './Util';

class TagTestData {
    input: string;
    expectedHtml: string;
    expectedPlainText: string;
    expectedMetaData: { key: string; value: any }[];
    isBlock: boolean;

    public get blockSpace() {
        return this.isBlock ? '' : ' ';
    }
    public get blockNewLine() {
        return this.isBlock ? '' : '\n';
    }

    constructor(input: string, expectedHtml: string, expectedPlainText: string, expectedMetaData: { key: string; value: any }[] = [], isBlock: boolean = false) {
        this.input = input;
        this.expectedHtml = expectedHtml;
        this.expectedPlainText = expectedPlainText;
        this.expectedMetaData = expectedMetaData;
        this.isBlock = isBlock;
    }
}

function testSimpleTransform(parser: Parser, data: TagTestData, transform: (x: TagTestData, s: string) => string) {
    const input = transform(data, data.input);
    const html = sanitise(transform(data, data.expectedHtml));
    const plain = transform(data, data.expectedPlainText);
    const result = parser.ParseResult(input);
    expect(result.ToHtml(sanitise)).toBe(html);
    expect(result.ToPlainText()).toBe(plain);
    const expectedMeta = data.expectedMetaData;
    const meta = result.GetMetadata();
    expect(meta.length).toBe(expectedMeta.length);
    for (let i = 0; i < meta.length; i++) {
        const em = expectedMeta[i];
        const m = meta[i];
        expect(m.Key).toBe(em.key);
        expect(m.Value).toBe(em.value);
    }
}

function baseTest(tag: Tag, data: TagTestData[]) {
    const config = new ParserConfiguration();
    config.Tags.push(tag);
    const parser = new Parser(config);

    for (const td of data) {
        test(`Standalone: ${td.input}`, () => testSimpleTransform(parser, td, (d, x) => x));
        test(`Before: ${td.input}`, () => testSimpleTransform(parser, td, (d, x) => 'before' + d.blockSpace + x));
        test(`After: ${td.input}`, () => testSimpleTransform(parser, td, (d, x) => x + d.blockSpace + 'after'));
        test(`Before and after: ${td.input}`, () => testSimpleTransform(parser, td, (d, x) => 'before' + d.blockSpace + x + d.blockSpace + 'after'));
        test(`New line: ${td.input}`, () => testSimpleTransform(parser, td, (d, x) => 'before' + d.blockNewLine + x + d.blockNewLine + 'after'));
    }
}

describe('CodeTagTest', () => {
    baseTest(new CodeTag(), [
        new TagTestData('[code]code?[/code]', '<code>code?</code>', 'code?'),
        new TagTestData('[code]code?[/CODE]', '<code>code?</code>', 'code?'),
        new TagTestData('[code][code]code?[/code][/code]', '<code>[code]code?</code>[/code]', '[code]code?[/code]'),
    ]);
});

describe('QuoteTagTest', () => {
    baseTest(new QuoteTag(), [
        new TagTestData('[quote]quote[/quote]', '<blockquote>quote</blockquote>', '[quote]\nquote\n[/quote]'),
        new TagTestData('[quote][quote]quote[/quote][/quote]', '<blockquote><blockquote>quote</blockquote></blockquote>', '[quote]\n[quote]\nquote\n[/quote]\n[/quote]'),
        new TagTestData('[quote][QUOTE]quote[/quote][/QUOTE]', '<blockquote><blockquote>quote</blockquote></blockquote>', '[quote]\n[quote]\nquote\n[/quote]\n[/quote]'),
    ]);
});
