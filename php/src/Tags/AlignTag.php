<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Colours;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class AlignTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'align';
        $this->element = 'div';
        $this->mainOption = 'align';
        $this->options = ['align'];
        $this->allOptionsInMain = true;
        $this->isBlock = true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode
    {
        $before = '<' . $this->element;
        $cls = ($this->elementClass || '') . ' ';
        if (isset($options['align']) && self::IsValidAlign($options['align'])) {
            $cls .= 'text-' . self::ConvertAlign($options['align']);
        }
        $before .= ' class="'. trim($cls) . '">';
        $content = $parser->ParseTags($data, $text, $scope, $this->TagContext());
        $after = '</' . $this->element . '>';
        $ret = new HtmlNode($before, $content, $after);
        $ret->isBlockNode = true;
        return $ret;
    }

    private static function IsValidAlign(string $text) : bool {
        return $text == 'left' || $text == 'right' || $text == 'center';
    }

    private static function ConvertAlign(string $text) : string {
        if ($text == 'left') return 'start';
        if ($text == 'right') return 'end';
        return 'center';
    }
}