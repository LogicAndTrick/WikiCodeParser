<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class ImageTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'img';
        $this->element = 'div';
        $this->mainOption = 'url';
        $this->options = ['url'];
        $this->isBlock = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $url = HtmlHelper::AttributeEncode(self::BuildUrl($options, $text));

        $classes = ['embedded', 'image'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;
        $element = $this->element;

        if (!$this->isBlock) {
            $element = 'span';
            $classes[] = 'inline';
        } else {
            $state->SkipWhitespace();
        }

        $before = '<' . $element . ' class="' . implode(' ', $classes) . '">' .
            '<span class="caption-panel">' .
            '<img class="caption-body" src="' . $url . '" alt="User posted image" />';
        $after = "</span></$element>";
        $plainsp = $element == 'div' ? "\n" : '';
        $ret = new HtmlNode($before, PlainTextNode::Empty(), $after);
        $ret->plainBefore = $plainsp . "[User posted image]" . $plainsp;
        $ret->isBlockNode = $element == 'div';
        return $ret;
    }

    public function Validate(array $options, string $text): bool
    {
        $url = self::BuildUrl($options, $text);
        return HtmlHelper::ValidateUrl($url) && preg_match('/^[^\]"\n ]+$/i', $url);
    }

    private static function BuildUrl(array $options, string $text): string
    {
        $url = $text;
        if (isset($options['url'])) $url = $options['url'];
        $url = HtmlHelper::StripControlCharacters($url);
        if (HtmlHelper::GetUrlScheme($url) == null) $url = 'http://' . $url;
        return $url;
    }
}
