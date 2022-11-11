<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

class NodeExtensions
{
    public static function Remove(INode $root, INode $remove): bool {
        $children = $root->GetChildren();
        $idx = array_search($remove, $children, true);
        if ($idx >= 0) {
            $root->ReplaceChild($idx, new RemovedNode($remove));
            return true;
        }
        foreach ($children as $ch) {
            if (NodeExtensions::Remove($ch, $remove)) return true;
        }
        return false;
    }

    public static function Walk(INode $node, callable $visitor): bool {
        if ($visitor($node) === false) return false;
        foreach ($node->GetChildren() as $child) {
            if (NodeExtensions::Walk($child, $visitor) === false) return false;
        }
        return true;
    }

    public static function WalkBack(INode $node, callable $visitor): bool {
        $rev = array_reverse($node->GetChildren());
        foreach ($rev as $child) {
            if (NodeExtensions::WalkBack($child, $visitor) === false) return false;
        }
        if ($visitor($node) === false) return false;
        return true;
    }
}