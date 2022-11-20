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

class WikiFileTag extends Tag
{
    private static function GetTag(State $state) : ?string {
        $peekTag = $state->Peek(6);
        $pt = $state->PeekTo(']');
        if ($peekTag == '[file:' && strlen($pt) > 6 && !str_contains($pt, "\n")) return 'file';
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

        $success = preg_match('/^([^#\]]+?)(?:\|([^\]]*?))?$/i', $str, $match);
        if (!$success) {
            $state->Seek($index, true);
            return null;
        }

        $page = $match[1];
        $text = isset($match[2]) && strlen($match[2]) > 0 ? $match[2] : $page;
        $slug = WikiRevision::CreateSlug($page);
        $url = HtmlHelper::AttributeEncode("https://twhl.info/wiki/embed/${slug}");
        $infoUrl = HtmlHelper::AttributeEncode("https://twhl.info/wiki/embed-info/${slug}");

        $before = "<span class=\"embedded-inline download\" data-info=\"${infoUrl}\"><a href=\"${url}\"><span class=\"fa fa-download\"></span> ";
        $after = '</a></span>';

        $content = new NodeCollection();
        $content->nodes[] = new MetadataNode('WikiUpload', $page);
        $content->nodes[] = new PlainTextNode($text);

        return new HtmlNode($before, $content, $after);
    }
}