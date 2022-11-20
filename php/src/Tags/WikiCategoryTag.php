<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiCategoryTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = '';
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(5);
        $pt = $state->PeekTo(']');
        return $peekTag == '[cat:' && $pt != null && strlen($pt) > 5 && !str_contains($pt, "\n");
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;
        if ($state->ScanTo(':') != '[cat' || $state->Next() != ':') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']');
        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $state->SkipWhitespace();
        return new MetadataNode('WikiCategory', trim($str));
    }
}