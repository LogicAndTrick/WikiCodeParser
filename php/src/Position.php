<?php

namespace LogicAndTrick\WikiCodeParser;

class Position
{
    public static function Zero(): Position
    {
        return new Position(0, 0);
    }

    public int $line;
    public int $column;

    public function __construct(int $line, int $column)
    {
        $this->line = $line;
        $this->column = $column;
    }
}
