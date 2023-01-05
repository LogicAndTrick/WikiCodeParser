<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Colours;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class FontTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'font';
        $this->element = 'span';
        $this->mainOption = 'color';
        $this->options = ['color', 'colour', 'size'];
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        $before = '<' . $this->element;
        if ($this->elementClass != null) $before .= ' class="' . $this->elementClass . '"';
        if (isset($options['color']) || isset($options['colour']) || isset($options['size'])) {
            $before .= ' style="';
            if (isset($options['color']) && Colours::IsValidColor($options['color'])) $before .= 'color: ' . $options['color'] . '; ';
            else if (isset($options['colour']) && Colours::IsValidColor($options['colour'])) $before .= 'color: ' . $options['colour'] . '; ';
            if (isset($options['size']) && FontTag::IsValidSize($options['size'])) $before .= 'font-size: ' . $options['size'] . 'px; ';
            $before = rtrim($before) . '"';
        }
        $before .= '>';
        $content = $parser->ParseTags($data, $text, $scope, $this->TagContext());
        $after = '</' . $this->element . '>';
        return new HtmlNode($before, $content, $after);
    }

    public static function IsValidSize(string $text): bool
    {
        $num = intval($text, 10) || 0;
        return $num >= 6 && $num <= 40;
    }
}