<?php

namespace LogicAndTrick\WikiCodeParser;

use InvalidArgumentException;
use LogicAndTrick\WikiCodeParser\Elements\Element;
use LogicAndTrick\WikiCodeParser\Elements\MdCodeElement;
use LogicAndTrick\WikiCodeParser\Elements\MdColumnsElement;
use LogicAndTrick\WikiCodeParser\Elements\MdHeadingElement;
use LogicAndTrick\WikiCodeParser\Elements\MdLineElement;
use LogicAndTrick\WikiCodeParser\Elements\MdListElement;
use LogicAndTrick\WikiCodeParser\Elements\MdPanelElement;
use LogicAndTrick\WikiCodeParser\Elements\MdQuoteElement;
use LogicAndTrick\WikiCodeParser\Elements\MdTableElement;
use LogicAndTrick\WikiCodeParser\Elements\PreElement;
use LogicAndTrick\WikiCodeParser\Elements\QuoteElement;
use LogicAndTrick\WikiCodeParser\Elements\RefElement;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\RemovedNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\ParserConfiguration;
use LogicAndTrick\WikiCodeParser\ParseResult;
use LogicAndTrick\WikiCodeParser\Processors\AutoLinkingProcessor;
use LogicAndTrick\WikiCodeParser\Processors\MarkdownTextProcessor;
use LogicAndTrick\WikiCodeParser\Processors\MentionsProcessor;
use LogicAndTrick\WikiCodeParser\Processors\NewLineProcessor;
use LogicAndTrick\WikiCodeParser\Processors\SmiliesProcessor;
use LogicAndTrick\WikiCodeParser\Processors\TrimWhitespaceAroundBlockNodesProcessor;
use LogicAndTrick\WikiCodeParser\Tags\AlignTag;
use LogicAndTrick\WikiCodeParser\Tags\CodeTag;
use LogicAndTrick\WikiCodeParser\Tags\ColorTag;
use LogicAndTrick\WikiCodeParser\Tags\FontTag;
use LogicAndTrick\WikiCodeParser\Tags\ImageTag;
use LogicAndTrick\WikiCodeParser\Tags\LinkTag;
use LogicAndTrick\WikiCodeParser\Tags\ListTag;
use LogicAndTrick\WikiCodeParser\Tags\PreTag;
use LogicAndTrick\WikiCodeParser\Tags\QuickLinkTag;
use LogicAndTrick\WikiCodeParser\Tags\QuoteTag;
use LogicAndTrick\WikiCodeParser\Tags\SizeTag;
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
use LogicAndTrick\WikiCodeParser\TestCaseUtils;
use LogicAndTrick\WikiCodeParser\TestModels\TestConfig;
use LogicAndTrick\WikiCodeParser\TestModels\TestNode;
use LogicAndTrick\WikiCodeParser\TestModels\TestDefinition;
use PHPUnit\Framework\Assert;
use PHPUnit\Framework\Attributes\DataProvider;
use PHPUnit\Framework\TestCase;

// https://stackoverflow.com/a/15875555
ImplementationTest::$RemovedContentGuid = (function () {
    $data = random_bytes(16);

    $data[6] = chr(ord($data[6]) & 0x0f | 0x40); // set version to 0100
    $data[8] = chr(ord($data[8]) & 0x3f | 0x80); // set bits 6-7 to 10

    return vsprintf('%s%s-%s-%s-%s-%s%s%s', str_split(bin2hex($data), 4));
})();

class ImplementationTest extends TestCase
{
    static string $RemovedContentGuid;

