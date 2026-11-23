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
use LogicAndTrick\WikiCodeParser\Util;

class Parser
{
    public ParserConfiguration $configuration;

    /**
     * @param ParserConfiguration $configuration
     */
    public function __construct(ParserConfiguration $configuration)
    {
        $this->configuration = $configuration;
    }

    public function ParseResult(string $text, string $scope = ''): ParseResult
    {
        $data = new ParseData();
        $node = $this->ParseElements($data, $text, $scope, 1);
        $node = $this->RunProcessors(null, $node, $data, $scope);
        $res = new ParseResult();
        $res->content = $node;
        return $res;
    }

    public function ParseElements(ParseData $data, string $text, string $scope, int $line): INode
    {
        $data->current_element_depth++;
        $root = new NodeCollection();

        // Elements are line-based scopes, an element cannot start in the middle of a line.
        // this is fine to do for line numbers as long as the \r characters at the end of the line, which is normal.
        // \r in the middle of a line will cause column positions to be wrong, but column positions are nice-to-haves anyway, so we don't really need to handle that.
        $text = str_replace("\r", '', $text);

        $lines = new Lines($line, $text);
        $inscope = Util::OrderByDescending(array_filter($this->configuration->elements, fn(Element $x) => $x->InScope($scope)), fn(Element $x) => $x->priority);
        if ($data->current_element_depth > $this->configuration->max_element_depth) $inscope = [];

        $plainStart = $lines->sourceLine;
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
                    $root->nodes[] = self::TrimWhitespace($this->ParseTags($data, implode("\n", $plain), $scope, TagParseContext::Block, $plainStart, 0));
                    $root->nodes[] = UnprocessablePlainTextNode::NewLine(new Position($plainStart + count($plain) - 1, strlen($plain[count($plain) - 1]))); // Newline before next element
                }
                $plain = [];
                $plainStart = $lines->sourceLine + 1; // plain starts on the next line

                $root->nodes[] = $con;
                $root->nodes[] = UnprocessablePlainTextNode::NewLine(new Position($lines->sourceLine, 0)); // Elements always have a newline after
                $matched = true;
                break;
            }

            if (!$matched) $plain[] = $lines->Value(); // there wasn't any match, so this line was plain text
        }

        // parse any plain text that might be left
        if (count($plain) > 0) $root->nodes[] = self::TrimWhitespace($this->ParseTags($data, implode("\n", $plain), $scope, TagParseContext::Block, $plainStart, 0));

        // Trim off any whitespace nodes at the end
        $shouldTrim = function () use ($root) {
            if (count($root->nodes) === 0) return false;
            $last = $root->nodes[count($root->nodes) - 1];
            if (!($last instanceof UnprocessablePlainTextNode)) return false;
            return !$last->text || trim($last->text) == '';
        };
        while ($shouldTrim()) {
            array_splice($root->nodes, count($root->nodes) - 1, 1);
        }
        self::FlattenNestedNodeCollections($root);
        $ret = self::TrimWhitespace($root);

        $data->current_element_depth--;
        return $ret;
    }

    public static function TrimWhitespace(INode $node, bool $start = true, bool $end = true): INode
    {
        $removedNodes = [];

        if ($start) {
            NodeExtensions::Walk($node, function (INode $x) use (&$removedNodes) {
                if ($x instanceof NodeCollection) return true;
                if ($x instanceof UnprocessablePlainTextNode && $x->text !== null) $x->text = Util::TrimStart($x->text);
                if ($x instanceof PlainTextNode && $x->text !== null) $x->text = Util::TrimStart($x->text);
                if ($x->HasContent()) return false;
                if ($x instanceof UnprocessablePlainTextNode || $x instanceof PlainTextNode) $removedNodes[] = $x;
                return true;
            });
        }

        if ($end) {
            NodeExtensions::WalkBack($node, function (INode $x) use (&$removedNodes) {
                if ($x instanceof NodeCollection) return true;
                if ($x instanceof UnprocessablePlainTextNode && $x->text !== null) $x->text = Util::TrimEnd($x->text);
                if ($x instanceof PlainTextNode && $x->text !== null) $x->text = Util::TrimEnd($x->text);
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

    public function ParseTags(ParseData $data, string $text, string $scope, TagParseContext $context, int $line, int $column): INode
    {
        $data->current_tag_depth++;

        $state = new State($line, $column, $text);
        $root = new NodeCollection();
        $inscope = Util::OrderByDescending(array_filter($this->configuration->tags, fn(Tag $x) => $x->InScope($scope)), fn(Tag $x) => $x->priority);
        if ($data->current_tag_depth > $this->configuration->max_tag_depth) $inscope = [];

        while (!$state->Done) {
            $pos = $state->sourcePosition;
            $plain = $state->ScanTo('[');
            if ($plain !== null && $plain !== '') {
                $p = new PlainTextNode($pos, $plain);
                $p->line = $pos->line;
                $p->column = $pos->column;
                $root->nodes[] = $p;
            }
            if ($state->Done) break;

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
                if ($plain && $plain != '') {
                    $p = new PlainTextNode($pos, $plain);
                    $p->line = $pos->line;
                    $p->column = $pos->column;
                    $root->nodes[] = $p;
                }
            }
        }

        $data->current_tag_depth--;
        return $root;
    }

    public static function FlattenNestedNodeCollections(INode $node): void
    {
        if ($node instanceof NodeCollection) {
            /* @var $coll NodeCollection */
            $coll = $node;
            while (Util::Find($coll->nodes, fn(INode $x) => $x instanceof NodeCollection) != null) {
                $coll->nodes = array_merge(...array_map(fn(INode $x) => $x instanceof NodeCollection ? $x->nodes : [$x], $coll->nodes));
            }
        } else {
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

    public function RunProcessors(INode|null $parentNode, INode $node, ParseData $data, string $scope): INode
    {
        $processors = Util::OrderByDescending($this->configuration->processors, fn(INodeProcessor $x) => $x->Priority());
        foreach ($processors as $processor) {
            $node = $this->RunProcessor($parentNode, $node, $processor, $data, $scope);
        }

        return $node;
    }

    public function RunProcessor(INode|null $parentNode, INode $node, INodeProcessor $processor, ParseData $data, string $scope): INode
    {
        // If the node can be processed, don't touch subnodes - the processor can invoke RunProcessor if it's needed.
        if ($processor->ShouldProcess($node, $scope)) {
            $result = $processor->Process($this, $data, $parentNode, $node, $scope);
            return count($result) == 1 ? $result[0] : new NodeCollection(...$result);
        }

        $children = $node->GetChildren();

        for ($i = 0; $i < count($children); $i++) {
            $child = $children[$i];
            $processed = $this->RunProcessor($node, $child, $processor, $data, $scope);
            $node->ReplaceChild($i, $processed);
        }

        return $node;
    }
}
