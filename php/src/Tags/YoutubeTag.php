<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;

class YoutubeTag extends Tag
{
    public function __construct()
    {
        parent::__construct();
        $this->token = 'youtube';
        $this->element = 'div';
        $this->mainOption = 'id';
        $this->options = ['id'];
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode
    {
        $id = $text;
        if (isset($options['id']) && $options['id']) $id = $options['id'];

        $classes = ['embedded', 'video'];
        if ($this->elementClass != null) $classes[] = $this->elementClass;

        $captionNode = new HtmlNode('', PlainTextNode::Empty(), '');
        $captionNode->plainBefore = '[YouTube video] ';
        $captionNode->plainAfter = "\n";

        $clsJoin = implode(' ', $classes);
        $before = "<div class=\"$clsJoin\">" .
            ' <div class="caption-panel">' .
            '  <div class="video-container caption-body">' .
            '   <div class="video-content">' .
            "    <div class=\"uninitialised\" data-youtube-id=\"$id\" style=\"background-image: url('https://i.ytimg.com/vi/${id}/hqdefault.jpg');\"></div>" .
            '   </div>' .
            '  </div>';
        $after = '</div></div>';
        $ret = new HtmlNode($before, $captionNode, $after);
        $ret->isBlockNode = true;
        return $ret;
    }

    public function Validate(array $options, string $text): bool
    {
        $url = $text;
        if (isset($options['id']) && $options['id']) $url = $options['id'];
        return preg_match('/^[a-zA-Z0-9_-]{6,11}$/i', $url);
    }
}