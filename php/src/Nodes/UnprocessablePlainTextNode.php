<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Position;

class UnprocessablePlainTextNode implements INode
{
    public static function Empty(Position $pos): INode
    {
        return new UnprocessablePlainTextNode($pos, '');
    }

    public static function NewLine(Position $pos): INode
    {
        return new UnprocessablePlainTextNode($pos, "\n");
    }

    public int $line = 0;
    public int $column = 0;
    public string $text;

    public function __construct(Position $pos, string $text)
    {
        $this->line = $pos->line;
        $this->column = $pos->column;
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
