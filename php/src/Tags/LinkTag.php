<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Position;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\Util;

class LinkTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'url';
        $this->element = 'a';
        $this->mainOption = 'url';
        $this->options = ['url'];
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text, Position $tagPosition, Position $textPosition): INode|null
    {
        $url = HtmlHelper::AttributeEncode($this->BuildUrl($options, $text));

        $classes = [];
        if ($this->elementClass != null) $classes[] = $this->elementClass;

        $element = $this->element;
        $before = "<$element " . (count($classes) > 0 ? 'class="' . implode(' ', $classes) . '" ' : '') . 'href="' . $url . '">';
        $after = "</$element>";

        $content = isset($options['url'])
            ? $parser->ParseTags($data, $text, $scope, $this->TagContext(), $textPosition->line, $textPosition->column)
            : new UnprocessablePlainTextNode($textPosition, $text);
        return new HtmlNode($tagPosition, $before, $content, $after);
    }

    public function Validate(array $options, string $text): bool
    {
        if (isset($options['url']) && Util::Trim($options['url']) === '') return false;
        $url = $this->BuildUrl($options, $text);
        return HtmlHelper::ValidateUrl($url) && preg_match('/^[^\]"\n ]+$/i', $url);
    }

    private function BuildUrl(array $options, string $text): string
    {
        $url = $text;
        if (isset($options['url'])) $url = $options['url'];
        if (!HtmlHelper::ValidateUrl($url)) return '';
        if ($this->token == 'email') $url = 'mailto:' . Util::Trim($url);
        return HtmlHelper::SanitiseUrl($url);
    }
}
