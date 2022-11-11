<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Colours;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class PreElement extends Element
{
    /**
     * @var string[]
     */
    public static array $allowedLanguages = [
        'php', 'dos', 'bat', 'cmd', 'css', 'cpp', 'c', 'c++', 'cs', 'ini', 'json', 'xml', 'html', 'angelscript',
        'javascript', 'js', 'plaintext'
    ];

    public function Matches(Lines $lines): bool
    {
        $value = trim($lines->Value());
        return strlen($value) > 4 && str_starts_with($value, '[pre') && preg_match('/\[pre(=[a-z ]+)?]/i', $value);
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();
        $arr = [];

        $line = trim($lines->Value());
        $success = preg_match('/\[pre(?:=([a-z0-9 ]+))?]/i', $line, $res);
        if (!$success) {
            $lines->SetCurrent($current);
            return null;
        }

        $line = substr($line, strlen($res[0]));

        $lang = null;
        $hl = false;
        if (isset($res[1])) {
            $spl = explode(' ', $res[1]);
            $hl = in_array('highlight', $spl);
            $lang = strtolower(array_filter($spl, fn(string $x) => $x != 'highlight')[0]);
            if (!in_array($lang, PreElement::$allowedLanguages)) $lang = null;
        }

        if (str_ends_with($line, '[/pre]')) {
            $arr[] = substr($line, 0, strlen($line) - 6);
        } else {
            if (strlen($line) > 0) $arr[] = $line;
            $found = false;
            while ($lines->Next()) {
                $value = rtrim($lines->Value());
                if (str_ends_with($value, '[/pre]')) {
                    $lastLine = substr($value, 0, strlen($value) - 6);
                    $arr[] = $lastLine;
                    $found = true;
                    break;
                } else {
                    $arr[]  = $value;
                }
            }

            if (!$found || count($arr) == 0) {
                $lines->SetCurrent($current);
                return null;
            }
        }

        // Trim blank lines from the start and end of the array
        for ($i = 0; $i < 2; $i++) {
            while (count($arr) > 0 && trim($arr[0]) == '') array_splice($arr, 0, 1);
            $arr = array_reverse($arr);
        }

        // Process highlight commands
        $highlight = [];
        if ($hl) {
            // Highlight commands get their own line so we need to keep track of which lines we're removing as we go
            $newArr = [];
            $firstLine = 0;
            foreach ($arr as $srcLine) {
                if (str_starts_with($srcLine, '@@')) {
                    $success = preg_match('/^@@(?:(#[0-9a-f]{3}|#[0-9a-f]{6}|[a-z]+|\d+)(?::(\d+))?)?$/im', $srcLine, $match);
                    if ($success) {
                        $numLines = 1;
                        $color = '#FF8000';
                        for ($i = 1; $i < count($match); $i++) {
                            $p = $match[$i];
                            if (Colours::IsValidColor($p)) $color = $p;
                            else if (intval($p, 10)) $numLines = intval($p, 10);
                        }
                        $highlight[] = [ 'firstLine' => $firstLine, 'numLines' => $numLines, 'color' => $color ];
                        continue;
                    }
                }
                $firstLine++;
                $newArr[] = $srcLine;
            }
            $arr = $newArr;

            // Make sure highlights don't overlap each other or go past the end of the block
            $highlight[] = [ 'firstLine' => count($arr), 'numLines' => 0, 'color' => '' ];
            for ($i = 0; $i < count($highlight) - 1; $i++) {
                $currFirst = $highlight[$i]['firstLine'];
                $currNum = $highlight[$i]['numLines'];
                $currCol = $highlight[$i]['color'];
                $nextFirst = $highlight[$i + 1]['firstLine'];
                $lastLine = $currFirst + $currNum - 1;
                if ($lastLine >= $nextFirst) $highlight[$i] = [ 'firstLine' => $currFirst, 'numLines' => $nextFirst - $currFirst, 'color' => $currCol ];
            }
            $highlight = array_filter($highlight, fn($x) => $x['numLines'] > 0);
        }

        $arr = PreElement::FixCodeIndentation($arr);

        $highlights = implode('', array_map(fn($h) => "<div class=\"line-highlight\" style=\"top: ${h['firstLine']}em; height: ${h['numLines']}em; background: ${h['color']};\"></div>", $highlight));
        $plain = new UnprocessablePlainTextNode(implode("\n", $arr));
        $cls = !$lang || trim($lang) == '' ? '' : " class=\"lang-$lang\"";
        $before = "<pre$cls><code>$highlights";
        $after = '</code></pre>';
        return new HtmlNode($before, $plain, $after);
    }

    public static function FixCodeIndentation(array $arr) : array {
        // Replace all tabs with 4 spaces
        $arr = array_map(fn(string $x) => str_replace("\t", '    ', $x), $arr);

        // Find the longest common whitespace amongst all lines (ignore blank lines)
        $longestWhitespace = array_reduce($arr, function (int $c, string $i) {
            if (strlen(trim($i)) == 0) return $c;
            $wht = strlen($i) - strlen(ltrim($i));
            return min($wht, $c);
        }, 9999);

        // Dedent all lines by the longest common whitespace
        return array_map(fn($a) => substr($a, min($longestWhitespace, strlen($a))), $arr);
    }
}