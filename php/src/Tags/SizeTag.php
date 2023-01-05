<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class SizeTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'size';
        $this->element = 'span';
        $this->mainOption = 'size';
        $this->options = ['size'];
        $this->allOptionsInMain = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        if (isset($options['size'])) {
            $before .= ' style="';
            if (isset($options['size']) && FontTag::IsValidSize($options['size'])) $before .= 'font-size: ' . $options['size'] . 'px; ';
            $before = rtrim($before) . '"';
        }
        $before .= '>';
        $content = $parser->ParseTags($data, $text, $scope, $this->TagContext());
        $after = '</' . $this->element . '>';
        return new HtmlNode($before, $content, $after);
    }
}