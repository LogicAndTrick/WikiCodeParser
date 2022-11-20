<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class MarkdownTextProcessor implements INodeProcessor
{

    /**
     * @inheritDoc
     */
    function Priority(): int
    {
        // TODO: Implement Priority() method.
    }

    /**
     * @inheritDoc
     */
    function ShouldProcess(INode $node, string $scope): bool
    {
        // TODO: Implement ShouldProcess() method.
    }

    /**
     * @inheritDoc
     */
    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        // TODO: Implement Process() method.
    }
}