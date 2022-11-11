<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

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
    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        /* @var $coll NodeCollection */
        $coll = $node;

        $ret = [];

        $trimStart = false;
        for ($i = 0; $i < count($coll->nodes); $i++) {
            $child = $coll->nodes[$i];
            $next = $i < count($coll->nodes) - 1 ? $coll->nodes[$i + 1] : null;
            if ($child instanceof PlainTextNode) {
                $text = $child->text;
                if ($trimStart) $text = ltrim($text);
                if ($next instanceof HtmlNode && $next->isBlockNode) $text = rtrim($text);
                $child->text = $text;
            }

            $child = $parser->RunProcessor($child, $this, $data, $scope);

            if ($child instanceof HtmlNode && $child->isBlockNode) {
                $trimStart = true;
                $ret[] = UnprocessablePlainTextNode::NewLine();
                $ret[] = $child;
                $ret[] = UnprocessablePlainTextNode::NewLine();
            } else {
                $trimStart = false;
                $ret[] = $child;
            }
        }

        return $ret;
    }
}