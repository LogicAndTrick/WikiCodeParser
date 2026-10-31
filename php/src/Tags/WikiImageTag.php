<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Models\WikiRevision;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNodeBuilder;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;
use LogicAndTrick\WikiCodeParser\Util;

class WikiImageTag extends Tag
{
    public bool $twhlBehaviour = false;

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
            if ($peekTag == "[$tag:" && $pt && strlen($pt) > 2 + strlen($tag) && !str_contains($pt, "\n")) return $tag;
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
        if (!$tag || $state->ScanTo(':') != "[$tag" || $state->Next() != ':') {
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
        $params = isset($match[2]) && strlen($match[2]) > 0 ? explode('|', Util::Trim($match[2])) : [];
        $src = $image;
        if (!str_contains($image, '/')) {
            if ($this->twhlBehaviour) {
                $content->nodes[] = new MetadataNode('WikiUpload', $image);
                $slug = WikiRevision::CreateSlug($image);
                $src = "https://twhl.info/wiki/embed/$slug";
            } else {
                $state->Seek($index, true);
                return null;
            }
        } else {
            $src = HtmlHelper::StripControlCharacters($src);
            if (!HtmlHelper::ValidateUrl($src)) {
                $state->Seek($index, true);
                return null;
            }
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
            else if (strlen($l) > 4 && str_starts_with($l, 'url:')) $url = Util::Trim(substr($p, 4));
            else $caption = Util::Trim($p);
        }

        if (!$caption || trim($caption) == '') $caption = null;

        if ($url != null) $url = HtmlHelper::StripControlCharacters($url);
        if ($tag == 'img' && $url != null && HtmlHelper::ValidateUrl($url)) {
            if ($this->twhlBehaviour && HtmlHelper::GetUrlScheme($url) == null) {
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
        if ($caption != null) $content->nodes[] = HtmlNodeBuilder::Make('span')->AddClass('caption')->Add(new PlainTextNode($caption))->ToNode(plainBefore: ' ');

        $panel = HtmlNodeBuilder::Make('span')->AddClass('caption-panel')->Add($content)->ToNode();
        $wrapper = !$url || strlen($url) == 0 ? $panel : HtmlNodeBuilder::Make('a')->Attr('href', $url)->Add($panel)->ToNode();

        return HtmlNodeBuilder::Make($el)
            ->AddClasses($classes)
            ->If($caption && strlen($caption) > 0, fn(HtmlNodeBuilder $x) => $x->Attr('title', $caption))
            ->Add($wrapper)
            ->ToNode(isBlockNode: $el == 'div');
    }

    private static function GetEmbedObject(string $tag, string $url, ?string $caption, bool $loop): ?INode
    {
        switch ($tag) {
            case 'img':
                return HtmlNodeBuilder::Make('img')
                    ->AddClass('caption-body')
                    ->Attr('src', $url)
                    ->Attr('alt', $caption ?? 'User posted image')
                    ->ToNode(plainBefore: '[Image]', selfClosing: true);
            case 'video':
            case 'audio':
                return HtmlNodeBuilder::Make($tag)
                    ->AddClass('caption-body')
                    ->Attr('src', $url)
                    ->Prop('playsinline')->Prop('controls')
                    ->If($loop, fn(HtmlNodeBuilder $x) => $x->Prop('autoplay')->Prop('loop')->Prop('muted'))
                    ->Add(new UnprocessablePlainTextNode("Your browser doesn't support embedded {$tag}."))
                    ->ToNode(plainBefore: strtoupper(substr($tag, 0, 1)) . substr($tag, 1), selfClosing: true);
            default:
                return null;
        }
    }

    private static array $validClasses = ['large', 'medium', 'small', 'thumb', 'left', 'right', 'center', 'inline'];

    private static function IsClass(string $param): bool
    {
        return in_array($param, self::$validClasses);
    }
}
