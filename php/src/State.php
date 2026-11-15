<?php

namespace LogicAndTrick\WikiCodeParser;

use LogicAndTrick\WikiCodeParser\Position;

/**
 * Character-indexed state tracker for a string
 */
class State
{
    public string $text;
    public int $length;
    public int $index;
    public function Done(): bool
    {
        return $this->index >= $this->length;
    }

    private array $newlinePositions;
    private int $startSourceLine;
    private int $startSourceColumn;

    public function __construct(int $startSourceLine, int $startSourceColumn, string $text)
    {
        $this->text = $text;
        $this->length = strlen($text);
        $this->index = 0;

        $this->startSourceLine = $startSourceLine;
        $this->startSourceColumn = $startSourceColumn;
        // get the positions of every newline character, so we can count them
        $nlpos = [-1]; // lines are 1-indexed, we always start with a newline
        $idx = strpos($text, "\n");
        while ($idx !== false) {
            $nlpos[] = $idx;
            $idx = strpos($text, "\n", $idx + 1);
        }
        $this->newlinePositions = $nlpos;
    }

    public int $sourceLine {
        get {
            $count = 0;
            foreach ($this->newlinePositions as $x) {
                if ($x < $this->index) $count++;
            }
            return $this->startSourceLine + $count - 1; // the input line is 1-indexed too, so subtract 1 here.
        }
    }

    public int $sourceColumn {
        get {
            $last = 0;
            foreach ($this->newlinePositions as $x) {
                if ($x < $this->index) $last = $x;
            }
            $last += 1;
            return $this->index - $last + ($last == 0 ? $this->startSourceColumn : 0);
        }
    }

    public Position $sourcePosition {
        get => new Position($this->sourceLine, $this->sourceColumn);
    }

    /**
     * @phpstan-impure
     */
    public function ScanTo(string $find, bool $ignoreCase = false): string
    {
        $pos = $ignoreCase
            ? strpos(strtolower($this->text), strtolower($find), $this->index)
            : strpos($this->text, $find, $this->index);
        if ($pos === false) $pos = $this->length;
        $ret = substr($this->text, $this->index, $pos - $this->index);
        $this->index = $pos;
        return $ret;
    }

    /**
     * @phpstan-impure
     */
    public function SkipWhitespace(): void
    {
        while ($this->index < $this->length && trim($this->text[$this->index]) == '') $this->index++;
    }

    public function PeekTo(string $find): string | null
    {
        $pos = strpos($this->text, $find, $this->index);
        if ($pos === false) return null;
        return substr($this->text, $this->index, $pos - $this->index);
    }

    /**
     * @phpstan-impure
     */
    public function Seek(int $index, bool $fromStart): void
    {
        $this->index = $fromStart ? $index : $this->index + $index;
    }

    public function Peek(int $count): string
    {
        if ($this->index + $count > $this->length) $count = $this->length - $this->index;
        return substr($this->text, $this->index, $count);
    }

    /**
     * @phpstan-impure
     */
    public function  Next(): string
    {
        if ($this->index >= $this->length) return "\0";
        return $this->text[$this->index++];
    }

    public function GetToken(): string | null
    {
        if ($this->Done() || $this->text[$this->index] != '[') return null;
        $found = false;
        $tok = "";
        for ($i = $this->index + 1; $i < min($this->index + 10, $this->length); $i++) {
            $c = $this->text[$i];
            if ($c == ' ' || $c == '=' || $c == ']') {
                $found = strlen($tok) > 0;
                break;
            }

            $tok .= $c;
        }

        return $found ? strtolower($tok) : null;
    }
}