    private static function CreateConfig(mixed $config): ParserConfiguration
    {
        /** @var TestConfig $config */
        $con = match ($config->base) {
            "twhl" => ParserConfiguration::Twhl("https://twhl.info"),
            "snarkpit" => ParserConfiguration::Snarkpit("https://snarkpit.net"),
            default => new ParserConfiguration("https://example.com"),
        };

        foreach (($config->elements ?? []) as $e) {
            $con->elements[] = match ($e) {
                "MdCode" => new MdCodeElement(),
                "MdColumns" => new MdColumnsElement(),
                "MdHeading" => new MdHeadingElement(),
                "MdLine" => new MdLineElement(),
                "MdList" => new MdListElement(),
                "MdPanel" => new MdPanelElement(),
                "MdQuote" => new MdQuoteElement(),
                "MdTable" => new MdTableElement(),
                "Pre" => new PreElement(),
                "Quote" => new QuoteElement(),
                "Ref" => new RefElement(),
                default => throw new InvalidArgumentException("Unknown element name '$e'"),
            };
        }

        foreach (($config->tags ?? []) as $t) {
            $con->tags[] = match ($t) {
                "Strong" => (new Tag("b", "strong"))->WithScopes("inline", "excerpt"),
                "Emphasis" => (new Tag("i", "em"))->WithScopes("inline", "excerpt"),
                "Underline" => (new Tag("u", "span", "underline"))->WithScopes("inline", "excerpt"),
                "Strikethrough" => (new Tag("s", "span", "strikethrough"))->WithScopes("inline", "excerpt"),
                "Align" => new AlignTag(),
                "Code" => new CodeTag(),
                "Color" => new ColorTag(),
                "Font" => new FontTag(),
                "Image" => new ImageTag(),
                "Link" => new LinkTag(),
                "List" => new ListTag(),
                "Pre" => new PreTag(),
                "QuickLink" => new QuickLinkTag(),
                "Quote" => new QuoteTag(),
                "Size" => new SizeTag(),
                "Spoiler" => new SpoilerTag(),
                "VaultEmbed" => new VaultEmbedTag(),
                "WikiArchive" => new WikiArchiveTag(),
                "WikiBook" => new WikiBookTag(),
                "WikiCategory" => new WikiCategoryTag(),
                "WikiCredit" => new WikiCreditTag(),
                "WikiFile" => new WikiFileTag(),
                "WikiLink" => new WikiLinkTag(),
                "WikiYoutube" => new WikiYoutubeTag(),
                "Youtube" => new YoutubeTag(),
                "WikiImage" => (function () {
                    $tag = new WikiImageTag();
                    $tag->twhlBehaviour = true;
                    return $tag;
                })(),
                default => throw new InvalidArgumentException("Unknown tag name '$t'"),
            };
        }

        foreach (($config->processors ?? []) as $p) {
            $con->processors[] = match ($p) {
                "AutoLinking" => new AutoLinkingProcessor(),
                "MarkdownText" => new MarkdownTextProcessor(),
                "NewLine" => new NewLineProcessor(),
                "Smilies" => new SmiliesProcessor("/smilies/{0}.png")->Add('smile', ':)'),
                "TrimWhitespace" => new TrimWhitespaceAroundBlockNodesProcessor(),
                "Mentions" => new MentionsProcessor("/user/{0}"),
                default => throw new InvalidArgumentException("Unknown processor name '$p'"),
            };
        }

        return $con;
    }

    private static function AssertMetadata(mixed $expected, mixed $actual): void
    {
        if ($expected === null) {
            Assert::assertNull($actual);
            return;
        }

        if (is_object($expected)) $expected = json_decode(json_encode($expected) ?: '{}', true);
        if (is_object($actual)) $actual = json_decode(json_encode($actual) ?: '{}', true);

        Assert::assertNotNull($actual);
        // Assert::assertEquals(get_class($expected), get_class($actual));

        if (is_string($expected)) {
            Assert::assertIsString($actual);
            Assert::assertEquals($expected, $actual);
        } elseif (is_array($expected)) {
            Assert::assertIsArray($actual);

            /** @var array<string, string|array|null> $eobj */ $eobj = $expected;
            /** @var array<string, string|array|null> $robj */ $robj = $actual;

            $rkeys = [];
            foreach ($robj as $key => $value) {
                if ($value !== null && $value != "0" && $value != "") {
                    $rkeys[] = $key;
                }
            }
            sort($rkeys);

            $ekeys = array_keys($eobj);
            sort($ekeys);

            Assert::assertArraysAreEqual($rkeys, $ekeys);

            if ($expected == $actual) return;

            foreach ($eobj as $key => $ev) {
                $rv = $robj[$key] ?? null;
                Assert::assertEquals($ev, $rv);
            }
        }
    }

