<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

/** @noinspection PhpMultipleClassesDeclarationsInOneFile */
class SpoilerNode implements INode
{
    private string $visibleText;
    private INode $spoilerContent;

    public function __construct(string $visibleText, INode $spoilerContent)
    {
        $this->visibleText = $visibleText;
        $this->spoilerContent = $spoilerContent;
    }

    function ToHtml(): string
    {
        return $this->spoilerContent->ToHtml();
    }

    function ToPlainText(): string
    {
        return "[$this->visibleText](spoiler text)";
    }

    function GetChildren(): array
    {
        return [$this->spoilerContent];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i != 0) throw new \Exception('Argument out of range');
        $this->spoilerContent = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}

class SpoilerTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'spoiler';
        $this->element = 'span';
        $this->elementClass = 'spoiler';
        $this->mainOption = 'text';
        $this->options = ['text'];
        $this->allOptionsInMain = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $visibleText = 'Spoiler';
        if (isset($options['text']) && strlen($options['text']) > 0) $visibleText = $options['text'];

        $before = "<{$this->element}";
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        $before .= " title=\"{$visibleText}\">";
        $after = "</{$this->element}>";
        return new HtmlNode($before, new SpoilerNode($visibleText, $parser->ParseTags($data, $text, $scope, $this->TagContext())), $after);
    }
}