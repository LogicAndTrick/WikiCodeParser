<?php

namespace LogicAndTrick\WikiCodeParser\Processors;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\Nodes\UnprocessablePlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
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

    private static function GetTokenIndex(string $c): int
    {
        $s = array_search($c, self::$Tokens);
        if ($s === false) return -1;
        return intval($s);
    }
    private static function IsStartBreakChar(string $c): bool
    {
        return in_array($c, self::$StartBreakChars);
    }
    private static function IsEndBreakChar(string $c): bool
    {
        return in_array($c, self::$StartBreakChars) || in_array($c, self::$ExtraEndBreakChars) || in_array($c, self::$Tokens);
    }

    /**
     * @param array<int> $tracker
     */
    private static function ParseToken(array &$tracker, State $state): ?INode
    {
        $tokStart = $state->sourcePosition;
        $index = $state->index;
        $token = $state->Next();
        $tokenIndex = self::GetTokenIndex($token);

        // Make sure we're not already in this token
        if ($tracker[$tokenIndex] != 0) {
            $state->Seek($index, true);
            return null;
        }

        $strStart = $state->sourcePosition;
        $str = $state->ScanTo($token);

        // the contents cannot be empty or contain newlines, and the next char must be the token
        if ($state->Next() != $token || strlen($str) == 0 || strpos($str, "\n") !== false) {
            $state->Seek($index, true);
            return null;
        }

        // Make sure we can close this token
        $valid = ($state->Done || self::IsEndBreakChar($state->Peek(1))) // end of string or before an end breaker
            && strlen(Util::TrimEnd($str)) == strlen($str); // not whitespace previous
        if (!$valid) {
            $state->Seek($index, true);
            return null;
        }

        $tracker[$tokenIndex] = 1;

        // code tokens cannot be nested
        if ($token == '`') {
            $contents = new UnprocessablePlainTextNode($strStart, $str);
        } else {
            $toks = self::ParseTokens($strStart->line, $strStart->column, $tracker, $str);
            $contents = count($toks) == 1 ? $toks[0] : new NodeCollection(...$toks);
        }

        $tracker[$tokenIndex] = 0;

        $ret = new HtmlNode($tokStart, self::$OpenTags[$tokenIndex], $contents, self::$CloseTags[$tokenIndex]);
        $ret->plainBefore = $token;
        $ret->plainAfter = $token;
        return $ret;
    }

    /**
     * @param array<int> $tracker
     * @param string $text
     * @return INode[]
     */
    private static function ParseTokens(int $line, int $column, array &$tracker, string $text): array
    {
        /** @var INode[] $ret */
        $ret = [];
        $state = new State($line, $column, $text);
        $plainPos = $state->sourcePosition;
        $plain = "";
        $advanceToken = false;
        while (!$state->Done) {
            // always advance one token except at the start, or after we find a valid token
            if ($advanceToken) $plain .= $state->Next();
            $advanceToken = true;

            $plain .= $state->ScanToAny(self::$Tokens);
            if ($state->Done) break;

            // Make sure we can start a new token
            $valid = ($state->index == 0 || self::IsStartBreakChar($state->text[$state->index - 1])) // start of string or after a start breaker
                && $state->index + 1 < strlen($text) // not end of string
                && Util::Trim($state->text[$state->index + 1]) != ""; // not whitespace next
            if (!$valid) continue;

            // Try to parse a token
            $parsed = self::ParseToken($tracker, $state);
            if ($parsed === null) continue;

            if (strlen($plain) > 0) $ret[] = new PlainTextNode($plainPos, $plain);
            $ret[] = $parsed;

            $advanceToken = false;
            $plain = "";
            $plainPos = $state->sourcePosition;
        }
        // Return the rest of the text as plain
        $plain .= $state->ScanToEnd();
        if (strlen($plain) > 0) $ret[] = new PlainTextNode($plainPos, $plain);

        return $ret;
    }

    function Process(Parser $parser, ParseData $data, INode|null $parentNode, INode $node, string $scope): array
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

        $tracker = array_map(fn() => 0, self::$Tokens);

        foreach (self::ParseTokens($node->line, $node->column, $tracker, $text) as $token) {
            $ret[] = $token;
        }

        return $ret;
    }
}
