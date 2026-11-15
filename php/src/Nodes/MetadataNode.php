<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

use LogicAndTrick\WikiCodeParser\Position;

class MetadataNode implements INode
{
    public int $line;
    public int $column;
    public string $key;
    public mixed $value;

    public function __construct(Position $pos, string $key, mixed $value)
    {
        $this->line = $pos->line;
        $this->column = $pos->column;
        $this->key = $key;
        $this->value = $value;
    }

    function ToHtml(): string
    {
        return '';
    }

    function ToPlainText(): string
    {
        return '';
    }

    /**
     * @inheritDoc
     */
    function GetChildren(): array
    {
        return [];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        throw new \BadMethodCallException();
    }

    function HasContent(): bool
    {
        return false;
    }
}
