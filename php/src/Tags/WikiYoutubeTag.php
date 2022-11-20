<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class WikiYoutubeTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = 'div';
        $this->mainOption = 'id';
        $this->options = ['id'];
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $peekTag = $state->Peek(9);
        $pt = $state->PeekTo(']');
        return $context == TagParseContext::Block && $peekTag == '[youtube:' && $pt != null && strlen($pt) > 9 && !str_contains($pt, "\n");
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;

        if ($state->ScanTo(':') != '[youtube' || $state->Next() != ':') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']');

        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $success = preg_match('/^([^|\]]*?)(?:\|([^\]]*?))?$/i', $str, $regs);
        if (!$success) {
            $state->Seek($index, true);
            return null;
        }

        $id = $regs[1];
        $params = isset($regs[2]) && strlen($regs[2]) > 0 ? explode('|', trim($regs[2])) : [];

        if (!self::ValidateID($id)) {
            $state->Seek($index, true);
            return null;
        }

        $state->SkipWhitespace();

        $caption = null;
        $classes = ['embedded', 'video'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;
        foreach ($params as $p) {
            $l = strtolower($p);
            if (self::IsClass($l)) $classes[] = $l;
            else $caption = trim($p);
        }

        if (!$caption || trim($caption) == '') $caption = null;

        $captionNode = new HtmlNode(
            $caption != null ? '<span class="caption">' : '',
            new PlainTextNode($caption ?? ''),
            $caption != null ? '</span>' : ''
        );
        $captionNode->plainBefore = '[YouTube video] ';
        $captionNode->plainAfter = "\n";

        $clsJoin = implode(' ', $classes);
        $before = "<div class=\"$clsJoin\">" .
            '<div class="caption-panel">' .
            '<div class="video-container caption-body">' .
            '<div class="video-content">' .
            "<div class=\"uninitialised\" data-youtube-id=\"$id\" style=\"background-image: url('https://i.ytimg.com/vi/${id}/hqdefault.jpg');\"></div>" .
            '</div>' .
            '</div>';
        $after = '</div></div>';
        $ret = new HtmlNode($before, $captionNode, $after);
        $ret->isBlockNode = true;
        return $ret;
    }

    private static function ValidateID(string $url): bool
    {
        return preg_match('/^[a-zA-Z0-9_-]{6,11}$/i', $url);
    }

    private static array $validClasses = ['large', 'medium', 'small', 'left', 'right', 'center'];

    private static function IsClass(string $param): bool
    {
        return in_array($param, self::$validClasses);
    }
}