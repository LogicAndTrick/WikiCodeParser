<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Elements\MdHeadingElement;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\Processors\NewLineProcessor;
use LogicAndTrick\WikiCodeParser\Tags\QuickLinkTag;
use PHPUnit\Framework\TestCase;

class BasicTest extends TestCase
{
    private static function GetLeavesRecursive(array &$list, INode $node): void
    {
        $children = $node->GetChildren();
        if (count($children) == 0) {
            $list[] = $node;
        } else {
            foreach ($children as $child) self::GetLeavesRecursive($list, $child);
        }
    }

    /**
     * @return INode[]
     */
    private static function GetLeaves(INode $root): array
    {
        $list = [];
        self::GetLeavesRecursive($list, $root);
        return $list;
    }

    private static function CollapseCollectionsRecursive(array &$list, INode $node): void
    {
        if ($node instanceof NodeCollection) {
            foreach ($node->nodes as $x) self::CollapseCollectionsRecursive($list, $x);
        } else {
            $list[] = $node;
        }
    }

    /**
     * @return INode[]
     */
    private static function CollapseCollections(INode $root): array
    {
        $list = [];
        self::CollapseCollectionsRecursive($list, $root);
        return $list;
    }

    public function testHtmlEscapingOutsideTag()
    {
        $parser = new Parser(new ParserConfiguration());
        $result = $parser->ParseResult('1 & 2');
        self::assertInstanceOf(NodeCollection::class, $result->content);
        $leaves = self::GetLeaves($result->content);
        self::assertCount(1, $leaves);
        $node = $leaves[0];
        self::assertInstanceOf(PlainTextNode::class, $node);
        self::assertEquals("1 & 2", $node->text);
        self::assertEquals("1 &amp; 2", $node->ToHtml());
        self::assertEquals("1 & 2", $node->ToPlainText());
    }

    public function testHtmlEscapingInsideTag()
    {
        $config = new ParserConfiguration();
        $config->tags[] = new QuickLinkTag();
        $parser = new Parser($config);
        $result = $parser->ParseResult("[https://example.com|ex&ple]");
        self::assertInstanceOf(NodeCollection::class, $result->content);
        $leaves = self::CollapseCollections($result->content);
        self::assertCount(1, $leaves);
        $node = $leaves[0];
        self::assertInstanceOf(HtmlNode::class, $node);
        $htnode = $node;
        self::assertEquals("<a href=\"https://example.com\">", $htnode->htmlBefore);
        self::assertEquals("</a>", $htnode->htmlAfter);
        $content = self::CollapseCollections($htnode->content);
        self::assertCount(1, $content);
        self::assertInstanceOf(UnprocessablePlainTextNode::class, $content[0]);
        $ptnode = $content[0];
        self::assertEquals("ex&ple", $ptnode->text);
    }

    public function testBlockNewLines()
    {
        $input = "a\n\n\n\n= b\n\n\n\nc\nd\n\ne";
        $output = "a\n<h1 id=\"b\">b</h1>\nc<br/>\nd<br/>\n<br/>\ne";

        $config = new ParserConfiguration();
        $config->elements[] = new MdHeadingElement();
        $config->processors[] = new NewLineProcessor();
        $parser = new Parser($config);
        $result = $parser->ParseResult($input);

        self::assertEquals($output, $result->ToHtml());
    }

    public function testCarriageReturns()
    {
        $input = "Line 1\r\nLine 2\r\nLine 3\nLine 4";
        $output = "Line 1<br/>\nLine 2<br/>\nLine 3<br/>\nLine 4";

        $config = new ParserConfiguration();
        $config->processors[] = new NewLineProcessor();
        $parser = new Parser($config);
        $result = $parser->ParseResult($input);

        self::assertEquals($output, $result->ToHtml());
    }
}
