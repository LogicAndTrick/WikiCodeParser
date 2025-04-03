<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\RefNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class TableRow implements INode
{
    public string $type;
    public array $cells;

    public function __construct(string $type, array $cells)
    {
        $this->type = $type;
        $this->cells = $cells;
    }

    function ToHtml(): string
    {
        $sb = "<tr>\n";
        foreach ($this->cells as $cell) {
            $sb .= "<{$this->type}>{$cell->ToHtml()}</{$this->type}>\n";
        }
        $sb .= "</tr>\n";
        return $sb;
    }

    function ToPlainText(): string
    {
        $sb = '';
        $first = true;
        foreach ($this->cells as $cell) {
            if (!$first) $sb .= ' | ';
            $sb .= $cell->ToPlainText();
            $first = false;
        }
        $sb .= "\n";
        return $sb;
    }

    function GetChildren(): array
    {
        return [...$this->cells];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        $this->cells[$i] = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}

class MdTableElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = rtrim($lines->Value());
        return strlen($value) >= 2 && $value[0] == '|' && ($value[1] == '=' || $value[1] == '-');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $arr = [];
        do {
            $value = rtrim($lines->Value());
            if (strlen($value) < 2 || $value[0] != '|' || ($value[1] != '=' && $value[1] != '-')) {
                $lines->Back();
                break;
            }
            $cells = array_map(fn (string $x) => self::ResolveCell($x, $parser, $data, $scope), self::SplitTable(substr($value, 2)));
            $arr[] = new TableRow($value[1] == '=' ? 'th' : 'td', $cells);
        } while ($lines->Next());

        return new HtmlNode('<div class="table-responsive"><table class="table table-bordered">', new NodeCollection(...$arr), '</table></div>');
    }

    private static function SplitTable(string $text) : array {
        $ret = [];
        $level = 0;
        $last = 0;
        $text = trim($text);
        $len =  strlen($text);
        for ($i = 0; $i < $len; $i++) {
            $c = $text[$i];
            if ($c == '[') $level++;
            else if ($c == ']') $level--;
            else if (($c == '|' && $level == 0) || $i == $len - 1) {
                $ret[] = trim(substr($text, $last, ($i - $last) + ($i == $len - 1 ? 1 : 0)));
                $last = $i + 1;
            }
        }
        if ($last < $len) $ret[] = trim(substr($text, $last, ($i - $last) + ($i == $len - 1 ? 1 : 0)));
        return $ret;
    }

    private static function ResolveCell(string $text, Parser $parser, ParseData $data, string $scope): INode {
        $success = preg_match('/^:ref=([a-z0-9 ]+)$/i', trim($text), $res);
        if ($success) {
            $name = $res[1];
            return new RefNode($data, $name);
        }
        return $parser->ParseTags($data, $text, $scope, TagParseContext::Block);
    }
}