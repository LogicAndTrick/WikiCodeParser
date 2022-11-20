<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Models\WikiRevisionBook;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiBookTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = '';
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(6);
        $pt = $state->PeekTo(']');
        return $peekTag == '[book:' && $pt != null && strlen($pt) > 6 && !str_contains($pt, "\n");
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

        $book = new WikiRevisionBook();

        $sections = explode('|', $str);
        foreach ($sections as $section) {
            $spl = explode(':', $section);
            $key = $spl[0];
            $val = count($spl) > 1 ? implode(':', array_slice($spl, 1)) : '';
            switch ($key) {
                case 'book':
                    $book->BookName = $val;
                    break;
                case 'chapter':
                    $book->ChapterName = $val;
                    break;
                case 'chapternumber':
                    $book->ChapterNumber = intval($val, 10) ?? null;
                    break;
                case 'pagenumber':
                    $book->PageNumber = intval($val, 10) ?? null;
                    break;
            }
        }

        $state->SkipWhitespace();
        return new MetadataNode('WikiBook', $book);
    }
}