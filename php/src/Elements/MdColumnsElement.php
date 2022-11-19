<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

/** @noinspection PhpMultipleClassesDeclarationsInOneFile */
class ColumnNode implements INode {
    public int $width;
    public INode $content;

    public function __construct(int $width, INode $content) {
        $this->width = $width;
        $this->content = $content;
    }
    function ToHtml(): string
    {
        return "<div class=\"col-md-{$this->width}\">\n{$this->content->ToHtml()}</div>\n";
    }

    function ToPlainText(): string
    {
        return $this->content->ToPlainText();
    }

    function GetChildren(): array
    {
        return [$this->content];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i != 0) throw new \Exception('Argument out of range');
        $this->content = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}

class MdColumnsElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = $lines->Value();
        return str_starts_with($value, '%%columns=');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();

        $meta = substr($lines->Value(), 10);
        $colDefs = array_map(fn(string $x) => intval($x, 10) ?? 0, explode(':', $meta));
        $total = 0;

        foreach ($colDefs as $d) {
            if ($d > 0) {
                $total += $d;
            } else {
                $lines->SetCurrent($current);
                return null;
            }
        }

        if ($total != 12) {
            $lines->SetCurrent($current);
            return null;
        }

        $i = 0;

        $arr = [];
        $cols = [];
        while ($lines->Next() && $i < count($colDefs)) {
            $value = rtrim($lines->Value());
            if ($value == '%%') {
                $cols[] = new ColumnNode($colDefs[$i], $parser->ParseElements($data, implode("\n", $arr), $scope));
                $arr = [];
                $i++;
            } else {
                $arr[] = $value;
            }
            if ($i >= count($colDefs)) break;
        }

        if ($i != count($colDefs) || count($arr) > 0) {
            $lines->SetCurrent($current);
            return null;
        }

        return new HtmlNode('<div class="row">', new NodeCollection(...$cols), '</div>');
    }
}