<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class VaultEmbedTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->element = 'div';
        $this->mainOption = 'id';
        $this->options = ['id'];
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(7);
        $pt = $state->PeekTo(']');
        return $context == TagParseContext::Block && $peekTag == '[vault:' && strlen($pt) > 7 && !str_contains($pt, "\n");
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;

        if ($state->ScanTo(':') != '[vault' || $state->Next() != ':') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']');
        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $id = intval($str, 10);
        if (!$id) {
            $state->Seek($index, true);
            return null;
        }

        $classes = ['embedded', 'vault'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;

        $state->SkipWhitespace();

        $clsJoin = implode(' ', $classes);
        $before = "<div class=\"$clsJoin\">" .
            '<div class="embed-container">' .
            '<div class="embed-content">' .
            "<div class=\"uninitialised\" data-embed-type=\"vault\" data-vault-id=\"$id\">" .
            "Loading embedded content: Vault Item #${id}";
        $after = '</div></div></div></div>';
        $ret = new HtmlNode($before, PlainTextNode::Empty(), $after);
        $ret->plainBefore = '[TWHL vault item #{id}]';
        $ret->plainAfter = "\n";
        $ret->isBlockNode = true;
        return $ret;
    }
}