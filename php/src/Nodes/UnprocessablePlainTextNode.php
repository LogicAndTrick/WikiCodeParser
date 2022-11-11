<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

use LogicAndTrick\WikiCodeParser\HtmlHelper;

class UnprocessablePlainTextNode implements INode
{
    public static function Empty(): INode
    {
        return new UnprocessablePlainTextNode('');
    }

    public static function NewLine(): INode
    {
        return new UnprocessablePlainTextNode("\n");
    }

    public string $text;

    public function __construct(string $text)
    {
        $this->text = $text;
    }

    function ToHtml(): string
    {
        return HtmlHelper::Encode($this->text);
    }

    function ToPlainText(): string
    {
        return $this->text;
    }

    function GetChildren(): array
    {
        return [];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        throw new \Exception('Invalid operation');
    }

    function HasContent(): bool
    {
        return $this->text && trim($this->text) != '';
    }
}