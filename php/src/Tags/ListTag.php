<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNodeBuilder;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Position;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\Util;

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
            array_map(fn(string $x) => Util::Trim($x), explode('[*]', $text)),
            fn(string $x) => $x && strlen($x) > 0
        );
        return parent::Validate($options, $text) && count($items) > 0;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text, Position $tagPosition, Position $textPosition): INode|null
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        $before .= ">\n";

        $content = new NodeCollection();

        $itemState = new State($textPosition->line, $textPosition->column, $text);
        $itemPos = $textPosition;
        while (!$itemState->Done) {
            $plainPos = $itemState->sourcePosition;
            $plain = $itemState->ScanTo("[*]");

            $itemNode = $this->CreateListItemNode($parser, $data, $itemState, $scope, $plain, $plainPos, $itemPos);
            if ($itemNode != null) $content->nodes[] = $itemNode;

            $itemPos = $itemState->sourcePosition;
            $itemState->Seek(3, false); // read the [*]
        }

        $after = '</' . $this->element . '>';
        $ret = new HtmlNode($tagPosition, $before, $content, $after);
        $ret->isBlockNode = true;
        return $ret;
    }

    private function CreateListItemNode(Parser $parser, ParseData $data, State $state, string $scope, string $text, Position $textPosition, Position $tagPosition): HtmlNode|null
    {
        $text = Util::Trim($text);
        if (strlen($text) == 0) return null;

        return HtmlNodeBuilder::Make("li", $tagPosition)
            ->Add($parser->ParseTags($data, $text, $scope, $this->TagContext(), $textPosition->line, $textPosition->column))
            ->ToNode(plainBefore: "* ", plainAfter: "\n", htmlAfter: "\n");
    }
}
