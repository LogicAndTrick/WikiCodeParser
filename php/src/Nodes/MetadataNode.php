<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class MetadataNode implements INode
{
    public string $key;
    public mixed $value;

    public function __construct(string $key, mixed $value)
    {
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
