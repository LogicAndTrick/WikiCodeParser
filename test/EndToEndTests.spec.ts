import { assert } from 'chai';
import { existsSync, readFileSync } from 'fs';
import { describe } from 'mocha';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';

function Test(input: string, expectedOutput: string, split = false): void {
    const config = ParserConfiguration.Default();
    const parser = new Parser(config);

    const result = parser.ParseResult(input);
    const resultHtml = result.ToHtml();

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
}

function RunTestCase(name: string, split = false) {
    let _in : string;
    let _out : string;
    if (existsSync(`${__dirname}/cases/${name}`)) {
        const text = readFileSync(`${__dirname}/cases/${name}`, 'utf-8');
        [_in, _out] = text.split('###').map(x => x.trim());
    } else {
        _in = readFileSync(`${__dirname}/cases/${name}.in`, 'utf-8');
        _out = readFileSync(`${__dirname}/cases/${name}.out`, 'utf-8');
    }
    _in = _in.replace(/\r/g, '');
    _out = _out.replace(/\r/g, '');
    Test(_in, _out, split);
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

    it('columns-simple', () => RunTestCase('columns-simple', true));
    it('columns-invalid', () => RunTestCase('columns-invalid'));

    it('processor-newlines', () => RunTestCase('processor-newlines'));
});

describe('End to end tests', () => {
    //it('wikicode-page', () => RunTestCase('wikicode-page', true));
});