<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class MdLineElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = rtrim($lines->Value());
        return strlen($value) >= 3 && $value == str_repeat('-', strlen($value));
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $ret = new HtmlNode('<hr />', PlainTextNode::Empty(), '');
        $ret->plainBefore = '---';
        return $ret;
    }
}