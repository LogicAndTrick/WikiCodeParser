<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\NodeExtensions;

class ParseResult
{
    public INode $content;

    public function __construct()
    {
        $this->content = new NodeCollection();
    }

    /** @noinspection PhpArrayUsedOnlyForWriteInspection */
    public function GetMetadata(): array
    {
        $list = [];
        NodeExtensions::Walk($this->content, function (INode $n) use ($list) {
            if ($n instanceof MetadataNode) $list[] = ['key' => $n->key, 'value' => $n->value];
            return true;
        });
        return $list;
    }

    public function ToHtml(): string
    {
        return $this->content->ToHtml();
    }

    public function ToPlainText(): string
    {
        return $this->content->ToPlainText();
    }
}
