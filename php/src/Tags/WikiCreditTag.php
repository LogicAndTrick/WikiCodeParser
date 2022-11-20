<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Models\WikiRevisionCredit;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiCreditTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = '';
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(8);
        $pt = $state->PeekTo(']');
        return $peekTag == '[credit:' && $pt != null && strlen($pt) > 8 && !str_contains($pt, "\n");
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;
        if ($state->Next() != '[') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']');

        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $credit = new WikiRevisionCredit();
        $credit->Type = WikiRevisionCredit::TypeCredit;

        $sections = explode('|', $str);
        foreach ($sections as $section) {
            $spl = explode(':', $section);
            $key = $spl[0];
            $val = count($spl) > 1 ? implode(':', array_slice($spl, 1)) : '';
            switch ($key) {
                case 'credit':
                    $credit->Description = $val;
                    break;
                case 'user':
                    $credit->UserID = intval($val, 10) ?? null;
                    break;
                case 'name':
                    $credit->Name = $val;
                    break;
                case 'url':
                    $credit->Url = $val;
                    break;
            }
        }

        $state->SkipWhitespace();
        return new MetadataNode('WikiCredit', $credit);
    }
}