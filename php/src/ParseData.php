<?php

namespace LogicAndTrick\WikiCodeParser;

class ParseData
{
    private array $values;

    public function __construct() {
        $this->values = [];
    }

    public function &Get(string $key, callable $defaultValue) : mixed {
        if (array_key_exists($key, $this->values)) return $this->values[$key];
        $v = $defaultValue();
        $this->values[$key] = $v;
        return $v;
    }

    public function Set(string $key, mixed &$value) {
        $this->values[$key] = &$value;
    }
}
