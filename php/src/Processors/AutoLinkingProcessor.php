<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeExtensions;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class AutoLinkingProcessor implements INodeProcessor
{
    function Priority(): int
    {
        return 11;
    }

    function ShouldProcess(INode $node, string $scope): bool
    {
        // handle <a> parent nodes so we skip processing of everything under them
        if ($node instanceof HtmlNode && str_contains($node->htmlBefore, "<a ")) return true;
        return $node instanceof PlainTextNode
            && (str_contains(strtolower($node->text), 'http') || str_contains($node->text, '@'));
    }

    private const AllowedStartChars = [' ', '\n', '\r', '\t', '(', '"'];
    private const AllowedEndChars = [' ', '\n', '\r', '\t', ')', '"', '.', ',', '!', '?'];

    function Process(Parser $parser, ParseData $data, INode|null $parentNode, INode $node, string $scope): array
    {
        if (!($node instanceof PlainTextNode)) {
            return [$node];
        }

        $text = $node->text;

        $ret = [];

        $allMatches = [];

        // an entry in $result:
        // 0 => ['url', 0], url => ['url', 0], 1 => ['url', 0]
        // 0 => ['url', 0], email => ['url', 0], 1 => ['url', 0]

        preg_match_all('/(?<![^\s""(])(?<url>https?:\/\/[^\][""\s]+)(?=[.,!?""]{0,5}(?!\S))/iu', $text, $urlResult, PREG_SET_ORDER | PREG_OFFSET_CAPTURE);
        array_push($allMatches, ...$urlResult);

        preg_match_all('/(?<![^\s""(])(?<email>[^\][""\s@]+@[^\][""\s@.]+(?:\.[^\][""\s@.]+)+)(?=[.,!?""]{0,5}(?!\S))/iu', $text, $emailResult, PREG_SET_ORDER | PREG_OFFSET_CAPTURE);
        array_push($allMatches, ...$emailResult);

        usort($allMatches, fn($a, $b) => $a[0][1] - $b[0][1]);

        $start = 0;
        foreach ($allMatches as $urlMatch) {
            $idx = $urlMatch[0][1];
            $len = strlen($urlMatch[0][0]);
            if ($idx < $start) continue;

            // skip matches starting on the first letter if the previous sibling doesn't end in whitespace or one of the allowed punctuation chars
            if ($idx == 0 && !NodeExtensions::PreviousNodeEndsWith($node, $parentNode, self::AllowedStartChars)) continue;

            // walk back any punctuation or brackets if they are balanced
            while ($len > 0 && AutoLinkingProcessor::ShouldWalkBack($text, $idx, $len)) $len--;

            if (mb_strlen(substr($text, $idx, $len), 'UTF-8') < 5) continue; // urls and emails can't be this short. something is wrong, so skip it.

            // skip matches ending on the last letter if the next sibling doesn't start in whitespace or one of the allowed punctuation chars
            if ($idx + $len == strlen($text) && !NodeExtensions::NextNodeStartsWith($node, $parentNode, self::AllowedEndChars)) continue;

            $matchingText = substr($text, $idx, $len);
            if (isset($urlMatch['url']) && !HtmlHelper::ValidateUrl($matchingText)) continue;

            // return the prefix text
            if ($idx > $start) $ret[] = new PlainTextNode(substr($text, $start, $idx - $start));

            if (isset($urlMatch['url'])) {
                $enc = HtmlHelper::AttributeEncode(HtmlHelper::SanitiseUrl($matchingText));
                $ret[] = new HtmlNode("<a href=\"$enc\">", new UnprocessablePlainTextNode($matchingText), '</a>');
            } else if (isset($urlMatch['email'])) {
                $enc = HtmlHelper::AttributeEncode($matchingText);
                $ret[] = new HtmlNode("<a href=\"mailto:$enc\">", new UnprocessablePlainTextNode($matchingText), '</a>');
            }
            $start = $idx + $len;
        }

        // return the remaining text
        if ($start < strlen($text)) $ret[] = new PlainTextNode(substr($text, $start));

        return $ret;
    }

    private const Punctuation = ['.', '!', '?', ','];
    private const UrlFragments = ['/', '#', '?', '&', ':'];

    private static function ShouldWalkBack(string $text, int $idx, int $len): bool
    {
        $c = $text[$idx + $len - 1]; // last char of the match
        if (in_array($c, self::Punctuation, true)) return true; // english punctuation, unlikely a url ends with these

        // if we get a close paren, then try to balance them
        if ($c === ')') {
            $lvl = 0;
            do {
                $c = $text[$idx + $len - 1];
                if ($c === ')') $lvl++;
                elseif ($c === '(') $lvl--;
                elseif (in_array($c, self::UrlFragments, true)) break; // url/route fragment, assume it can't be balanced
                $len--;
            } while ($len > 0 && $lvl !== 0);
            if ($lvl > 0) return true;
        }
        return false;
    }
}
