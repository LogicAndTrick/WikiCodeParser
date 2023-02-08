import { assert } from 'chai';
import { existsSync, readFileSync } from 'fs';
import { describe } from 'mocha';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';

function AssertSame(name : string, expected : string, actual : string, split : boolean) : void {
    if (split) {
        const expectedLines = expected.split('\n');
        const actualLines = actual.split('\n');

        for (let i = 0; i < expectedLines.length; i++) {
            const ex = expectedLines[i];
            const ac = actualLines[i];
            assert.equal(ac, ex, `[${name}] \n\nMatch failed on line ${i + 1}.\nExpected: ${ex}\nActual  : ${ac}`);
        }
    } else {
        assert.equal(actual, expected, `[${name}] Match failed.`);
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
        assert.equal(resultMeta, expectedMeta);
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

    it('missing-tag', () => RunTestCase('missing-tag'));
    it('unicode-escape', () => RunTestCase('unicode-escape'));
    
    it('ref-simple', () => RunTestCase('ref-simple'));
    
    it('pre-simple', () => RunTestCase('pre-simple'));
    it('pre-lang', () => RunTestCase('pre-lang'));
    it('pre-highlight', () => RunTestCase('pre-highlight'));

    it('mdcode-simple', () => RunTestCase('mdcode-simple'));
    it('mdcode-lang', () => RunTestCase('mdcode-lang'));

    it('heading-simple', () => RunTestCase('heading-simple'));

    it('mdline-simple', () => RunTestCase('mdline-simple'));

    it('columns-simple', () => RunTestCase('columns-simple'));
    it('columns-invalid', () => RunTestCase('columns-invalid'));

    it('panel-simple', () => RunTestCase('panel-simple'));

    it('mdquote-simple', () => RunTestCase('mdquote-simple'));
    it('mdquote-nested', () => RunTestCase('mdquote-nested'));
    
    it('list-simple', () => RunTestCase('list-simple'));
    it('list-nested', () => RunTestCase('list-nested'));
    it('list-continuation', () => RunTestCase('list-continuation'));

    it('table-simple', () => RunTestCase('table-simple'));
    it('table-ref', () => RunTestCase('table-ref'));

    it('tags-plain', () => RunTestCase('tags-plain'));
    it('code-tag', () => RunTestCase('code-tag'));
    it('font-tag', () => RunTestCase('font-tag'));
    it('h-tag', () => RunTestCase('h-tag'));
    it('pre-tag', () => RunTestCase('pre-tag'));
    it('quote-tag', () => RunTestCase('quote-tag'));
    it('image-tag', () => RunTestCase('image-tag'));
    it('link-tag', () => RunTestCase('link-tag'));
    it('vault-tag', () => RunTestCase('vault-tag'));
    it('quick-link-tag', () => RunTestCase('quick-link-tag'));
    it('spoiler-tag', () => RunTestCase('spoiler-tag'));
    it('youtube-tag', () => RunTestCase('youtube-tag'));
    
    it('wiki-category-tag', () => RunTestCase('wiki-category-tag'));
    it('wiki-image-tag', () => RunTestCase('wiki-image-tag'));
    it('wiki-file-tag', () => RunTestCase('wiki-file-tag'));
    it('wiki-credit-tag', () => RunTestCase('wiki-credit-tag'));
    it('wiki-book-tag', () => RunTestCase('wiki-book-tag'));
    it('wiki-archive-tag', () => RunTestCase('wiki-archive-tag'));
    it('wiki-youtube-tag', () => RunTestCase('wiki-youtube-tag'));
    it('wiki-link-tag', () => RunTestCase('wiki-link-tag'));

    it('processor-newlines', () => RunTestCase('processor-newlines'));
    it('processor-smilies-basic', () => RunTestCase('processor-smilies-basic'));
    it('processor-smilies-toomany', () => RunTestCase('processor-smilies-toomany'));
    it('processor-markdowntext', () => RunTestCase('processor-markdowntext'));
    it('processor-autolinking', () => RunTestCase('processor-autolinking'));
});

describe('Isolated tests: snarkpit', () => {
    const RunTestCase = (name: string, split = false) => RunTestCaseInFolder(ParserConfiguration.Snarkpit(), 'isolated-sp', name, split);

    it('pre-simple', () => RunTestCase('pre-simple'));
    it('pre-lang', () => RunTestCase('pre-lang'));
    it('pre-highlight', () => RunTestCase('pre-highlight'));
    it('code-tag', () => RunTestCase('code-tag'));
    it('pre-tag', () => RunTestCase('pre-tag'));
    it('align-tag', () => RunTestCase('align-tag'));
    it('size-tag', () => RunTestCase('size-tag'));
    it('color-tag', () => RunTestCase('color-tag'));
    it('list-tag', () => RunTestCase('list-tag'));
    it('wiki-image-tag', () => RunTestCase('wiki-image-tag'));
    it('processor-smilies-basic', () => RunTestCase('processor-smilies-basic'));
});

describe('End to end tests', () => {
    const RunTestCase = (name: string, split = false) => RunTestCaseInFolder(ParserConfiguration.Twhl(), 'endtoend', name, split);
    it('wikicode-page', () => RunTestCase('wikicode-page'));
});