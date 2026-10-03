<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

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

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $url = HtmlHelper::AttributeEncode($this->BuildUrl($options, $text));

        $classes = [];
        if ($this->elementClass != null) $classes[] = $this->elementClass;

        $element = $this->element;
        $before = "<$element " . (count($classes) > 0 ? 'class="' . implode(' ', $classes) . '" ' : '') . 'href="' . $url . '">';
        $after = "</$element>";

        $content = isset($options['url'])
            ? $parser->ParseTags($data, $text, $scope, $this->TagContext())
            : new UnprocessablePlainTextNode($text);
        return new HtmlNode($before, $content, $after);
    }

    public function Validate(array $options, string $text): bool
    {
        if (isset($options['url']) && trim($options['url']) === '') return false;
        $url = $this->BuildUrl($options, $text);
        return HtmlHelper::ValidateUrl($url) && preg_match('/^[^\]"\n ]+$/i', $url);
    }

    private function BuildUrl(array $options, string $text): string
    {
        $url = $text;
        if (isset($options['url'])) $url = $options['url'];
        $url = HtmlHelper::StripControlCharacters($url);
        if ($this->token == 'email') $url = 'mailto:' . $url;
        else if (HtmlHelper::GetUrlScheme($url) == null) $url = 'http://' . $url;
        return $url;
    }
}
