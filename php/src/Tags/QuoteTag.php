<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class QuoteTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'quote';
        $this->element = 'blockquote';
        $this->mainOption = 'name';
        $this->options = ['name'];
        $this->allOptionsInMain = true;
        $this->isBlock = true;
        $this->isNested = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        $before .= '>';
        if (isset($options['name'])) {
            $before .= '<strong class="quote-name">' . $options['name'] . ' said:</strong><br/>';
        }
        $after = '</' . $this->element . '>';
        $content = $parser->ParseTags($data, trim($text), $scope, $this->TagContext());
        $ret = new HtmlNode($before, $content, $after);
        $ret->plainBefore = (isset($options['name']) ? $options['name'] . ' said: ' : '') . "[quote]\n";
        $ret->plainAfter = "\n[/quote]";
        $ret->isBlockNode = $this->isBlock;
        return $ret;
    }
}