<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

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
        /* @var $node PlainTextNode */
        $text = $node->text;
        $text = preg_replace('/ *<br> */', "\n", $text);

        $ret = [];
        $lines = explode("\n", $text);
        for ($i = 0; $i < count($lines); $i++) {
            $line = $lines[$i];
            $ret[] = new PlainTextNode($line);
            // Don't emit a line break after the final line of the text as it did not end with a newline
            if ($i < count($lines) - 1) $ret[] = new HtmlNode('<br/>', UnprocessablePlainTextNode::NewLine(), '');
        }
        return $ret;
    }
}