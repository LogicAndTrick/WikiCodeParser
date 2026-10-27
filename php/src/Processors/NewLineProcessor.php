<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Util;

class NewLineProcessor implements INodeProcessor
{
    /**
     * @inheritDoc
     */
    function Priority(): int
    {
        return 1;
    }

    /**
     * @inheritDoc
     */
    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof PlainTextNode && (str_contains($node->text, "\n") || str_contains($node->text, '<br>'));
    }

    /**
     * @inheritDoc
     */
    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        if (!($node instanceof PlainTextNode)) throw new \InvalidArgumentException('Node is not a PlainTextNode');
        $text = $node->text ?? '';

        $lines = [];
        foreach (explode("\n", $text) as $line) {
            $breaks = explode('<br>', $line);
            for ($i = 0; $i < count($breaks); $i++) {
                $s = $breaks[$i];
                if ($i > 0) $s = Util::TrimStart($s);
                if ($i < count($breaks) - 1) $s = Util::TrimEnd($s);
                $lines[] = $s;
            }
        }

        $ret = [];
        for ($i = 0; $i < count($lines); $i++) {
            $line = $lines[$i];
            $ret[] = new PlainTextNode($line);
            // Don't emit a line break after the final line of the text as it did not end with a newline
            if ($i < count($lines) - 1) $ret[] = new HtmlNode('<br/>', UnprocessablePlainTextNode::NewLine(), '');
        }
        return $ret;
    }
}
