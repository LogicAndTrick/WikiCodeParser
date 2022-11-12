<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\Assert;

class TestCaseUtils
{
    private static function Test(string $input, string $expectedOutput, ?string $expectedMeta, bool $split = false) : void {
        $config = ParserConfiguration::Default();
        $parser = new Parser($config);

        $result = $parser->ParseResult($input);
        $resultHtml = trim($result->ToHtml());

        if ($split) {
            $expectedLines = explode("\n", $expectedOutput);
            $actualLines = explode("\n", $resultHtml);

            for ($i = 0; $i < count($expectedLines); $i++) {
                $ex = $expectedLines[$i];
                $ac = $actualLines[$i];
                $line = $i + 1;
                Assert::assertEquals($ex, $ac, "Match failed on line $line.\nExpected: $ex\nActual  : $ac");
            }
        } else {
            Assert::assertEquals($expectedOutput, $resultHtml);
        }

        if ($expectedMeta != null) {
            throw new \Exception("not implemented yet");
        }
    }

    private static function GetTestCaseDirectory(string $folder) : string {
        return __DIR__.'/../../tests/'.$folder;
    }

    public static function RunTestCase(string $folder, string $name, bool $split = false) : void {
        $dir = TestCaseUtils::GetTestCaseDirectory($folder);
        $_in = '';
        $_out = '';
        $_meta = null;
        if (file_exists("$dir/$name")) {
            $text = file_get_contents("$dir/$name");
            $spl = array_map(fn($x) => trim($x), explode('###', $text));
            $_in = $spl[0];
            $_out = $spl[1];
            if (count($spl) > 2) $_meta = $spl[2];
        } else {
            $_in = file_get_contents("$dir/$name.in");
            $_out = file_get_contents("$dir/$name.out");
            if (file_exists("$dir/$name.meta")) {
                $_meta = file_get_contents("$dir/$name.meta");
            }
        }

        $_in = str_replace("\r", '', $_in);
        $_out = str_replace("\r", '', $_out);
        $_meta = str_replace("\r", '', $_meta);
        TestCaseUtils::Test($_in, $_out, $_meta, $split);
    }
}
