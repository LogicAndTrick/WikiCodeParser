<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\Attributes\DataProvider;
use PHPUnit\Framework\TestCase;

require_once __DIR__ . '/TestCaseUtils.php';

class IsolatedTest extends TestCase
{
    public static function getTestCases(): array
    {
        $tests = TestCaseUtils::GetTestCaseNames('isolated');
        $arr = [];
        foreach ($tests as $t) {
            $arr[$t] = [$t];
        }
        return $arr;
    }

    #[DataProvider('getTestCases')]
    public function testIsolated(string $name)
    {
        TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', $name);
    }

    public static function getSnarkpitTestCases(): array
    {
        $tests = TestCaseUtils::GetTestCaseNames('isolated-sp');
        $arr = [];
        foreach ($tests as $t) {
            $arr[$t] = [$t];
        }
        return $arr;
    }

    #[DataProvider('getSnarkpitTestCases')]
    public function testIsolatedSnarkpit(string $name)
    {
        TestCaseUtils::RunTestCase(ParserConfiguration::Snarkpit(), 'isolated-sp', $name);
    }
}
