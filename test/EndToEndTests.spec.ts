import { assert } from 'chai';
import { existsSync, readFileSync } from 'fs';
import { describe } from 'mocha';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';

function Test(input: string, expectedOutput: string, expectedMeta : string | undefined, split = false): void {
    const config = ParserConfiguration.Default();
    const parser = new Parser(config);

    const result = parser.ParseResult(input);
    const resultHtml = result.ToHtml().trim();
    const resultMeta = result.GetMetadata().map(x => `${x.Key}=${JSON.stringify(x.Value)}`).join('\n');

    if (split) {
        const expectedLines = expectedOutput.split('\n');
        const actualLines = resultHtml.split('\n');

        for (let i = 0; i < expectedLines.length; i++) {
            const ex = expectedLines[i];
            const ac = actualLines[i];
            assert.equal(ac, ex, `\n\nMatch failed on line ${i + 1}.\nExpected: ${ex}\nActual  : ${ac}`);
        }
    } else {
        assert.equal(resultHtml, expectedOutput);
    }

    if (expectedMeta) {
        assert.equal(resultMeta, expectedMeta);
    }
}

function RunTestCase(name: string, split = false) {
    let _in : string;
    let _out : string;
    let _meta : string | undefined = undefined;
    if (existsSync(`${__dirname}/cases/${name}`)) {
        const text = readFileSync(`${__dirname}/cases/${name}`, 'utf-8');
        [_in, _out, _meta] = text.split('###').map(x => x.trim());
    } else {
        _in = readFileSync(`${__dirname}/cases/${name}.in`, 'utf-8');
        _out = readFileSync(`${__dirname}/cases/${name}.out`, 'utf-8');
        if (existsSync(`${__dirname}/cases/${name}.meta`)) {
            _meta = readFileSync(`${__dirname}/cases/${name}.meta`, 'utf-8');
        }
    }
    _in = _in.replace(/\r/g, '');
    _out = _out.replace(/\r/g, '');
    _meta = _meta?.replace(/\r/g, '');
    Test(_in, _out, _meta, split);
}

describe('Isolated tests', () => {
    it('missing-tag', () => RunTestCase('missing-tag'));
    
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
});

describe('End to end tests', () => {
    //it('wikicode-page', () => RunTestCase('wikicode-page', true));
});