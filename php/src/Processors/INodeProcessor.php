<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

interface INodeProcessor
{
    /**
     * Higher priority processors are run first.
     */
    function Priority() : int;

    /**
     * Return true if the given node should be processed by this processor
     */
    function ShouldProcess(INode $node, string $scope) : bool;

    /**
     * Process a node and return the nodes that will replace it.
     * @return INode[]
     */
    function Process(Parser $parser, ParseData $data, INode $node, string $scope) : array;
}
