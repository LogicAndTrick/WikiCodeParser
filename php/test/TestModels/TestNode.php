<?php

namespace LogicAndTrick\WikiCodeParser\TestModels;

class TestNode
{
    public string|null $name = null;
    public string $type;
    public array $pos;
    public array|null $children = null;
    public string|null $text = null;
    public array|null $html = null;
    public array|null $plain = null;
    public string|null $key = null;
    public string|array|null $value = null;
}
