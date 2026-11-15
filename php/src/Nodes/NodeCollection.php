<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class NodeCollection implements INode
{
    public int $line {
        get => count($this->nodes) == 0 ? 0 : $this->nodes[0]->line;
    }
    public int $column {
        get => count($this->nodes) == 0 ? 0 : $this->nodes[0]->column;
    }
    /**
     * @var INode[]
     */
    public array $nodes;

    public function __construct(INode ...$nodes)
    {
        $this->nodes = $nodes;
    }

    function ToHtml(): string
    {
        return implode('', array_map(fn(INode $x) => $x->ToHtml(), $this->nodes));
    }

    function ToPlainText(): string
    {
        return implode('', array_map(fn(INode $x) => $x->ToPlainText(), $this->nodes));
    }

    /**
     * @inheritDoc
     */
    function GetChildren(): array
    {
        return $this->nodes;
    }

    function ReplaceChild(int $i, INode $node): void
    {
        $this->nodes[$i] = $node;
    }

    function HasContent(): bool
    {
        foreach ($this->nodes as $n) {
            if ($n->HasContent()) return true;
        }
        return false;
    }
}
