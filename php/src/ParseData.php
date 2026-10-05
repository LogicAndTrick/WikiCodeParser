<?php

namespace LogicAndTrick\WikiCodeParser;

class ParseData
{
    private array $values;

    public function __construct()
    {
        $this->values = [];
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
