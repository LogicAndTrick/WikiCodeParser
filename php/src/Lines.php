<?php

namespace LogicAndTrick\WikiCodeParser;

class Lines
{
    /**
     * @var string[]
     */
    public array $content;

    public int $index;

    public function __construct(string $content)
    {
        $this->content = explode("\n", $content);
        $this->index = -1;
    }
    public function Back() {
        $this->index--;
    }
    public function Next() : bool {
        return ++$this->index < count($this->content);
    }
    public function Value() : string {
        return $this->content[$this->index];
    }
    public function Current() : int {
        return $this->index;
    }
    public function SetCurrent(int $index) {
        $this->index = $index;
    }
}