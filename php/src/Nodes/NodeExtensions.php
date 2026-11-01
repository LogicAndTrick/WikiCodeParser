<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class NodeExtensions
{
    public static function Remove(INode $root, INode $remove): bool
    {
        $children = $root->GetChildren();
        $idx = array_search($remove, $children, true);
        if ($idx !== false && $idx >= 0) {
            $root->ReplaceChild(intval($idx), new RemovedNode($remove));
            return true;
        }
        foreach ($children as $ch) {
            if (NodeExtensions::Remove($ch, $remove)) return true;
        }
        return false;
    }

    public static function Walk(INode $node, callable $visitor): bool
    {
        if ($visitor($node) === false) return false;
        foreach ($node->GetChildren() as $child) {
            if (NodeExtensions::Walk($child, $visitor) === false) return false;
        }
        return true;
    }

    public static function WalkBack(INode $node, callable $visitor): bool
    {
        $rev = array_reverse($node->GetChildren());
        foreach ($rev as $child) {
            if (NodeExtensions::WalkBack($child, $visitor) === false) return false;
        }
        if ($visitor($node) === false) return false;
        return true;
    }

    public static function PreviousSibling(INode $node, INode|null $parentNode): ?INode
    {
        if ($parentNode === null) return null;
        $children = $parentNode->GetChildren();
        $idx = array_search($node, $children, true);
        if ($idx === false || $idx <= 0 || !is_integer($idx)) return null;
        return $children[$idx - 1];
    }

    public static function NextSibling(INode $node, INode|null $parentNode): ?INode
    {
        if ($parentNode === null) return null;
        $children = $parentNode->GetChildren();
        $idx = array_search($node, $children, true);
        if ($idx === false || $idx >= count($children) - 1 || !is_integer($idx)) return null;
        return $children[$idx + 1];
    }

    public static function PreviousNodeEndsWith(INode $node, INode|null $parentNode, array $chars): bool
    {
        $prev = $node;
        while (($prev = self::PreviousSibling($prev, $parentNode)) !== null) {
            if ($prev instanceof PlainTextNode && strlen($prev->text) > 0) return in_array($prev->text[-1], $chars, true);
            if ($prev instanceof UnprocessablePlainTextNode && strlen($prev->text) > 0) return in_array($prev->text[-1], $chars, true);
            if ($prev->HasContent()) return false;
        }
        return true;
    }

    public static function NextNodeStartsWith(INode $node, INode|null $parentNode, array $chars): bool
    {
        $next = $node;
        while (($next = self::NextSibling($next, $parentNode)) !== null) {
            if ($next instanceof PlainTextNode && strlen($next->text) > 0) return in_array($next->text[0], $chars, true);
            if ($next instanceof UnprocessablePlainTextNode && strlen($next->text) > 0) return in_array($next->text[0], $chars, true);
            if ($next->HasContent()) return false;
        }
        return true;
    }
}
