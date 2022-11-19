<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\TagParseContext;

/** @noinspection PhpMultipleClassesDeclarationsInOneFile */

class ListNode implements INode
{
    public string $tag;
    public array $items;

    public function __construct(string $tag, ListItemNode ...$items)
    {
        $this->tag = $tag;
        $this->items = $items;
    }

    function ToHtml(): string
    {
        $sb = '';
        $sb .= "<{$this->tag}>\n";
        foreach ($this->items as $item) $sb .= $item->ToHtml();
        $sb .= "</{$this->tag}>\n";
        return $sb;
    }

    function ToPlainText(): string
    {
        return $this->ToPlainTextPrefixed('');
    }

    public function ToPlainTextPrefixed(string $prefix): string
    {
        $st = $prefix . ($this->tag == 'ol' ? '#' : '-');
        $sb = '';
        foreach ($this->items as $item) $sb .= $item->ToPlainTextPrefixed($st);
        return $sb;
    }

    function GetChildren(): array
    {
        return [...$this->items];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        $this->items[$i] = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}

class ListItemNode implements INode
{
    public INode $content;
    public array $subtrees;

    public function __construct(INode $content)
    {
        $this->content = $content;
        $this->subtrees = [];
    }

    function ToHtml(): string
    {
        $sb = '<li>';
        $sb .= $this->content->ToHtml();
        foreach ($this->subtrees as $st) $sb .= $st->ToHtml();
        $sb .= "</li>\n";
        return $sb;
    }

    function ToPlainText(): string
    {
        throw new \Exception('Invalid operation');
    }

    public function ToPlainTextPrefixed(string $prefix): string
    {
        $sb = $prefix . ' ';
        $sb .= $this->content->ToPlainText() . '\n';
        foreach ($this->subtrees as $st) $sb .= $st->ToPlainTextPrefixed($prefix);
        return $sb;
    }

    function GetChildren(): array
    {
        return [$this->content, ...$this->subtrees];
    }

    function ReplaceChild(int $i, INode $node): void
    {
        if ($i == 0) $this->content = $node;
        else $this->subtrees[$i - 1] = $node;
    }

    function HasContent(): bool
    {
        return true;
    }
}

class MdListElement extends Element
{
    private static array $ulTokens = ['*', '-'];
    private static array $olTokens = ['#'];

    private static function IsUnsortedToken(string $c): bool
    {
        return in_array($c, self::$ulTokens);
    }

    private static function IsSortedToken(string $c): bool
    {
        return in_array($c, self::$olTokens);
    }

    private static function IsListToken(string $c): bool
    {
        return self::IsUnsortedToken($c) || self::IsSortedToken($c);
    }

    private static function IsValidListItem(string $value, int $currentLevel): int
    {
        $len = strlen($value);
        if ($len == 0) return 0;

        $tokens = 0;
        $foundSpace = false;
        for ($i = 0; $i < $len; $i++) {
            $c = $value[$i];
            if (self::IsListToken($c)) {
                $tokens++;
                continue;
            }

            if ($c == ' ') {
                $foundSpace = true;
                break;
            }

            return 0;
        }

        if ($foundSpace && $tokens > 0 && $tokens <= $currentLevel + 1) return $tokens;
        return 0;
    }

    public function Matches(Lines $lines): bool
    {
        $value = trim($lines->Value());
        return self::IsValidListItem($value, 0) > 0;
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();

        // Put all the subtrees into a dummy item node
        $item = new ListItemNode(PlainTextNode::Empty());
        $this->CreateListItems($item, '', $parser, $data, $lines, $scope);

        if (count($item->subtrees) == 0) {
            $lines->SetCurrent($current);
            return null;
        }

        // Pull the subtrees out again for the result
        if (count($item->subtrees) == 1) return $item->subtrees[0];
        return new NodeCollection(...$item->subtrees);
    }

    private function CreateListItems(?ListItemNode $lastItemNode, string $prefix, Parser $parser, ParseData $data, Lines $lines, string $scope)
    {
        $ret = [];
        do {
            $value = rtrim($lines->Value());

            if (!str_starts_with($value, $prefix)) {
                // No longer valid for this list
                $lines->Back();
                break;
            }

            $value = substr($value, strlen($prefix)); // strip the prefix off the line

            // Possibilities:
            // empty string : not valid - stop parsing
            // first character is whitespace : trim and create list item
            // first character is list token, second character is whitespace: create sublist
            // anything else : not valid - stop parsing

            if (strlen($value) > 1 && $value[0] == ' ' && strlen($prefix) > 0) { // don't allow this if we're parsing at level 0
                // List item
                $value = ltrim($value);

                // Support for continuations
                while (str_ends_with($value, '^')) {
                    if (str_ends_with($value, '\\^')) { // super basic way to escape continuations
                        $value = substr($value, 0, strlen($value) - 2) . '^';
                        break;
                    } else if ($lines->Next()) {
                        $value = trim(substr($value, 0, strlen($value) - 1)) . "\n" . ltrim($lines->Value());
                    } else {
                        break;
                    }
                }

                $pt = $parser->ParseTags($data, trim($value), $scope, TagParseContext::Block);
                $lastItemNode = new ListItemNode($pt);
                $ret[] = $lastItemNode;
            } else if (strlen($value) > 2 && self::IsListToken($value[0]) && $value[1] == ' ' && $lastItemNode != null) {
                // Sublist
                $tag = self::IsSortedToken($value[0]) ? 'ol' : 'ul';
                $sublist = new ListNode($tag, ...$this->CreateListItems($lastItemNode, $prefix . $value[0], $parser, $data, $lines, $scope));
                $lastItemNode->subtrees[] = $sublist;
            } else {
                // Cannot parse this line, list is complete
                $lines->Back();
                break;
            }
        } while ($lines->Next());
        return $ret;
    }
}
