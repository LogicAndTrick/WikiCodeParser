<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

require_once __DIR__.'./TestCaseUtils.php';

class EndToEndTest extends TestCase
{
    public function testwikicodepage() { TestCaseUtils::RunTestCase('endtoend', 'wikicode-page'); }
}
