<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

require_once __DIR__ . '/TestCaseUtils.php';

class EndToEndTest extends TestCase
{
    // @formatter:off
    public function testwikicodepage() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'endtoend', 'wikicode-page'); }
}
