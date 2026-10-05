<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Util;

class MdQuoteElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = $lines->Value();
        return strlen($value) > 0 && str_starts_with($value, '>');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $value = $lines->Value();
        $arr = [Util::Trim(substr($value, 1))];
        while ($lines->Next()) {
            $value = Util::Trim($lines->Value());
            if (strlen($value) == 0 || $value[0] != '>') {
                $lines->Back();
                break;
            }
            $arr[] = Util::Trim(substr($value, 1));
        }

        $text = Util::Trim(implode("\n", $arr));
        $ret = new HtmlNode('<blockquote>', $parser->ParseElements($data, $text, $scope), '</blockquote>');
        $ret->plainBefore = "[quote]\n";
        $ret->plainAfter = "\n[/quote]";
        return $ret;
    }
}
