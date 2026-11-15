<?php

namespace LogicAndTrick\WikiCodeParser;

class Lines
{
    /**
     * @var string[]
     */
    public array $content;

    public int $index;

    private int $startSourceLine;
    public int $sourceLine {
        get => $this->startSourceLine + max(0, $this->index);
    }

    public function __construct(int $startSourceLine, string $content)
    {
        $this->content = explode("\n", $content);
        $this->index = -1;
        $this->startSourceLine = $startSourceLine;
    }
    public function Back(): void
    {
        $this->index--;
    }
    public function Next(): bool
    {
        return ++$this->index < count($this->content);
    }
    public function Value(): string
    {
        return $this->content[$this->index];
    }
    public function Current(): int
    {
        return $this->index;
    }
    public function SetCurrent(int $index): void
    {
        $this->index = $index;
    }
    public function IsEmpty(): bool
    {
        return Util::Trim($this->Value()) === '';
    }
}
