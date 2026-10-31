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

    public function GetMetadata(): array
    {
        $list = [];
        NodeExtensions::Walk($this->content, function (INode $n) use (&$list) {
            if ($n instanceof MetadataNode) $list[] = ['key' => $n->key, 'value' => $n->value];
            return true;
        });
        return $list;
    }

    /**
     * Convert the result to HTML and sanitise it with a given callback.
     * Supplying a valid sanitiser is the responsibility of the caller.
     * @param callable(string): string $sanitiser
     */
    public function ToHtml(callable $sanitiser): string
    {
        return $sanitiser($this->content->ToHtml());
    }

    /**
     * Convert the result to HTML without a sanitiser callback.
     * Its possible malicious input could include injection attacks. Use at your own risk.
     */
    public function ToHtmlUnsanitised(): string
    {
        return $this->content->ToHtml();
    }

    /**
     * Convert the result to plain text.
     */
    public function ToPlainText(): string
    {
        return $this->content->ToPlainText();
    }
}
