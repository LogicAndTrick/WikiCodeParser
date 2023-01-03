<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

require_once __DIR__.'./TestCaseUtils.php';

class IsolatedSnarkpitTest extends TestCase
{
    public function testpresimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'pre-simple'); }
    public function testprelang() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'pre-lang'); }
    public function testprehighlight() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'pre-highlight'); }
    public function testcodetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'code-tag'); }
    public function testpretag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'pre-tag'); }
    public function testaligntag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'align-tag'); }
    public function testsizetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'size-tag'); }
    public function testcolortag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'color-tag'); }
    public function testlisttag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'list-tag'); }
    public function testwikiimagetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(),'isolated-sp', 'wiki-image-tag'); }
}