    private static function AssertNodeHierarchy(mixed $expected, INode $node, int $lineOffset): void
    {
        /** @var TestNode $expected */

        if ($expected->pos[0] > 0) {
            Assert::assertEquals($expected->pos[0] + $lineOffset, $node->line, "Wrong line for '" . ($expected->name ?? $expected->type) . "'");
            Assert::assertEquals($expected->pos[1], $node->column, "Wrong column for '" . ($expected->name ?? $expected->type) . "'");
        }

        $commonNames = [
            "list" => "NodeCollection",
            "plain" => "PlainTextNode",
            "html" => "HtmlNode",
            "meta" => "MetadataNode",
            "ref" => "RefNode",
            "removed" => "RemovedNode",
        ];

        $expectedType = $commonNames[$expected->type] ?? $expected->type;
        if ($expectedType === "UnprocessablePlainTextNode") {
            $expectedType = "PlainTextNode";
        }

        $actualType = pathinfo(str_replace('\\', '/', get_class($node)), PATHINFO_FILENAME);
        if ($actualType === "UnprocessablePlainTextNode") {
            $actualType = "PlainTextNode";
        }

        Assert::assertEquals($expectedType, $actualType);

        switch ($expectedType) {
            case "PlainTextNode":
                if ($node instanceof PlainTextNode || $node instanceof UnprocessablePlainTextNode) {
                    $text = $node->text;
                } else {
                    throw new InvalidArgumentException("Unexpected node type");
                }
                Assert::assertEquals($expected->text, $text);
                break;

            case "HtmlNode":
                /** @var HtmlNode */ $hn = $node;
                Assert::assertInstanceOf(HtmlNode::class, $hn);
                Assert::assertNotNull($expected->html);
                Assert::assertCount(2, $expected->html, "Invalid test configuration");
                Assert::assertNotNull($expected->plain);
                Assert::assertCount(2, $expected->plain, "Invalid test configuration");
                Assert::assertEquals($expected->html[0], $hn->htmlBefore);
                Assert::assertEquals($expected->html[1], $hn->htmlAfter);
                Assert::assertEquals($expected->plain[0], $hn->plainBefore);
                Assert::assertEquals($expected->plain[1], $hn->plainAfter);
                break;

            case "MetadataNode":
                /** @var MetadataNode */ $mn = $node;
                Assert::assertInstanceOf(MetadataNode::class, $mn);
                Assert::assertEquals($expected->key, $mn->key);
                /** @var array<string, string|array|null> */ $av = $mn->value;
                self::AssertMetadata($expected->value, $av);
                break;
        }

        $expectedChildren = $expected->children ?? [];
        $children = array_values(array_filter(
            $node->GetChildren(),
            fn($child) => !($child instanceof RemovedNode)
        ));

        Assert::assertCount(
            count($expectedChildren),
            $children,
            "Wrong number of child nodes for '" . ($expected->Name ?? $expected->type) . "'.\n" .
                "Actual children: [" . implode(", ", array_map(fn($x) => get_class($x), $children)) . "]"
        );

        foreach ($expectedChildren as $i => $child) {
            self::AssertNodeHierarchy($child, $children[$i], $lineOffset);
        }
    }

    private static array $lineOffsets = [0, 1, 5];
    public static function getTestCases(): array
    {
        $arr = [];
        foreach (TestCaseUtils::GetTestCaseNames('implementations') as $x) {
            foreach (self::$lineOffsets as $y) {
                $arr["{$x} / {$y}"] = [$x, $y];
            }
        }
        return $arr;
    }

    public function RunImplementationTest(string $name, int $lineOffset): void
    {
        $dir = TestCaseUtils::GetTestCaseDirectory("implementations");
        /** @var TestDefinition $def */
        $def = json_decode(file_get_contents("$dir/$name.json") ?: '{}');

        $input = $def->input;
        $config = self::CreateConfig($def->config);
        $parser = new Parser($config);
        $afterParse = fn(ParseResult $result) => null;

        if ($lineOffset > 0) {
            $input = str_repeat("\n", $lineOffset) . $input;
            array_unshift($config->elements, new LineOffsetEaterElement($lineOffset));
            $afterParse = self::RemoveLineOffsetElement(...);
        }

        $result = $parser->ParseResult($input);
        $afterParse($result);

        $resultHtml = $result->ToHtml(TestCaseUtils::Sanitise(...));
        $resultPlain = $result->ToPlainText();

        $expectedHtml = TestCaseUtils::Sanitise($def->html);
        $expectedPlain = $def->plain;

        Assert::assertEquals($expectedHtml, $resultHtml);
        Assert::assertEquals($expectedPlain, $resultPlain);

        self::AssertNodeHierarchy($def->nodes, $result->content, $lineOffset);
    }

    #[DataProvider('getTestCases')]
    public function testImplementation(string $name, int $lineOffset)
    {
        self::RunImplementationTest($name, $lineOffset);
    }

    private static function RemoveLineOffsetElement(ParseResult $result): void
    {
        $nc = $result->content;
        if (!($nc instanceof NodeCollection) || count($nc->nodes) < 1) return;
        if (!($nc->nodes[0] instanceof UnprocessablePlainTextNode) || $nc->nodes[0]->text !== self::$RemovedContentGuid) return;
        array_splice($nc->nodes, 0, 1);

        if (count($nc->nodes) < 1) return;
        if (!($nc->nodes[0] instanceof UnprocessablePlainTextNode) || $nc->nodes[0]->text !== "\n") return;
        array_splice($nc->nodes, 0, 1);
    }
}

class LineOffsetEaterElement extends Element
{
    private int $lineOffset;

    public function __construct(int $lineOffset)
    {
        $this->lineOffset = $lineOffset;
        $this->priority = PHP_INT_MAX;
    }

    public function Matches(Lines $lines): bool
    {
        return $this->lineOffset > 0 && $lines->sourceLine === 1;
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): INode|null
    {
        if ($data->current_element_depth > 1) return null;
        for ($i = 0; $i < $this->lineOffset - 1; $i++) $lines->Next();
        return new UnprocessablePlainTextNode(new Position(1, 0), ImplementationTest::$RemovedContentGuid);
    }
}
