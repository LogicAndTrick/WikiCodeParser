<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class QuickLinkTag extends Tag
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
        $pt = $state->PeekTo(']');
        if (!$pt || $pt == '') return false;

        $pt = substr($pt, 1);
        return strlen($pt) > 0 && !str_contains($pt, "\n") && preg_match('/^([a-z]{2,10}:\/\/[^\]]*?)(?:\|([^\]]*?))?/i', $pt);
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

        $success = preg_match('/^([a-z]{2,10}:\/\/[^\]]*?)(?:\|([^\]]*?))?$/i', $str, $match);
        if (!$success) {
            $state->Seek($index, true);
            return null;
        }

        $url = $match[1];
        $text = isset($match[2]) && strlen($match[2]) > 0 ? $match[2] : $url;
        $options = [ 'url' => $url ];
        if (!$this->Validate($options, $text)) {
            $state->Seek($index, true);
            return null;
        }

        $url = HtmlHelper::AttributeEncode($url);
        $before = "<$this->element href=\"$url\">";
        $after = "</$this->element>";

        $content = new UnprocessablePlainTextNode($text);
        $ret = new HtmlNode($before, $content, $after);
        $ret->plainAfter = isset($match[2]) && strlen($match[2]) > 0 ? " ($url)" : '';
        return $ret;
    }

    public function Validate(array $options, string $text): bool
    {
        $url = $text;
        if (isset($options['url'])) $url = $options['url'];
        return !str_contains($url, '<script') && preg_match('/^([a-z]{2,10}:\/\/)?([^\]"\n ]+?)$/i', $url);
    }
}