<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Models\WikiRevision;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiLinkTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = 'a';
        $this->mainOption = 'url';
        $this->options = ['url'];
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $pt = $state->PeekTo(']]');
        return strlen($pt) > 1 && $pt[1] == '[' && !str_contains($pt, "\n")
            && preg_match('/([^\]]*?)(?:\|([^\]]*?))?/i', substr($pt, 2));
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;

        if ($state->Next() != '[' || $state->Next() != '[') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']]');

        if ($state->Next() != ']' || $state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $success = preg_match('/^([^\]]+?)(?:\|([^\]]*?))?$/i', $str, $match);
        if (!$success) {
            $state->Seek($index, true);
            return null;
        }

        $page = $match[1];
        $text = $match[2] ?? $page;
        $hash = '';
        if (str_contains($page, '#')) {
            $spl = explode('#', $page);
            $page = $spl[0];
            $anchor = count($spl) > 1 ? implode('#', array_slice($spl, 1)) : '';
            $hash = '#' . preg_replace('/[^\\da-z?\\/:@\\-._~!$&\'()*+,;=]/i', '_', $anchor);
        }

        $slug = WikiRevision::CreateSlug($page);
        $url = HtmlHelper::AttributeEncode("https://twhl.info/wiki/page/$slug") . $hash;
        $before = "<a href=\"${url}\">";
        $after = '</a>';

        $content = new NodeCollection();
        $content->nodes[] = new MetadataNode('WikiLink', $page);
        $content->nodes[] = new PlainTextNode($text);

        return new HtmlNode($before, $content, $after);
    }
}