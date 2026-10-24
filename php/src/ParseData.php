<?php

namespace LogicAndTrick\WikiCodeParser;

class ParseData
{
    private array $values;
    public int $current_element_depth;
    public int $current_tag_depth;

    public function __construct()
    {
        $this->values = [];
        $this->current_element_depth = 0;
        $this->current_tag_depth = 0;
    }

    public function &Get(string $key, callable $defaultValue): mixed
    {
        if (!array_key_exists($key, $this->values)) {
            $this->values[$key] = $defaultValue();
        }
        return $this->values[$key];
    }

    public function Set(string $key, mixed &$value): void
    {
        $this->values[$key] = &$value;
    }
}
