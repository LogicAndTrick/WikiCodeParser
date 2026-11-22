<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Position;
use LogicAndTrick\WikiCodeParser\Util;

class TrimWhitespaceAroundBlockNodesProcessor implements INodeProcessor
{

    /**
     * @inheritDoc
     */
    function Priority(): int
    {
        return 20;
    }

    /**
     * @inheritDoc
     */
    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof NodeCollection;
    }

    /**
     * @inheritDoc
     */
    function Process(Parser $parser, ParseData $data, INode|null $parentNode, INode $node, string $scope): array
    {
        if (!($node instanceof NodeCollection)) throw new \InvalidArgumentException('Node is not a NodeCollection');
        $coll = $node;

        $ret = [];

        $trimStart = false;
        for ($i = 0; $i < count($coll->nodes); $i++) {
            $child = $coll->nodes[$i];
            $next = $i < count($coll->nodes) - 1 ? $coll->nodes[$i + 1] : null;
            if ($child instanceof PlainTextNode) {
                $text = $child->text;
                if ($trimStart) $text = Util::TrimStart($text);
                if ($next instanceof HtmlNode && $next->isBlockNode) $text = Util::TrimEnd($text);
                $child->text = $text;
            }

            $child = $parser->RunProcessor($coll, $child, $this, $data, $scope);

            if ($child instanceof HtmlNode && $child->isBlockNode) {
                $trimStart = true;
                $ret[] = UnprocessablePlainTextNode::NewLine(new Position($child->line, $child->column));
                $ret[] = $child;
                // this is sort of wrong, but it doesn't come from any source content, so it can be whatever.
                $ret[] = UnprocessablePlainTextNode::NewLine(new Position($child->line, $child->column));
            } else {
                $trimStart = false;
                $ret[] = $child;
            }
        }

        return $ret;
    }
}
