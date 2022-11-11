<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class MdCodeElement extends Element
{

    public function __construct()
    {
        $this->priority = 10;
    }

    public function Matches(Lines $lines): bool
    {
        $value = $lines->Value();
        return str_starts_with($value, '```');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();
        $firstLine = rtrim(substr($lines->Value(), 3));

        $lang = null;
        if (in_array(strtolower($firstLine), PreElement::$allowedLanguages)) {
            $lang = $firstLine;
            $firstLine = '';
        }

        $arr = [$firstLine];

        $found = false;
        while ($lines->Next()) {
            $value = rtrim($lines->Value());
            if (str_ends_with($value, '```')) {
                $lastLine = substr($value, 0, strlen($value) - 3);
                $arr[] = $lastLine;
                $found = true;
                break;
            } else {
                $arr[] = $value;
            }
        }

        if (!$found) {
            $lines->SetCurrent($current);
            return null;
        }

        // Trim blank lines from the start and end of the array
        for ($i = 0; $i < 2; $i++) {
            while (count($arr) > 0 && trim($arr[0]) == '') array_splice($arr, 0, 1);
            $arr = array_reverse($arr);
        }

        // Replace all tabs with 4 spaces
        $arr = array_map(fn(string $x) => str_replace("\t", '    ', $x), $arr);

        // Find the longest common whitespace amongst all lines (ignore blank lines)
        $longestWhitespace = array_reduce($arr, function (int $c, string $i) {
            if (strlen(trim($i)) == 0) return $c;
            $wht = strlen($i) - strlen(ltrim($i));
            return min($wht, $c);
        }, 9999);

        // Dedent all lines by the longest common whitespace
        $arr = array_map(fn($a) => substr($a, min($longestWhitespace, strlen($a))), $arr);

        $plain = new UnprocessablePlainTextNode(implode("\n", $arr));
        $cls = !$lang || trim($lang) == '' ? '' : " class=\"lang-$lang\"";
        $before = "<pre${cls}><code>";
        $after = '</code></pre>';
        return new HtmlNode($before, $plain, $after);
    }
}