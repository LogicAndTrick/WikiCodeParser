<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

use LogicAndTrick\WikiCodeParser\ParseData;

class RefNode implements INode
{
    public ParseData $data;
    public string $name;

    public function __construct(ParseData $data, string $name)
    {
        $this->data = $data;
        $this->name = $name;
    }

    private function GetNode() : INode {
        return $this->data->Get("Ref::{$this->name}", fn() => UnprocessablePlainTextNode::Empty());
    }

    function ToHtml(): string
    {
        return $this->GetNode()->ToHtml();
    }

    function ToPlainText(): string
    {
        return $this->GetNode()->ToHtml();
    }

    function GetChildren(): array
    {
        return [$this->GetNode()];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i != 0) throw new \Exception('Index out of range');
        $this->data->Set("Ref::{$this->name}", $node);
    }

    function HasContent(): bool
    {
        return $this->GetNode()->HasContent();
    }
}