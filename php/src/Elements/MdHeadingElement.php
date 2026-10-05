<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use Exception;
use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\TagParseContext;
use LogicAndTrick\WikiCodeParser\Util;

/** @noinspection PhpMultipleClassesDeclarationsInOneFile */
class HeadingNode implements INode
{
    public int $level;
    public string $id;
    public INode $text;

    public function __construct(int $level, string $id, INode $text)
    {
        $this->level = $level;
        $this->id = $id;
        $this->text = $text;
    }

    public function ToHtml(): string
    {
        $escaped = HtmlHelper::AttributeEncode($this->id);
        return "<h{$this->level} id=\"{$escaped}\">{$this->text->ToHtml()}</h{$this->level}>";
    }

    public function ToPlainText(): string
    {
        $plain = $this->text->ToPlainText();
        $plain = str_replace("\n", ' ', $plain);
        // convert to utf-16 so we get consistent character counts between the 3 implementation languages
        // if we wanted to get a true character count, we could use grapheme_strlen() from the intl extension
        // however that is less commonly installed, so we do this instead.
        // divide by 2 since strlen counts bytes and UTF-16LE uses 2 bytes per character
        $length = intval(strlen(mb_convert_encoding($plain, 'UTF-16LE', 'UTF-8')) / 2);
        return $plain . "\n" . str_repeat('-', $length);
    }

    public function GetChildren(): array
    {
        return [$this->text];
    }

    /**
     * @throws Exception
     */
    public function ReplaceChild(int $i, INode $node): void
    {
        if ($i != 0) throw new Exception('Argument out of range');
        $this->text = $node;
    }

    public function HasContent(): bool
    {
        return true;
    }
}

class MdHeadingElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = $lines->Value();
        return strlen($value) > 0 && str_starts_with($value, '=');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $value = Util::Trim($lines->Value());
        $success = preg_match('/^(=+)(.*?)=*$/i', $value, $res);
        if (!$success) {
            return null;
        }

        $level = min(6, strlen($res[1]));
        $text = Util::Trim($res[2]);

        $contents = $parser->ParseTags($data, $text, $scope, TagParseContext::Inline);
        $contentsPlainText = $parser->RunProcessors($contents, $data, $scope)->ToPlainText();
        $id = MdHeadingElement::GetUniqueAnchor($data, $contentsPlainText);
        return new HeadingNode($level, $id, $contents);
    }

    private static function GetUniqueAnchor(ParseData $data, string $text): string
    {
        $key = 'MdHeadingElement.IdList';
        /** @var string[] $anchors */
        $anchors = &$data->Get($key, fn() => []);

        $id = preg_replace('/[^0-9A-Za-z?\\/:@\-._~!$&\'()*+,;=]+/u', '_', $text) ?? '';
        $anchor = $id;
        $inc = 1;
        do {
            // Increment if we have a duplicate
            if (!in_array($anchor, $anchors, true)) break;
            $inc++;
            $anchor = "{$id}_{$inc}";
        } while (true);

        $anchors[] = $anchor;
        return $anchor;
    }
}
