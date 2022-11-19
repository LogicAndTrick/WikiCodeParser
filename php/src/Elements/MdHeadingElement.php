<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use Exception;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\TagParseContext;

/** @noinspection PhpMultipleClassesDeclarationsInOneFile */
class HeadingNode implements INode {
    public int $level;
    public string $id;
    public INode $text;

    public function __construct(int $level, string $id, INode $text) {
        $this->level = $level;
        $this->id = $id;
        $this->text = $text;
    }

    public function ToHtml() : string {
        return "<h{$this->level} id=\"{$this->id}\">{$this->text->ToHtml()}</h{$this->level}>";
    }

    public function ToPlainText(): string {
        $plain = $this->text->ToPlainText();
        return $plain . '\n' . str_repeat('-', strlen($plain));
    }

    public function GetChildren(): array {
        return [ $this->text ];
    }

    /**
     * @throws Exception
     */
    public function ReplaceChild(int $i, INode $node): void {
        if ($i != 0) throw new Exception('Argument out of range');
        $this->text = $node;
    }

    public function HasContent(): bool {
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
        $value = trim($lines->Value());
        $success = preg_match('/^(=+)(.*?)=*$/i', $value, $res);
        if (!$success) {
            return null;
        }

        $level = min(6, strlen($res[1]));
        $text = trim($res[2]);

        $contents = $parser->ParseTags($data, $text, $scope, TagParseContext::Inline);
        $contents = $parser->RunProcessors($contents, $data, $scope);
        $id = MdHeadingElement::GetUniqueAnchor($data, $contents->ToPlainText());
        return new HeadingNode($level, $id, $contents);
    }

    private static function GetUniqueAnchor(ParseData $data, string $text) : string
    {
        $key = 'MdHeadingElement.IdList';
        $anchors = &$data->Get($key, fn() => []);

        $id = preg_replace('/[^\\da-z?\\/:@\-._~!$&\'()*+,;=]/i', '_', $text);
        $anchor = $id;
        $inc = 1;
        do {
            // Increment if we have a duplicate
            if (!in_array($anchor, $anchors)) break;
            $inc++;
            $anchor = "${id}_${inc}";
        } while (true);

        $anchors[] = $anchor;
        return $anchor;
    }
}