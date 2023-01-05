<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class ListTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'list';
        $this->element = 'ul';
        $this->isBlock = true;
    }

    public function Validate(array $options, string $text): bool
    {
        $items = array_filter(
            array_map(fn(string $x) => trim($x), explode('[*]', $text)),
            fn(string $x) => $x && strlen($x) > 0
        );
        return parent::Validate($options, $text) && count($items) > 0;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        $before .= ">\n";

        $content = new NodeCollection();
        $items = array_filter(
            array_map(fn(string $x) => trim($x), explode('[*]', $text)),
            fn(string $x) => $x && strlen($x) > 0
        );
        foreach ($items as $item) {
            $node = new HtmlNode('<li>', $parser->ParseTags($data, $item, $scope, $this->TagContext()), "</li>\n");
            $node->plainBefore = '* ';
            $node->plainAfter = "\n";
            $content->nodes[] = $node;
        }

        $after = '</' . $this->element . '>';
        $ret = new HtmlNode($before, $content, $after);
        $ret->isBlockNode = true;
        return $ret;
    }

}