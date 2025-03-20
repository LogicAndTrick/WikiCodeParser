<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\NodeCollection;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class QuoteElement extends Element
{
    private const OPEN_QUOTE_REGEX = '/^\[quote(?:(?: name)?=([^]]*))?\]/i';
    private const CLOSE_QUOTE_LENGTH = 8;

    public function Matches(Lines $lines): bool
    {
        $value = trim($lines->value());
        return strlen($value) > 6 && stripos($value, "[quote") === 0 && preg_match(self::OPEN_QUOTE_REGEX, $value);
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->current();
        $arr = [];

        $line = trim($lines->value());
        if (!preg_match(self::OPEN_QUOTE_REGEX, $line, $res)) {
            $lines->setCurrent($current);
            return null;
        }

        $text = self::BalanceQuotes($lines, $author, $postfix);
        if ($text === null) {
            $lines->setCurrent($current);
            return null;
        }

        $before = "<blockquote>";
        $plainBefore = "[quote]\n";
        if ($author !== null && trim($author) !== '') {
            $before .= "<strong class=\"quote-name\">" . $author . " said:</strong><br/>";
            $plainBefore = $author . " said: " . $plainBefore;
        }

        $node = new HtmlNode($before, $parser->parseElements($data, $text, $scope), "</blockquote>");
        $node->plainBefore = $plainBefore;
        $node->plainAfter = "\n[/quote]";
        $node->isBlockNode = true;

        if ($postfix !== null && trim($postfix) !== '') {
            return new NodeCollection($node, $parser->parseTags($data, $postfix, $scope, TagParseContext::Inline));
        }
        return $node;
    }

    public static function BalanceQuotes(Lines $lines, &$name, &$postfix): string|null
    {
        $name = null;
        $postfix = null;

        $line = ltrim($lines->value());
        if (!preg_match(self::OPEN_QUOTE_REGEX, $line, $openMat)) return null;
        if (isset($openMat[1])) $name = $openMat[1];

        $line = substr($line, strlen($openMat[0]));
        $arr = [];
        $currentLevel = 1;
        do {
            $idx = 0;
            do {
                $openMatSuccess = preg_match(self::OPEN_QUOTE_REGEX, $line, $openMat, PREG_OFFSET_CAPTURE, $idx);
                $openMatIdx = $openMatSuccess ? $openMat[0][1] : -1;
                $closeMatIdx = stripos($line, "[/quote]", $idx);

                if ($openMatIdx >= 0 && ($closeMatIdx === false || $closeMatIdx > $openMatIdx)) {
                    // Open quote
                    $currentLevel++;
                    $idx = $openMat[0][1] + strlen($openMat[0][0]);
                } elseif ($closeMatIdx !== false) {
                    // Close quote
                    $currentLevel--;
                    if ($currentLevel == 0) {
                        if (strlen($line) > $closeMatIdx + self::CLOSE_QUOTE_LENGTH) {
                            $postfix = substr($line, $closeMatIdx + self::CLOSE_QUOTE_LENGTH);
                        }
                        $arr[] = substr($line, 0, $closeMatIdx);
                        return implode("\n", $arr);
                    }
                    $idx = $closeMatIdx + self::CLOSE_QUOTE_LENGTH;
                } else {
                    $arr[] = $line;
                    break;
                }
            } while (true);

            if ($lines->next()) $line = $lines->value();
            else break;
        } while (true);

        return null;
    }
}
