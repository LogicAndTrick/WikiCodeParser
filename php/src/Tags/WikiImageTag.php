<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Models\WikiRevision;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNodeBuilder;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Position;
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
        $pos = $state->sourcePosition;
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

        $image = HtmlHelper::StripControlCharacters($match[1]);
        $params = isset($match[2]) && strlen($match[2]) > 0 ? explode('|', Util::Trim($match[2])) : [];
        $src = $image;
        if (!str_contains($image, '/')) {
            if ($this->twhlBehaviour) {
                $content->nodes[] = new MetadataNode($pos, 'WikiUpload', $image);
                $slug = WikiRevision::CreateSlug($image);
                $src = $parser->configuration->BuildUrl("/wiki/embed/$slug");
            } else {
                $state->Seek($index, true);
                return null;
            }
        }

        if (!HtmlHelper::ValidateUrl($src)) {
            $state->Seek($index, true);
            return null;
        }

        $src = HtmlHelper::SanitiseUrl($src);

        $url = null;
        $caption = null;
        $loop = false;

        $classes = ['embedded', 'image'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;
        $sizeClass = null;
        $positionClass = null;
        $valid = true;

        foreach ($params as $p) {
            $l = strtolower($p);
            if (self::IsSizeClass($l)) {
                if ($sizeClass !== null) $valid = false;
                $sizeClass = $l;
                $classes[] = $l;
            } else if (self::IsPositionClass($l)) {
                if ($positionClass !== null) $valid = false;
                $positionClass = $l;
                $classes[] = $l;
            } else if ($l == 'loop') {
                if ($loop) $valid = false;
                $loop = true;
            } else if (strlen($l) > 4 && str_starts_with($l, 'url:')) {
                if ($url !== null) $valid = false;
                $url = Util::Trim(substr($p, 4));
            } else {
                if ($caption !== null) $valid = false;
                $caption = Util::Trim($p);
            }
        }

        if (!$valid) {
            $state->Seek($index, true);
            return null;
        }

        if (!$caption || trim($caption) == '') $caption = null;

        if ($url !== null) {
            if ($tag == 'img' && HtmlHelper::ValidateUrl($url)) {
                if ($this->twhlBehaviour && HtmlHelper::GetUrlScheme($url) == null) {
                    $content->nodes[] = new MetadataNode($pos, 'WikiLink', $url);
                    $slug = WikiRevision::CreateSlug($url);
                    $url = $parser->configuration->BuildUrl("/wiki/page/$slug");
                }
                $url = HtmlHelper::SanitiseUrl($url);
            } else {
                $state->Seek($index, true);
                return null;
            }
        }

        $el = 'span';

        // Force inline if we are in an inline context
        if ($context == TagParseContext::Inline && !in_array('inline', $classes)) $classes[] = 'inline';

        // Non-inline images should eat any whitespace after them
        if (!in_array('inline', $classes)) {
            $state->SkipWhitespace();
            $el = 'div';
        }

        $embed = self::GetEmbedObject($pos, $tag, $src, $caption, $loop);
        if ($embed != null) $content->nodes[] = $embed;
        if ($caption != null) $content->nodes[] = HtmlNodeBuilder::Make('span', $pos)->AddClass('caption')->Add(new PlainTextNode($pos, $caption))->ToNode(plainBefore: ' ');

        $panel = HtmlNodeBuilder::Make('span', $pos)->AddClass('caption-panel')->Add($content)->ToNode();
        $wrapper = $url === null ? $panel : HtmlNodeBuilder::Make('a', $pos)->Attr('href', $url)->Add($panel)->ToNode();

        return HtmlNodeBuilder::Make($el, $pos)
            ->AddClasses($classes)
            ->If($caption && strlen($caption) > 0, fn(HtmlNodeBuilder $x) => $x->Attr('title', $caption))
            ->Add($wrapper)
            ->ToNode(isBlockNode: $el == 'div');
    }

    private static function GetEmbedObject(Position $pos, string $tag, string $url, ?string $caption, bool $loop): ?INode
    {
        switch ($tag) {
            case 'img':
                return HtmlNodeBuilder::Make('img', $pos)
                    ->AddClass('caption-body')
                    ->Attr('src', $url)
                    ->Attr('alt', $caption ?? 'User posted image')
                    ->ToNode(plainBefore: '[Image]', selfClosing: true);
            case 'video':
            case 'audio':
                return HtmlNodeBuilder::Make($tag, $pos)
                    ->AddClass('caption-body')
                    ->Attr('src', $url)
                    ->Prop('playsinline')->Prop('controls')
                    ->If($loop, fn(HtmlNodeBuilder $x) => $x->Prop('autoplay')->Prop('loop')->Prop('muted'))
                    ->Add(new UnprocessablePlainTextNode($pos, "Your browser doesn't support embedded {$tag}."))
                    ->ToNode(plainBefore: strtoupper(substr($tag, 0, 1)) . substr($tag, 1), selfClosing: true);
            default:
                return null;
        }
    }

    private static array $validSizeClasses = ['large', 'medium', 'small', 'thumb'];
    private static array $validPositionClasses = ['left', 'right', 'center', 'inline'];

    private static function IsSizeClass(string $param): bool
    {
        return in_array($param, self::$validSizeClasses);
    }

    private static function IsPositionClass(string $param): bool
    {
        return in_array($param, self::$validPositionClasses);
    }
}
