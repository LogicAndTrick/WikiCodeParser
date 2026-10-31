<?php

namespace LogicAndTrick\WikiCodeParser\Nodes;

use LogicAndTrick\WikiCodeParser\HtmlHelper;

class HtmlNodeBuilder
{
    /** @var array<string, string> */
    private array $attributes;
    /** @var string[] */
    private array $props;
    /** @var string[] */
    private array $classes;
    /** @var array<string, string> */
    private array $styles;

    public string $tag;

    public function GetAttributes(): array
    {
        return $this->attributes;
    }

    public function GetProps(): array
    {
        return $this->props;
    }

    public function GetClasses(): array
    {
        return $this->classes;
    }

    public function GetStyles(): array
    {
        return $this->styles;
    }

    /** @var INode[] */
    public array $contents;

    public static function Make(string $tag): HtmlNodeBuilder
    {
        return new HtmlNodeBuilder($tag);
    }

    public function __construct(string $tag)
    {
        if (!HtmlHelper::ValidateTagName($tag)) throw new \InvalidArgumentException('Invalid tag.');

        $this->tag = $tag;
        $this->attributes = [];
        $this->props = [];
        $this->classes = [];
        $this->styles = [];
        $this->contents = [];
    }

    public function If(bool $test, callable $action): HtmlNodeBuilder
    {
        if ($test) $action($this);
        return $this;
    }

    public function AddClass(string ...$classes): HtmlNodeBuilder
    {
        return $this->AddClasses($classes);
    }

    /** @param iterable<string> $classes */
    public function AddClasses(iterable $classes): HtmlNodeBuilder
    {
        foreach ($classes as $class) {
            if (!in_array($class, $this->classes, true)) $this->classes[] = $class;
        }
        return $this;
    }

    /**
     * Styles are not sanitised, be sure to handle them with care!
     */
    public function AddStyle(string $prop, string $val): HtmlNodeBuilder
    {
        if (!HtmlHelper::ValidateStylePropertyName($prop)) throw new \InvalidArgumentException('Invalid property.');
        if (!HtmlHelper::ValidateStyleValue($val)) throw new \InvalidArgumentException('Invalid value.');
        $this->styles[$prop] = $val;
        return $this;
    }

    public function Attr(string $key, string|null $val): HtmlNodeBuilder
    {
        if (!HtmlHelper::ValidateAttributeName($key)) throw new \InvalidArgumentException('Invalid attribute.');
        if (($key === 'href' || $key === 'src') && !HtmlHelper::ValidateUrl($val ?? '')) throw new \InvalidArgumentException('Invalid url.');
        if ($key === 'class') throw new \InvalidArgumentException('Use AddClass instead.');
        if ($key === 'style') throw new \InvalidArgumentException('Use AddStyle instead.');
        $this->attributes[$key] = $val ?? '';
        return $this;
    }

    public function DelAttr(string|null $key): HtmlNodeBuilder
    {
        if ($key !== null) unset($this->attributes[$key]);
        return $this;
    }

    public function Prop(string $prop, bool $on = true): HtmlNodeBuilder
    {
        if (!HtmlHelper::ValidateAttributeName($prop)) throw new \InvalidArgumentException('Invalid prop name.');
        if ($prop === 'class') throw new \InvalidArgumentException('Use AddClass instead.');
        if ($prop === 'style') throw new \InvalidArgumentException('Use AddStyle instead.');
        $index = array_search($prop, $this->props, true);
        if ($index !== false) unset($this->props[$index]);
        $this->props = array_values($this->props);
        if ($on) $this->props[] = $prop;
        return $this;
    }

    /** @param INode|iterable<INode> $nodes */
    public function Add(INode|iterable $nodes): HtmlNodeBuilder
    {
        if (!is_iterable($nodes)) $nodes = [$nodes];
        foreach ($nodes as $n) $this->contents[] = $n;
        return $this;
    }

    public function ToNode(
        bool $isBlockNode = false,
        string $plainBefore = '',
        string $plainAfter = '',
        bool $selfClosing = false
    ): HtmlNode {
        if (count($this->contents) > 0) $selfClosing = false;

        $tag = HtmlHelper::ValidateTagName($this->tag) ? $this->tag : 'div';
        $content = count($this->contents) === 1 ? $this->contents[0] : new NodeCollection(...$this->contents);
        $html = '<' . $tag;
        if (count($this->classes) > 0) {
            $classes = $this->classes;
            sort($classes);
            $html .= ' class="' . HtmlHelper::AttributeEncode(implode(' ', $classes)) . '"';
        }
        if (count($this->styles) > 0) {
            $html .= ' style="';
            $keys = array_keys($this->styles);
            sort($keys);
            foreach ($keys as $prop) {
                $prop = (string)$prop;
                if (!HtmlHelper::ValidateStylePropertyName($prop)) continue;
                if (!HtmlHelper::ValidateStyleValue($this->styles[$prop])) continue;
                $html .= HtmlHelper::AttributeEncode($prop . ':' . $this->styles[$prop] . ';');
            }
            $html .= '"';
        }
        $keys = array_keys($this->attributes);
        sort($keys);
        foreach ($keys as $attr) {
            $attr = (string)$attr;
            if ($attr === 'class' || $attr === 'style') continue;
            if (!HtmlHelper::ValidateAttributeName($attr)) continue;
            $val = $this->attributes[$attr];
            if ($attr === 'href' || $attr === 'src') {
                if (!HtmlHelper::ValidateUrl($val)) continue;
                $val = HtmlHelper::SanitiseUrl($val);
            }
            $html .= ' ' . $attr . '="' . HtmlHelper::AttributeEncode($val) . '"';
        }
        $props = $this->props;
        sort($props);
        foreach ($props as $prop) {
            if ($prop === 'class' || $prop === 'style' || array_key_exists($prop, $this->attributes)) continue;
            if (!HtmlHelper::ValidateAttributeName($prop)) continue;
            $html .= ' ' . $prop;
        }
        $after = '</' . $tag . '>';
        if ($selfClosing) {
            $html .= ' /';
            $after = '';
        }
        $html .= '>';

        $node = new HtmlNode($html, $content, $after);
        $node->isBlockNode = $isBlockNode;
        $node->plainBefore = $plainBefore;
        $node->plainAfter = $plainAfter;
        return $node;
    }
}
