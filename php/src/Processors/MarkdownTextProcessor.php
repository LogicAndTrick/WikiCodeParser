<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\Util;

class MarkdownTextProcessor implements INodeProcessor
{

    private static array $Tokens = ['`', '*', '/', '_', '~'];
    private static array $OpenTags = ['<code>', '<strong>', '<em>', '<span class="underline">', '<span class="strikethrough">'];
    private static array $CloseTags = ['</code>', '</strong>', '</em>', '</span>', '</span>'];
    private static array $StartBreakChars = ['!', '^', '(', ')', '+', '=', '[', ']', '{', '}', '"', '\'', '<', '>', '?', ',', '.', ' ', "\t", "\r", "\n"];
    private static array $ExtraEndBreakChars = [':', ';'];

    function Priority(): int
    {
        return 10;
    }

    function ShouldProcess(INode $node, string $scope): bool
    {
        return $node instanceof PlainTextNode && Util::IndexOfAny($node->text, self::$Tokens) >= 0;
    }

    private static function GetTokenIndex(string $c): int {
        $s = array_search($c, self::$Tokens);
        if ($s === false) return -1;
        return $s;
    }
    private static function IsStartBreakChar(string $c): bool {
        return in_array($c, self::$StartBreakChars);
    }
    private static function IsEndBreakChar(string $c): bool {
        return in_array($c, self::$StartBreakChars) || in_array($c, self::$ExtraEndBreakChars) || in_array($c, self::$Tokens);
    }

    /**
     * @param int $tracker
     * @param string $text
     * @param int $position
     * @param int $endPosition
     * @return INode|null
     */
    private static function ParseToken(array &$tracker, string $text, int $position, int &$endPosition): ?INode {
        $endPosition = -1;
        $token = $text[$position];
        $tokenIndex = self::GetTokenIndex($token);

        // Make sure we're not already in this token
        if ($tracker[$tokenIndex] != 0) return null;

        $endToken = strpos($text, $token, $position + 1);
        if ($endToken <= $position + 1) return null;
        $posCheck = strpos(substr($text, $position, $endToken - $position), "\n");
        if ($posCheck !== false && $posCheck >= 0) return null; // no newlines

        // Make sure we can close this token
        $valid = ($endToken + 1 == strlen($text) || self::IsEndBreakChar($text[$endToken + 1])) // end of string or before an end breaker
                 && trim($text[$endToken - 1]) != ''; // not whitespace previous
        if (!$valid) return null;

        $str = substr($text, $position + 1, $endToken - $position - 1);

        $tracker[$tokenIndex] = 1;

        // code tokens cannot be nested
        /** @var ?INode $contents */
        if ($token == '`') {
            $contents = new UnprocessablePlainTextNode($str);
        } else {
            $toks = self::ParseTokens($tracker, $str);
            $contents = count($toks) == 1 ? $toks[0] : new NodeCollection(...$toks);
        }

        $tracker[$tokenIndex] = 0;

        $endPosition = $endToken;

        return new HtmlNode(self::$OpenTags[$tokenIndex], $contents, self::$CloseTags[$tokenIndex]);
    }

    /**
     * @param int[] $tracker
     * @param string $text
     * @return INode[]
     */
    private static function ParseTokens(array &$tracker, string $text): array {
        $ret = [];
        $plainStart = 0;
        $index = 0;

        while (true) {
            $nextIndex = Util::IndexOfAny($text, self::$Tokens, $index);
            if ($nextIndex < 0) break;

            // Make sure we can start a new token
            $valid = ($nextIndex == 0 || self::IsStartBreakChar($text[$nextIndex - 1])) // start of string or after a start breaker
                && $nextIndex + 1 < strlen($text) // not end of string
                && trim($text[$nextIndex + 1]) != ''; // not whitespace next
            if (!$valid) {
                $index = $nextIndex + 1;
                continue;
            }

            $endIndex = -1;
            $parsed = self::ParseToken($tracker, $text, $nextIndex, $endIndex);
            if ($parsed == null) {
                $index = $nextIndex + 1; // no match, skip this token
            } else {
                if ($plainStart < $nextIndex) $ret[] = new PlainTextNode(substr($text, $plainStart, $nextIndex - $plainStart));
                $ret[] = $parsed;
                $index = $plainStart = $endIndex + 1;
            }
        }
        // Return the rest of the text as plain
        if ($plainStart < strlen($text)) $ret[] = new PlainTextNode(substr($text, $plainStart));

        return $ret;
    }

    function Process(Parser $parser, ParseData $data, INode $node, string $scope): array
    {
        /** @var PlainTextNode $node */
        $text = $node->text;

        $ret = [];

        $nextIndex = Util::IndexOfAny($text, self::$Tokens, 0);
        if ($nextIndex < 0) {
            // Short circuit
            $ret[] = $node;
            return $ret;
        }

        /*
         * Like everything else here, this isn't exactly markdown, but it's close.
         * _underline_
         * /italics/
         * *bold*
         * ~strikethrough~
         * `code`
         * Very simple rules: no newlines, must start/end on a word boundary, code tags cannot be nested
         */

        // pre-condition: start of a line OR one of: !?^()+=[]{}"'<>,. OR whitespace
        // first and last character is NOT whitespace. everything else is fine except for newlines
        // post-condition: end of a line OR one of: !?^()+=[]{}"'<>,.:; OR whitespace

        $tracker = array_map(fn(string $x) => 0, self::$Tokens);

        foreach (self::ParseTokens($tracker, $text) as $token) {
            $ret[] = $token;
        }

        return $ret;
    }
}