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

class WikiImageTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = 'img';
    }

    private static array $tags = ['img', 'video', 'audio'];

    private static function GetTag(State $state): ?string
    {
        foreach (self::$tags as $tag) {
            $peekTag = $state->Peek(2 + strlen($tag));
            $pt = $state->PeekTo(']');
            if ($peekTag == "[$tag:" && strlen($pt) > 2 + strlen($tag) && !str_contains($pt, "\n")) return $tag;
        }
        return null;
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        $tag = self::GetTag($state);
        return $tag != null;
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;

        $tag = self::GetTag($state);
        if ($state->ScanTo(':') != "[$tag" || $state->Next() != ':') {
            $state->Seek($index, true);
            return null;
        }

        $str = $state->ScanTo(']');

        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $success = preg_match('/^([^|\]]*?)(?:\|([^\]]*?))?$/i', $str, $match);
        if (!$success) {
            $state->Seek($index, true);
            return null;
        }

        $content = new NodeCollection();

        $image = $match[1];
        $params = isset($match[2]) && strlen($match[2]) > 0 ? explode('|', trim($match[2])) : [];
        $src = $image;
        if (!str_contains($image, '/')) {
            $content->nodes[] = new MetadataNode('WikiUpload', $image);
            $slug = WikiRevision::CreateSlug($image);
            $src = "https://twhl.info/wiki/embed/$slug";
        }

        $url = null;
        $caption = null;
        $loop = false;

        $classes = ['embedded', 'image'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;

        foreach ($params as $p) {
            $l = strtolower($p);
            if (self::IsClass($l)) $classes[] = $l;
            else if ($l == 'loop') $loop = true;
            else if (strlen($l) > 4 && str_starts_with($l, 'url:')) $url = trim(substr($p, 4));
            else $caption = trim($p);
        }

        if (!$caption || trim($caption) == '') $caption = null;

        if ($tag == 'img' && $url != null && self::ValidateUrl($url)) {
            if (!preg_match('/^[a-z]{2,10}:\/\//i', $url)) {
                $content->nodes[] = new MetadataNode('WikiLink', $url);
                $slug = WikiRevision::CreateSlug($url);
                $url = "https://twhl.info/wiki/page/$slug";
            }
        } else {
            $url = '';
        }

        $el = 'span';

        // Force inline if we are in an inline context
        if ($context == TagParseContext::Inline && !in_array('inline', $classes)) $classes[] = 'inline';

        // Non-inline images should eat any whitespace after them
        if (!in_array('inline', $classes)) {
            $state->SkipWhitespace();
            $el = 'div';
        }

        $embed = self::GetEmbedObject($tag, $src, $caption, $loop);
        if ($embed != null) $content->nodes[] = $embed;

        if ($caption != null) {
            $cn = new HtmlNode('<span class="caption">', new PlainTextNode($caption), '</span>');
            $cn->plainAfter = '\n';
            $content->nodes[] = $cn;
        }

        $clsJoin = implode(' ', $classes);
        $before = "<$el class=\"$clsJoin\"" . ($caption && strlen($caption) > 0 ? ' title="' . HtmlHelper::AttributeEncode($caption) . '"' : '') . '>'
            . ($url && strlen($url) > 0 ? '<a href="' . HtmlHelper::AttributeEncode($url) . '">' : '')
            . '<span class="caption-panel">';
        $after = '</span>'
            . ($url && strlen($url) > 0 ? '</a>' : '')
            . "</$el>";

        $ret = new HtmlNode($before, $content, $after);
        $ret->isBlockNode = $el == 'div';
        return $ret;
    }

    private static function GetEmbedObject(?string $tag, string $url, ?string $caption, bool $loop): ?INode
    {
        $url = HtmlHelper::AttributeEncode($url);
        switch ($tag) {
            case 'img':
                $caption = $caption ?? 'User posted image';
                $cap = HtmlHelper::AttributeEncode($caption);
                $ret = new HtmlNode("<img class=\"caption-body\" src=\"$url\" alt=\"$cap\" />", PlainTextNode::Empty(), '');
                $ret->plainBefore = '[Image] ';
                return $ret;
            case 'video':
            case 'audio':
                $auto = '';
                if ($loop) $auto = 'autoplay loop muted';
                $ret = new HtmlNode("<$tag class=\"caption-body\" src=\"$url\" playsinline controls ${auto}>Your browser doesn't support embedded $tag.</$tag>", PlainTextNode::Empty(), '');
                $ret->plainBefore = strtoupper(substr($tag, 0, 1)) . substr($tag, 1);
                return $ret;
        }

        return null;
    }

    private static function ValidateUrl(string $url): bool
    {
        return !str_contains($url, '<script');
    }

    private static array $validClasses = ['large', 'medium', 'small', 'thumb', 'left', 'right', 'center', 'inline'];

    private static function IsClass(string $param): bool
    {
        return in_array($param, self::$validClasses);
    }
}