<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\Assert;

class TestCaseUtils
{
    public static function AssertSame(string $name, string $expected, string $actual, bool $split) : void
    {
        if ($split) {
            $expectedLines = explode("\n", $expected);
            $actualLines = explode("\n", $actual);

            for ($i = 0; $i < count($expectedLines); $i++) {
                $ex = $expectedLines[$i];
                $ac = $actualLines[$i];
                $line = $i + 1;
                Assert::assertEquals($ex, $ac, "[$name] Match failed on line $line.\nExpected: $ex\nActual  : $ac");
            }
        } else {
            Assert::assertEquals($expected, $actual, "[$name] Match failed.");
        }
    }

    private static function Test(ParserConfiguration $config, string $input, string $expectedOutput, ?string $expectedPlain, ?string $expectedMeta, bool $split = false) : void {
        $parser = new Parser($config);

        $result = $parser->ParseResult($input);
        $resultHtml = trim($result->ToHtml());
        $resultPlain = trim($result->ToPlainText());

        self::AssertSame('html', $expectedOutput, $resultHtml, $split);
        if ($expectedPlain !== null) self::AssertSame('plain', $expectedPlain, $resultPlain, $split);

        if ($expectedMeta != null) {
            $resultMetaObjects = $result->GetMetadata();
            $expectedMetaObjects = array_map(function(string $x) {
                $x = trim($x);
                $spl = explode('=', $x, 2);
                return [ 'key' => $spl[0], 'value' => json_decode($spl[1]) ];
            }, explode("\n", $expectedMeta));
            Assert::assertEquals(count($expectedMetaObjects), count($resultMetaObjects));
            for ($i = 0; $i < count($resultMetaObjects); $i++)
            {
                $rmo = $resultMetaObjects[$i];
                $emo = $expectedMetaObjects[$i];
                Assert::assertEquals($emo['key'], $rmo['key']);
                if ($emo['value'] == $rmo['value']) continue;
                $robj = $rmo['value'];
                $eobj = $emo['value'];
                foreach ($eobj as $name => $ev) {
                    $rv = $robj->$name;
                    Assert::assertEquals($ev, $rv);
                }
            }
        }
    }

    private static function GetTestCaseDirectory(string $folder) : string {
        return __DIR__.'/../../tests/'.$folder;
    }

    public static function RunTestCase(ParserConfiguration $config, string $folder, string $name, bool $split = false) : void {
        $dir = TestCaseUtils::GetTestCaseDirectory($folder);
        $_in = '';
        $_out = '';
        $_plain = null;
        $_meta = null;
        if (file_exists("$dir/$name")) {
            $text = file_get_contents("$dir/$name");
            $spl = array_map(fn($x) => trim($x), explode('###', $text));
            $_in = $spl[0];
            $_out = $spl[1];
            if (count($spl) > 2) $_plain = $spl[2];
            if (count($spl) > 3) $_meta = $spl[3];
        } else {
            $_in = file_get_contents("$dir/$name.in");
            $_out = file_get_contents("$dir/$name.out");
            if (file_exists("$dir/$name.plain")) {
                $_plain = file_get_contents("$dir/$name.plain");
            }
            if (file_exists("$dir/$name.meta")) {
                $_meta = file_get_contents("$dir/$name.meta");
            }
        }

        $_in = str_replace("\r", '', $_in);
        $_out = str_replace("\r", '', $_out);
        $_plain = $_plain ? str_replace("\r", '', $_plain) : $_plain;
        $_meta = $_meta ? str_replace("\r", '', $_meta) : $_meta;
        TestCaseUtils::Test($config, $_in, $_out, $_plain, $_meta, $split);
    }
}
