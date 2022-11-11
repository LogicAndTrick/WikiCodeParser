<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

interface INode
{
    function ToHtml() : string;
    function ToPlainText() : string;
    /**
     * @return INode[]
     */
    function GetChildren() : array;
    function ReplaceChild(int $i, INode $node) : void;
    function HasContent() : bool;
}
