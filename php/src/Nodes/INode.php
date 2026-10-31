<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

interface INode
{
    /**
     * Convert the node to HTML. The resulting output may *not* be sanitised - its possible
     * malicious input could include injection attacks. Use at your own risk.
     */
    function ToHtml(): string;
    function ToPlainText(): string;
    /**
     * @return INode[]
     */
    function GetChildren(): array;
    function ReplaceChild(int $i, INode $node): void;
    function HasContent(): bool;
}
