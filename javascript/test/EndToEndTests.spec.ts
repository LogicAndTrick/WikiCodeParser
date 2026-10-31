import { describe, expect, test } from '@jest/globals';
import { existsSync, readdirSync, readFileSync } from 'fs';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';
import { sanitise } from './Util';

function AssertSame(name: string, expected: string, actual: string, split: boolean): void {
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

function Test(config: ParserConfiguration, input: string, expectedOutput: string, expectedPlain: string | undefined, expectedMeta: string | undefined, split = false): void {
    const parser = new Parser(config);

    const result = parser.ParseResult(input);
    const resultHtml = result.ToHtml(sanitise).trim();
    const resultPlain = result.ToPlainText().trim();
    const resultMeta = result
        .GetMetadata()
        .map(x => `${x.Key}=${JSON.stringify(x.Value)}`)
        .join('\n');
    expectedOutput = sanitise(expectedOutput).trim();

    AssertSame('html', expectedOutput, resultHtml, split);
    if (expectedPlain !== undefined) AssertSame('plain', expectedPlain, resultPlain, split);

    if (expectedMeta) {
        expect(resultMeta).toBe(expectedMeta);
    }
}

function GetTestCaseDirectory(folder: string): string {
    return `${__dirname}/../../tests/${folder}`;
}

function GetTestCaseNames(folder: string): string[] {
    const dir = GetTestCaseDirectory(folder);
    return readdirSync(dir).filter(x => !x.startsWith('ignored'));
}

function RunTestCase(config: ParserConfiguration, folder: string, name: string, split = false) {
    const dir = GetTestCaseDirectory(folder);
    let _in: string;
    let _out: string;
    let _plain: string | undefined = undefined;
    let _meta: string | undefined = undefined;
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
    test.each(GetTestCaseNames('isolated'))('Isolated: %p', name => RunTestCase(ParserConfiguration.Twhl(), 'isolated', name));
});

describe('Isolated tests: snarkpit', () => {
    test.each(GetTestCaseNames('isolated-sp'))('Isolated-SP: %p', name => RunTestCase(ParserConfiguration.Snarkpit(), 'isolated-sp', name));
});

describe('End to end tests', () => {
    test('wikicode-page', () => RunTestCase(ParserConfiguration.Twhl(), 'endtoend', 'wikicode-page'));
});
