<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Elements\QuoteElement;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\ParserConfiguration;
use PHPUnit\Framework\Attributes\DataProvider;
use PHPUnit\Framework\TestCase;

class QuoteElementTest extends TestCase
{
    private static function createParser()
    {
        $config = ParserConfiguration::twhl();
        return new Parser($config);
    }

    public static function getBalanceQuotesData()
    {
        return [
            ["[quote]Test[/quote]", "Test"],
            ["[quote]Test[/quote][quote]Test[/quote]", "Test"],
            ["[quote]Test\n[quote]Test[/quote][/quote]", "Test\n[quote]Test[/quote]"],
            ["[quote]Test\n[quote]Test[/quote]Test[/quote]", "Test\n[quote]Test[/quote]Test"],
            ["[quote]Test\n[quote]Test[/quote]Test\n[quote]Test[/quote]", null],
            ["[quote][quote]Test[/quote]Test[/quote]", "[quote]Test[/quote]Test"],
            ["[quote][quote]Test[/quote]\nTest[/quote]", "[quote]Test[/quote]\nTest"]
        ];
    }

    #[DataProvider('getBalanceQuotesData')]
    public function testBalanceQuotes($input, $output)
    {
        $lines = new Lines($input);
        $lines->next();
        $this->assertEquals($output, QuoteElement::BalanceQuotes($lines, $name, $rest));
    }

    #[DataProvider('getBalanceQuotesData')]
    public function testBalanceQuotesUpperCase($input, $output)
    {
        $lines = new Lines(strtoupper($input));
        $lines->next();
        $this->assertEquals($output ? strtoupper($output) : null, QuoteElement::BalanceQuotes($lines, $name, $rest));
    }

    public function testBalanceQuotesWithName()
    {
        $input = "[quote=Name]Test[/quote]";
        $output = "Test";
        $author = "Name";
        $lines = new Lines($input);
        $lines->next();
        $result = QuoteElement::BalanceQuotes($lines, $name, $rest);
        $this->assertEquals($output, $result);
        $this->assertEquals($author, $name);
    }

    public function testBalanceQuotesWithPostfix()
    {
        $input = "[quote]Test[/quote]ASDF";
        $output = "Test";
        $postfix = "ASDF";
        $lines = new Lines($input);
        $lines->next();
        $result = QuoteElement::BalanceQuotes($lines, $name, $rest);
        $this->assertEquals($output, $result);
        $this->assertEquals($postfix, $rest);
    }

    public function testQuoteSimple()
    {
        $input = "[quote]Test[/quote]";
        $output = "\n<blockquote>Test</blockquote>\n";
        $parser = self::createParser();
        $result = $parser->parseResult($input);
        $this->assertEquals($output, $result->toHtml());
    }
}