<?php

namespace LogicAndTrick\WikiCodeParser;

use Symfony\Component\HtmlSanitizer\Parser\NativeParser;
use Symfony\Component\HtmlSanitizer\TextSanitizer\StringSanitizer;
use Symfony\Component\HtmlSanitizer\Visitor\Model\Cursor;
use Symfony\Component\HtmlSanitizer\Visitor\Node\DocumentNode;
use Symfony\Component\HtmlSanitizer\Visitor\Node\Node;
use Symfony\Component\HtmlSanitizer\Visitor\Node\NodeInterface;
use Symfony\Component\HtmlSanitizer\Visitor\Node\TextNode;

/**
 * A fake version of the sanitiser that doesn't sanitise anything, but formats things the same way.
 * Source: https://github.com/symfony/symfony/blob/8.1/src/Symfony/Component/HtmlSanitizer/Visitor/DomVisitor.php
 * Copyright (c) 2021-present Fabien Potencier
 */
class FakeSanitiser
{
    public static function Sanitise(string $html): string
    {
        $html = str_replace(\chr(0), '�', $html);
        $parser = new NativeParser();
        if (trim($html) == '') return '';

        $parsed = $parser->parse($html);
        if (!$parsed) return '';

        return self::visit($parsed)?->render() ?? '';
    }

    private static function visit(\Dom\Node|\DOMNode $domNode): ?NodeInterface
    {
        $cursor = new Cursor(new DocumentNode());
        self::visitChildren($domNode, $cursor);
        return $cursor->node;
    }

    private static function visitNode(\Dom\Node|\DOMNode $domNode, Cursor $cursor): void
    {
        $nodeName = StringSanitizer::htmlLower($domNode->nodeName);
        if (self::enterNode($nodeName, $domNode, $cursor)) {
            self::visitChildren($domNode, $cursor);
            // @phpstan-ignore method.nonObject
            $cursor->node = $cursor->node->getParent();
        }
    }

    private static function enterNode(string $domNodeName, \Dom\Node|\DOMNode $domNode, Cursor $cursor): bool
    {
        // @phpstan-ignore argument.type
        $node = new Node($cursor->node, $domNodeName);
        $domAttributes = $domNode->attributes?->getIterator() ?? [];
        foreach ($domAttributes as $attribute) {
            $name = StringSanitizer::htmlLower($attribute->name);
            $value = $attribute->value;
            $node->setAttribute($name, $value);
        }
        // @phpstan-ignore method.nonObject
        $cursor->node->addChild($node);
        $cursor->node = $node;

        return true;
    }

    private static function visitChildren(\Dom\Node|\DOMNode $domNode, Cursor $cursor): void
    {
        /** @var \Dom\Node|\DOMNode $child */
        foreach ($domNode->childNodes ?? [] as $child) {
            if ('#text' === $child->nodeName) {
                // @phpstan-ignore method.nonObject, argument.type, argument.type
                $cursor->node->addChild(new TextNode($cursor->node, $child instanceof \Dom\Node ? ($child->textContent ?? '') : $child->nodeValue));
            } elseif ($child instanceof \Dom\Element || $child instanceof \DOMElement) {
                self::visitNode($child, $cursor);
            }
        }
    }
}
