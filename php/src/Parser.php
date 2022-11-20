<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Elements\Element;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\NodeExtensions;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\Processors\INodeProcessor;
use LogicAndTrick\WikiCodeParser\Tags\Tag;

class Parser
{
    public ParserConfiguration $configuration;

    /**
     * @param $configuration
     */
    public function __construct($configuration)
    {
        $this->configuration = $configuration;
    }

    public function ParseResult(string $text, string $scope = '') : ParseResult {
        $data = new ParseData();
        $text = trim($text);
        $node = $this->ParseElements($data, $text, $scope);
        $node = $this->RunProcessors($node, $data, $scope);
        $res = new ParseResult();
        $res->content = $node;
        return $res;
    }

    public function ParseElements(ParseData $data, string $text, string $scope) : INode {
        $root = new NodeCollection();

        // Elements are line-based scopes, an element cannot start in the middle of a line.
        $text = str_replace("\r", '', $text);

        $lines = new Lines($text);
        $inscope = Util::OrderByDescending(array_filter($this->configuration->elements, fn(Element $x) => $x->InScope($scope)), fn(Element $x) => $x->priority);
        $plain = [];

        while ($lines->Next()) {
            // Try and find an element for this line
            $matched = false;
            /* @var $e Element */
            foreach ($inscope as $e) {
                if (!$e->Matches($lines)) continue;

                $con = $e->Consume($this, $data, $lines, $scope); // found an element, generate the result
                if ($con == null) continue; // no result, guess this element wasn't valid after all

                // if we have any plain text, create a node for it
                if (count($plain) > 0) {
                    $root->nodes[] = self::TrimWhitespace($this->ParseTags($data, trim(implode("\n", $plain)), $scope, TagParseContext::Block));
                    $root->nodes[] = UnprocessablePlainTextNode::NewLine(); // Newline before next element
                }
                $plain = [];

                $root->nodes[] = $con;
                $root->nodes[] = UnprocessablePlainTextNode::NewLine(); // Elements always have a newline after
                $matched = true;
                break;
            }

            if (!$matched) $plain[] = $lines->Value(); // there wasn't any match, so this line was plain text
        }

        // parse any plain text that might be left
        if (count($plain) > 0) $root->nodes[] = self::TrimWhitespace($this->ParseTags($data, trim(implode("\n", $plain)), $scope, TagParseContext::Block));

        // Trim off any whitespace nodes at the end
        $shouldTrim = function() use ($root) {
            if (count($root->nodes) === 0) return false;
            $last = $root->nodes[count($root->nodes) - 1];
            if (!($last instanceof UnprocessablePlainTextNode)) return false;
            return !$last->text || trim($last->text) == '';
        };
        while ($shouldTrim()) {
            array_splice($root->nodes, count($root->nodes) - 1, 1);
        }
        self::FlattenNestedNodeCollections($root);
        return self::TrimWhitespace($root);
    }

    public static function TrimWhitespace(INode $node, bool $start = true, bool $end = true) : INode {
        $removedNodes = [];

        if ($start) {
            NodeExtensions::Walk($node, function (INode $x) use (&$removedNodes) {
                if ($x instanceof NodeCollection) return true;
                if ($x->HasContent()) return false;
                if ($x instanceof UnprocessablePlainTextNode || $x instanceof PlainTextNode) $removedNodes[] = $x;
                return true;
            });
        }

        if ($end) {
            NodeExtensions::WalkBack($node, function (INode $x) use (&$removedNodes) {
                if ($x instanceof NodeCollection) return true;
                if ($x->HasContent()) return false;
                if ($x instanceof UnprocessablePlainTextNode || $x instanceof PlainTextNode) $removedNodes[] = $x;
                return true;
            });
        }

        foreach ($removedNodes as $rem) {
            NodeExtensions::Remove($node, $rem);
        }

        return $node;
    }

    public function ParseTags(ParseData $data, string $text, string $scope, TagParseContext $context) : INode {
        // trim 3 or more newlines down to 2 newlines
        $text = preg_replace('/\n{3,}/i', '\n\n', $text);

        $state = new State($text);
        $root = new NodeCollection();
        $inscope = Util::OrderByDescending(array_filter($this->configuration->tags, fn(Tag $x) => $x->InScope($scope)), fn(Tag $x) => $x->priority);

        while (!$state->Done()) {
            $plain = $state->ScanTo('[');
            if ($plain && $plain != '') $root->nodes[] = new PlainTextNode($plain);
            if ($state->Done()) break;

            $token = $state->GetToken();
            $found = false;
            /* @var $t Tag */
            foreach ($inscope as $t) {
                if ($t->Matches($state, $token, $context)) {
                    $parsed = $t->Parse($this, $data, $state, $scope, $context);
                    if ($parsed != null) {
                        $root->nodes[] = $parsed;
                        $found = true;
                        break;
                    }
                }
            }

            if (!$found) {
                $plain = $state->Next();
                if ($plain && $plain != '') $root->nodes[] = new PlainTextNode($plain);
            }
        }

        return $root;
    }

    public static function FlattenNestedNodeCollections(INode $node) : void {
        if ($node instanceof NodeCollection) {
            /* @var $coll NodeCollection */
            $coll = $node;
            while (Util::Find($coll->nodes, fn(INode $x) => $x instanceof NodeCollection) != null) {
                $coll->nodes = array_merge(...array_map(fn(INode $x) => $x instanceof NodeCollection ? $x->nodes : [$x], $coll->nodes));
            }
        }
        else {
            $ch = $node->GetChildren();
            for ($i = 0; $i < count($ch); $i++) {
                while ($ch[$i] instanceof NodeCollection && count($ch[$i]->nodes) == 1) {
                    /* @var $chcoll NodeCollection */
                    $chcoll = $ch[$i];
                    $node->ReplaceChild($i, $chcoll->nodes[0]);
                    $ch[$i] = $chcoll->nodes[0];
                }
            }
        }

        foreach ($node->GetChildren() as $child) {
            self::FlattenNestedNodeCollections($child);
        }
    }

    public function RunProcessors(INode $node, ParseData $data, string $scope) : INode {
        $processors = Util::OrderByDescending($this->configuration->processors, fn (INodeProcessor $x) => $x->Priority());
        foreach ($processors as $processor) {
            $node = self::RunProcessor($node, $processor, $data, $scope);
        }

        return $node;
    }

    public function RunProcessor(INode $node, INodeProcessor $processor, ParseData $data, string $scope) : INode {
        // If the node can be processed, don't touch subnodes - the processor can invoke RunProcessor if it's needed.
        if ($processor->ShouldProcess($node, $scope)) {
            $result = $processor->Process($this, $data, $node, $scope);
            return count($result) == 1 ? $result[0] : new NodeCollection(...$result);
        }

        $children = $node->GetChildren();

        for ($i = 0; $i < count($children); $i++) {
            $child = $children[$i];
            $processed = self::RunProcessor($child, $processor, $data, $scope);
            $node->ReplaceChild($i, $processed);
        }

        return $node;
    }
}