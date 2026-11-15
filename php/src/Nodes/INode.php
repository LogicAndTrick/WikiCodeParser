<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

interface INode
{
    /**
     * The index of the line (1-indexed) in the source input where this node starts.
     * If the line is 0, then the position is not set.
     */
    public int $line { get; }
    /**
     * The index of the column (0-indexed) in the source input where this node starts.
     */
    public int $column { get; }
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
