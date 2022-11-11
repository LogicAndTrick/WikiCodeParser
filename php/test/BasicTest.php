<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Nodes\INode;
use PHPUnit\Framework\TestCase;


class BasicTest extends TestCase
{
    private static function GetLeavesRecursive(array &$list, INode $node): void {
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
    private static function GetLeaves(INode $root): array {
        $list = [];
        self::GetLeavesRecursive($list, $root);
        return $list;
    }

    public function testHtmlEscapingOutsideTag() {
        $parser = new Parser(new ParserConfiguration());
        $result = $parser->ParseResult('1 & 2');
        self::assertInstanceOf('NodeCollection', $result->Content);
        $leaves = self::GetLeaves($result->Content);
        self::assertCount(1, $leaves);
        $node = $leaves[0];
        self::assertInstanceOf('PlainTextNode', $node);
        self::assertEquals("1 & 2", $node->Text);
        self::assertEquals("1 &amp; 2", $node->ToHtml());
        self::assertEquals("1 & 2", $node->ToPlainText());
    }
}
