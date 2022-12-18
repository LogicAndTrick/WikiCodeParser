<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Elements\MdCodeElement;
use LogicAndTrick\WikiCodeParser\Elements\MdColumnsElement;
use LogicAndTrick\WikiCodeParser\Elements\MdHeadingElement;
use LogicAndTrick\WikiCodeParser\Elements\MdLineElement;
use LogicAndTrick\WikiCodeParser\Elements\MdListElement;
use LogicAndTrick\WikiCodeParser\Elements\MdPanelElement;
use LogicAndTrick\WikiCodeParser\Elements\MdQuoteElement;
use LogicAndTrick\WikiCodeParser\Elements\MdTableElement;
use LogicAndTrick\WikiCodeParser\Elements\PreElement;
use LogicAndTrick\WikiCodeParser\Elements\RefElement;
use LogicAndTrick\WikiCodeParser\Processors\AutoLinkingProcessor;
use LogicAndTrick\WikiCodeParser\Processors\MarkdownTextProcessor;
use LogicAndTrick\WikiCodeParser\Processors\NewLineProcessor;
use LogicAndTrick\WikiCodeParser\Processors\SmiliesProcessor;
use LogicAndTrick\WikiCodeParser\Processors\TrimWhitespaceAroundBlockNodesProcessor;
use LogicAndTrick\WikiCodeParser\Tags\CodeTag;
use LogicAndTrick\WikiCodeParser\Tags\FontTag;
use LogicAndTrick\WikiCodeParser\Tags\ImageTag;
use LogicAndTrick\WikiCodeParser\Tags\LinkTag;
use LogicAndTrick\WikiCodeParser\Tags\PreTag;
use LogicAndTrick\WikiCodeParser\Tags\QuickLinkTag;
use LogicAndTrick\WikiCodeParser\Tags\QuoteTag;
use LogicAndTrick\WikiCodeParser\Tags\SpoilerTag;
use LogicAndTrick\WikiCodeParser\Tags\Tag;
use LogicAndTrick\WikiCodeParser\Tags\VaultEmbedTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiArchiveTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiBookTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiCategoryTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiCreditTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiFileTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiImageTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiLinkTag;
use LogicAndTrick\WikiCodeParser\Tags\WikiYoutubeTag;
use LogicAndTrick\WikiCodeParser\Tags\YoutubeTag;

class ParserConfiguration
{
    public static function Twhl() : ParserConfiguration {
        $conf = new ParserConfiguration();

        // Standard inline
        $conf->tags[] = (new Tag('b', 'strong'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('i', 'em'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('u', 'span', 'underline'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('s', 'span', 'strikethrough'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('green', 'span', 'green'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('blue', 'span', 'blue'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('red', 'span', 'red'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('purple', 'span', 'purple'))->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new Tag('yellow', 'span', 'yellow'))->WithScopes('inline', 'excerpt');

        // Standard block
        $conf->tags[] = new PreTag();
        $conf->tags[] = (new Tag('h', 'h3'))->WithBlock(true);


        // Links
        $conf->tags[] = (new LinkTag())->WithScopes('excerpt');
        $conf->tags[] = (new LinkTag())->WithScopes('excerpt')->WithToken('email');
        $conf->tags[] = new QuickLinkTag();
        $conf->tags[] = new WikiLinkTag();
        $conf->tags[] = new WikiFileTag();

        // Embedded
        $conf->tags[] = new ImageTag();
        $conf->tags[] = (new ImageTag())->WithToken('simg')->WithBlock(false);
        $conf->tags[] = new WikiImageTag();
        $conf->tags[] = new YoutubeTag();
        $conf->tags[] = new WikiYoutubeTag();
        $conf->tags[] = new VaultEmbedTag();

        // Custom
        $conf->tags[] = new QuoteTag();
        $conf->tags[] = (new FontTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new WikiCategoryTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new WikiBookTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new WikiCreditTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new WikiArchiveTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new SpoilerTag())->WithScopes('inline', 'excerpt');
        $conf->tags[] = (new CodeTag())->WithScopes('excerpt');

        // Elements
        $conf->elements[] = new MdCodeElement();
        $conf->elements[] = new PreElement();
        $conf->elements[] = new MdHeadingElement();
        $conf->elements[] = new MdLineElement();
        $conf->elements[] = new MdQuoteElement();
        $conf->elements[] = new MdListElement();
        $conf->elements[] = new MdTableElement();
        $conf->elements[] = new MdPanelElement();
        $conf->elements[] = new MdColumnsElement();
        $conf->elements[] = new RefElement();

        // Processors
        $conf->processors[] = new MarkdownTextProcessor();
        $conf->processors[] = new AutoLinkingProcessor();
        $conf->processors[] = (new SmiliesProcessor())->AddDefault();
        $conf->processors[] = new TrimWhitespaceAroundBlockNodesProcessor();
        $conf->processors[] = new NewLineProcessor();

        return $conf;
    }

    public array $elements;
    public array $tags;
    public array $processors;

    public function __construct() {
        $this->elements = [];
        $this->tags = [];
        $this->processors = [];
    }
}