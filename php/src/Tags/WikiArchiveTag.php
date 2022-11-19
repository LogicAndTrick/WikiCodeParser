<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Models\WikiRevisionCredit;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiArchiveTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = '';
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(9);
        $pt = $state->PeekTo(']');
        return $peekTag == '[archive:' && $pt != null && strlen($pt) > 8 && !str_contains($pt, "\n");
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
        $credit->Type = WikiRevisionCredit::TypeArchive;

        $sections = explode('|', $str);
        foreach ($sections as $section) {
            $spl = explode(':', $section);
            $key = $spl[0];
            $val = count($spl) > 1 ? implode(':', array_slice($spl, 1)) : '';
            switch ($key) {
                case 'archive':
                    $credit->Name = $val;
                    break;
                case 'description':
                    $credit->Description = $val;
                    break;
                case 'url':
                    $credit->Url = $val;
                    break;
                case 'wayback':
                    $credit->WaybackUrl = $val;
                    break;
                case 'full':
                    $credit->Type = WikiRevisionCredit::TypeFull;
                    break;
            }
        }
        if ($credit->WaybackUrl != null && $credit->Url != null && !str_starts_with($credit->WaybackUrl, 'http://') && !str_starts_with($credit->WaybackUrl, 'https://') && intval($credit->WaybackUrl, 10)) {
            $credit->WaybackUrl = "https://web.archive.org/web/{$credit->WaybackUrl}/{$credit->Url}";
        }

        $state->SkipWhitespace();
        return new MetadataNode('WikiCredit', $credit);
    }
}