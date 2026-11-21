<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use Exception;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\RefNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Position;
use LogicAndTrick\WikiCodeParser\TagParseContext;
use LogicAndTrick\WikiCodeParser\Util;

class TableCell implements INode
{
    public int $line = 0;
    public int $column = 0;
    public string $type;
    public INode $contents;

    public function __construct(int $line, int $column, string $type, INode $contents)
    {
        $this->line = $line;
        $this->column = $column;
        $this->type = $type;
        $this->contents = $contents;
    }

    function ToHtml(): string
    {
        return "<{$this->type}>{$this->contents->ToHtml()}</{$this->type}>";
    }

    function ToPlainText(): string
    {
        return $this->contents->ToPlainText();
    }

    function GetChildren(): array
    {
        return [$this->contents];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i != 0) throw new Exception('Argument out of range');
        $this->contents = $node;
    }

    function HasContent(): bool
    {
        return $this->contents->HasContent();
    }
}

class TableRow implements INode
{
    public int $line = 0;
    public int $column = 0;
    public array $cells;

    public function __construct(int $line, int $column, array $cells)
    {
        $this->line = $line;
        $this->column = $column;
        $this->cells = $cells;
    }

    function ToHtml(): string
    {
        $sb = "<tr>\n";
        foreach ($this->cells as $cell) {
            $sb .= $cell->ToHtml() . "\n";
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
        $value = Util::TrimEnd($lines->Value());
        return strlen($value) >= 2 && $value[0] == '|' && ($value[1] == '=' || $value[1] == '-');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $startLine = $lines->sourceLine;
        $arr = [];
        do {
            $value = $lines->Value();
            if (strlen($value) < 2 || $value[0] != '|' || ($value[1] != '=' && $value[1] != '-')) {
                $lines->Back();
                break;
            }
            $line = $lines->sourceLine;
            $type = $value[1] == '=' ? 'th' : 'td';
            $cells = array_map(fn(array $x) => new TableCell($line, $x['col'], $type, self::ResolveCell($x['text'], $parser, $data, $scope, $line, $x['col'])), self::SplitTable(2, $value));
            $arr[] = new TableRow($line, 0, $cells);
        } while ($lines->Next());

        return new HtmlNode(new Position($startLine, 0), '<div class="table-responsive"><table class="table table-bordered">', new NodeCollection(...$arr), '</table></div>');
    }

    /**
     * @return array<array{col: int, text: string}>
     */
    private static function SplitTable(int $start, string $text): array
    {
        $ret = [];
        $level = 0;
        $last = $start;
        $len =  strlen($text);
        for ($i = $start; $i < $len; $i++) {
            $c = $text[$i];
            if ($c == '[') $level++;
            else if ($c == ']') {
                if ($level > 0) $level--;
            } else if (($c == '|' && $level == 0) || $i == $len - 1) {
                $ret[] = self::CreateTableText($text, $last, $i);
                $last = $i + 1;
            }
        }
        if ($last < $len) $ret[] = self::CreateTableText($text, $last, $i);
        return $ret;
    }

    /**
     * @return array{col: int, text: string}
     */
    private static function CreateTableText(string $text, int $start, int $end): array
    {
        // get the text length
        $col = $start;
        $txt = substr($text, $start, ($end - $start) + ($end == strlen($text) - 1 ? 1 : 0));

        // trim the start of the text - the number of removed chars is how much we need to add to the column
        $ol = strlen($txt);
        $ts = Util::TrimStart($txt);
        $col += $ol - strlen($ts);

        return ['col' => $col, 'text' => Util::TrimEnd($ts)];
    }

    private static function ResolveCell(string $text, Parser $parser, ParseData $data, string $scope, int $line, int $column): INode
    {
        $success = preg_match('/^:ref=([a-z0-9 ]+)$/i', $text, $res);
        if ($success) {
            $name = $res[1];
            return new RefNode($data, $name);
        }
        return $parser->ParseTags($data, $text, $scope, TagParseContext::Block, $line, $column);
    }
}
