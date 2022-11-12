<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Elements\PreElement;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class PreTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'pre';
        $this->element = 'pre';
        $this->isBlock = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        $before .= '><code>';
        $after = '</code></' . $this->element . '>';

        $arr = explode("\n", $text);

        // Trim blank lines from the start and end of the array
        for ($i = 0; $i < 2; $i++) {
            while (count($arr) > 0 && trim($arr[0]) == '') array_splice($arr, 0, 1);
            $arr = array_reverse($arr);
        }

        $arr = PreElement::FixCodeIndentation($arr);
        $text = implode("\n", $arr);

        $ret = new HtmlNode($before, new UnprocessablePlainTextNode($text), $after);
        $ret->isBlockNode = true;
        return $ret;
    }
}