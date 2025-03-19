import { describe, expect, test } from '@jest/globals';
import { existsSync, readFileSync } from 'fs';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';

function AssertSame(name : string, expected : string, actual : string, split : boolean) : void {
    if (split) {
        const expectedLines = expected.split('\n');
        const actualLines = actual.split('\n');

        for (let i = 0; i < expectedLines.length; i++) {
            const ex = expectedLines[i];
            const ac = actualLines[i];
            expect(ac).toBe(ex);
        }
    } else {
        expect(actual).toBe(expected);
    }
}

function Test(config : ParserConfiguration, input: string, expectedOutput: string, expectedPlain : string | undefined, expectedMeta : string | undefined, split = false): void {
    const parser = new Parser(config);

    const result = parser.ParseResult(input);
    const resultHtml = result.ToHtml().trim();
    const resultPlain = result.ToPlainText().trim();
    const resultMeta = result.GetMetadata().map(x => `${x.Key}=${JSON.stringify(x.Value)}`).join('\n');

    AssertSame('html', expectedOutput, resultHtml, split);
    if (expectedPlain !== undefined) AssertSame('plain', expectedPlain, resultPlain, split);

    if (expectedMeta) {
        expect(resultMeta).toBe(expectedMeta);
    }
}

function RunTestCaseInFolder(config : ParserConfiguration, folder: string, name: string, split = false) {
    const dir = `${__dirname}/../../tests/${folder}`;
    let _in : string;
    let _out : string;
    let _plain : string | undefined = undefined;
    let _meta : string | undefined = undefined;
    if (existsSync(`${dir}/${name}`)) {
        const text = readFileSync(`${dir}/${name}`, 'utf-8');
        [_in, _out, _plain, _meta] = text.split('###').map(x => x.trim());
    } else {
        _in = readFileSync(`${dir}/${name}.in`, 'utf-8');
        _out = readFileSync(`${dir}/${name}.out`, 'utf-8');
        if (existsSync(`${dir}/${name}.plain`)) {
            _plain = readFileSync(`${dir}/${name}.plain`, 'utf-8');
        }
        if (existsSync(`${dir}/${name}.meta`)) {
            _meta = readFileSync(`${dir}/${name}.meta`, 'utf-8');
        }
    }
    _in = _in.replace(/\r/g, '');
    _out = _out.replace(/\r/g, '');
    _plain = _plain?.replace(/\r/g, '');
    _meta = _meta?.replace(/\r/g, '');
    Test(config, _in, _out, _plain, _meta, split);
}

describe('Isolated tests', () => {
    const RunTestCase = (name: string, split = false) => RunTestCaseInFolder(ParserConfiguration.Twhl(), 'isolated', name, split);

    test('missing-tag', () => RunTestCase('missing-tag'));
    test('unicode-escape', () => RunTestCase('unicode-escape'));
    
    test('ref-simple', () => RunTestCase('ref-simple'));
    
    test('pre-simple', () => RunTestCase('pre-simple'));
    test('pre-lang', () => RunTestCase('pre-lang'));
    test('pre-highlight', () => RunTestCase('pre-highlight'));

    test('mdcode-simple', () => RunTestCase('mdcode-simple'));
    test('mdcode-lang', () => RunTestCase('mdcode-lang'));

    test('heading-simple', () => RunTestCase('heading-simple'));

    test('mdline-simple', () => RunTestCase('mdline-simple'));

    test('columns-simple', () => RunTestCase('columns-simple'));
    test('columns-invalid', () => RunTestCase('columns-invalid'));

    test('panel-simple', () => RunTestCase('panel-simple'));

    test('mdquote-simple', () => RunTestCase('mdquote-simple'));
    test('mdquote-nested', () => RunTestCase('mdquote-nested'));
    
    test('list-simple', () => RunTestCase('list-simple'));
    test('list-nested', () => RunTestCase('list-nested'));
    test('list-continuation', () => RunTestCase('list-continuation'));

    test('table-simple', () => RunTestCase('table-simple'));
    test('table-ref', () => RunTestCase('table-ref'));

    test('tags-plain', () => RunTestCase('tags-plain'));
    test('code-tag', () => RunTestCase('code-tag'));
    test('font-tag', () => RunTestCase('font-tag'));
    test('h-tag', () => RunTestCase('h-tag'));
    test('pre-tag', () => RunTestCase('pre-tag'));
    test('quote-tag', () => RunTestCase('quote-tag'));
    test('image-tag', () => RunTestCase('image-tag'));
    test('link-tag', () => RunTestCase('link-tag'));
    test('vault-tag', () => RunTestCase('vault-tag'));
    test('quick-link-tag', () => RunTestCase('quick-link-tag'));
    test('spoiler-tag', () => RunTestCase('spoiler-tag'));
    test('youtube-tag', () => RunTestCase('youtube-tag'));
    
    test('wiki-category-tag', () => RunTestCase('wiki-category-tag'));
    test('wiki-image-tag', () => RunTestCase('wiki-image-tag'));
    test('wiki-file-tag', () => RunTestCase('wiki-file-tag'));
    test('wiki-credit-tag', () => RunTestCase('wiki-credit-tag'));
    test('wiki-book-tag', () => RunTestCase('wiki-book-tag'));
    test('wiki-archive-tag', () => RunTestCase('wiki-archive-tag'));
    test('wiki-youtube-tag', () => RunTestCase('wiki-youtube-tag'));
    test('wiki-link-tag', () => RunTestCase('wiki-link-tag'));

    test('processor-newlines', () => RunTestCase('processor-newlines'));
    test('processor-smilies-basic', () => RunTestCase('processor-smilies-basic'));
    test('processor-smilies-toomany', () => RunTestCase('processor-smilies-toomany'));
    test('processor-markdowntext', () => RunTestCase('processor-markdowntext'));
    test('processor-autolinking', () => RunTestCase('processor-autolinking'));
});

describe('Isolated tests: snarkpit', () => {
    const RunTestCase = (name: string, split = false) => RunTestCaseInFolder(ParserConfiguration.Snarkpit(), 'isolated-sp', name, split);

    test('pre-simple', () => RunTestCase('pre-simple'));
    test('pre-lang', () => RunTestCase('pre-lang'));
    test('pre-highlight', () => RunTestCase('pre-highlight'));
    test('code-tag', () => RunTestCase('code-tag'));
    test('pre-tag', () => RunTestCase('pre-tag'));
    test('align-tag', () => RunTestCase('align-tag'));
    test('size-tag', () => RunTestCase('size-tag'));
    test('color-tag', () => RunTestCase('color-tag'));
    test('list-tag', () => RunTestCase('list-tag'));
    test('wiki-image-tag', () => RunTestCase('wiki-image-tag'));
    test('processor-smilies-basic', () => RunTestCase('processor-smilies-basic'));
});

describe('End to end tests', () => {
    const RunTestCase = (name: string, split = false) => RunTestCaseInFolder(ParserConfiguration.Twhl(), 'endtoend', name, split);
    test('wikicode-page', () => RunTestCase('wikicode-page'));
});