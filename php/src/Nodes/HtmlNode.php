<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class HtmlNode implements INode
{
    public string $htmlBefore;
    public INode $content;
    public string $htmlAfter;

    public string $plainBefore;
    public string $plainAfter;
    public bool $isBlockNode;

    public function __construct(string $htmlBefore, INode $content, string $htmlAfter)
    {
        $this->htmlBefore = $htmlBefore;
        $this->content = $content;
        $this->htmlAfter = $htmlAfter;
        $this->plainBefore = $this->plainAfter = '';
        $this->isBlockNode = false;
    }


    function ToHtml(): string
    {
        return $this->htmlBefore . $this->content->ToHtml() . $this->htmlAfter;
    }

    function ToPlainText(): string
    {
        return $this->plainBefore . $this->content->ToPlainText() . $this->plainAfter;
    }

    function GetChildren(): array
    {
        return  [ $this->content ];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i !== 0) throw new \InvalidArgumentException("Index out of range");
        $this->content = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}
