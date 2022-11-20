<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class AutoLinkingProcessor implements INodeProcessor
{
    function Priority(): int
    {
        return 9;
    }

    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof PlainTextNode
            && (str_contains($node->text, 'http') || str_contains($node->text, '@'));
    }

    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        /* @var $node PlainTextNode */
        $text = $node->text;

        $ret = [];

        $allMatches = [];

        // an entry in $result:
        // 0 => ['url', 0], url => ['url', 0], 1 => ['url', 0]
        // 0 => ['url', 0], email => ['url', 0], 1 => ['url', 0]

        preg_match_all('/(?<=^|\s)(?<url>https?:\/\/[^\][""\s]+)(?=\s|$)/i', $text, $urlResult, PREG_SET_ORDER | PREG_OFFSET_CAPTURE);
        array_push($allMatches, ...$urlResult);

        preg_match_all('/(?<=^|\s)(?<email>[^\][""\s@]+@[^\][""\s@]+\.[^\][""\s@]+)(?=\s|$)/i', $text, $emailResult, PREG_SET_ORDER | PREG_OFFSET_CAPTURE);
        array_push($allMatches, ...$emailResult);

        usort($allMatches, fn($a, $b) => $a[0][1] - $b[0][1]);

        $start = 0;
        foreach ($allMatches as $urlMatch) {
            $urlMatchIndex = $urlMatch[0][1];
            if ($urlMatchIndex < $start) continue;
            if ($urlMatchIndex > $start) $ret[] = new PlainTextNode(substr($text, $start, $urlMatchIndex - $start));
            if (isset($urlMatch['url'])) {
                $url = $urlMatch['url'][0];
                $enc = HtmlHelper::AttributeEncode($url);
                $ret[] = new HtmlNode("<a href=\"$enc\">", new PlainTextNode($url), '</a>');
            } else if (isset($urlMatch['email'])) {
                $email = $urlMatch['email'][0];
                $enc = HtmlHelper::AttributeEncode($email);
                $ret[] = new HtmlNode("<a href=\"mailto:$enc\">", new PlainTextNode($email), '</a>');
            }
            $start = $urlMatchIndex + strlen($urlMatch[0][0]);
        }
        if ($start < strlen($text)) $ret[] = new PlainTextNode(substr($text, $start));

        return $ret;
    }
}