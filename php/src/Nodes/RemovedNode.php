<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class RemovedNode implements INode
{
    public INode $originalNode;

    public function __construct(INode $originalNode)
    {
        $this->originalNode = $originalNode;
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