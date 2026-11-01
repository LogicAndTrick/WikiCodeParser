import { describe, expect, test } from '@jest/globals';
import { QuoteElement } from '../src/Elements/QuoteElement';
import { Lines } from '../src/Lines';
import { Parser } from '../src/Parser';
import { ParserConfiguration } from '../src/ParserConfiguration';
import { sanitise } from './Util';

describe('QuoteElementTest', () => {
    function createParser(): Parser {
        const config = ParserConfiguration.Twhl();
        return new Parser(config);
    }

    function* getBalanceQuotesData(): IterableIterator<[string, string | null]> {
        yield ['[quote]Test[/quote]', 'Test'];
        yield ['[quote]Test[/quote][quote]Test[/quote]', 'Test'];
        yield ['[quote]Test\n[quote]Test[/quote][/quote]', 'Test\n[quote]Test[/quote]'];
        yield ['[quote]Test\n[quote]Test[/quote]Test[/quote]', 'Test\n[quote]Test[/quote]Test'];
        yield ['[quote]Test\n[quote]Test[/quote]Test\n[quote]Test[/quote]', null];
        yield ['[quote][quote]Test[/quote]Test[/quote]', '[quote]Test[/quote]Test'];
        yield ['[quote][quote]Test[/quote]\nTest[/quote]', '[quote]Test[/quote]\nTest'];
    }

    const data = Array.from(getBalanceQuotesData());

    test.each(data)('BalanceQuotesTest (%s)', (input, output) => {
        const lines = new Lines(input);
        lines.Next();
        const { text } = QuoteElement.BalanceQuotes(lines);
        expect(text).toBe(output);
    });

    test.each(data)('BalanceQuotesUpperCaseTest (%s)', (input, output) => {
        const lines = new Lines(input.toUpperCase());
        lines.Next();
        const { text } = QuoteElement.BalanceQuotes(lines);
        expect(text).toBe(output === null ? null : output.toUpperCase());
    });

    test('BalanceQuotesWithNameTest', () => {
        const input = '[quote=Name]Test[/quote]';
        const output = 'Test';
        const author = 'Name';
        const lines = new Lines(input);
        lines.Next();
        const { text: result, author: name } = QuoteElement.BalanceQuotes(lines);
        expect(result).toBe(output);
        expect(name).toBe(author);
    });

    test('BalanceQuotesWithPostfixTest', () => {
        const input = '[quote]Test[/quote]ASDF';
        const output = 'Test';
        const postfix = 'ASDF';
        const lines = new Lines(input);
        lines.Next();
        const { text: result, postfix: rest } = QuoteElement.BalanceQuotes(lines);
        expect(result).toBe(output);
        expect(rest).toBe(postfix);
    });

    test('QuoteSimple', () => {
        const input = '[quote]Test[/quote]';
        const output = sanitise('\n<blockquote>Test</blockquote>\n');
        const parser = createParser();
        const result = parser.ParseResult(input);
        expect(result.ToHtml(sanitise)).toBe(output);
    });
});
