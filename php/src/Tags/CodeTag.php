<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class CodeTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'code';
        $this->element = 'code';
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode|null
    {
        return new HtmlNode('<code>', new UnprocessablePlainTextNode($text), '</code>');
    }
}