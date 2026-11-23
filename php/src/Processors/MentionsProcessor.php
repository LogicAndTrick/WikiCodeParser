<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNodeBuilder;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\MetadataNode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\Util;

class MentionsProcessor implements INodeProcessor
{
    public string $urlFormatString;

    public function __construct(string $urlFormatString)
    {
        $this->urlFormatString = $urlFormatString;
    }

    function Priority(): int
    {
        return 12;
    }

    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof PlainTextNode && str_contains($node->text, '@');
    }

    function Process(Parser $parser, ParseData $data, INode|null $parentNode, INode $node, string $scope): array
    {
        if (!($node instanceof PlainTextNode)) {
            return [$node];
        }

        /** @var INode[] $ret */
        $ret = [];
        $text = $node->text;

        /**
         * mention format = @<name>
         * name cannot contain newlines or the '>' character
         * name length must be between 1 and 64
         * name cannot start or end with whitespace
         */

        $state = new State($node->line, $node->column, $text);
        $plainPos = $state->sourcePosition;
        $plain = '';
        while (!$state->Done()) {
            $plain .= $state->ScanTo('@<');
            if ($state->Done() || $state->Peek(2) != '@<') continue;

            $tagStart = $state->sourcePosition;
            $state->Seek(2, false); // @<

            $textStart = $state->sourcePosition;
            $name = $state->ScanTo('>');
            if ($state->Done() || str_contains($name, "\n") || $state->Peek(1) != '>' || strlen($name) < 1 || strlen($name) > 64 || strlen($name) != strlen(Util::Trim($name))) {
                $plain .= '@<' . $name;
                continue;
            }

            $state->Seek(1, false); // >

            if (strlen($plain) > 0) $ret[] = new PlainTextNode($plainPos, $plain);

            $ret[] = new MetadataNode($tagStart, 'Mention', $name);
            $ret[] = HtmlNodeBuilder::make('a', $tagStart)
                ->addClass('mention')
                ->attr('data-name', $name)
                ->attr('href', $parser->configuration->BuildUrl(Util::Template($this->urlFormatString, ['0' => HtmlHelper::UrlEncode($name)])))
                ->add(new UnprocessablePlainTextNode($textStart, '@' . $name))
                ->toNode();

            $plain = '';
            $plainPos = $state->sourcePosition;
        }

        $plain .= $state->ScanToEnd();
        if (strlen($plain) > 0) $ret[] = new PlainTextNode($plainPos, $plain);

        return $ret;
    }
}
