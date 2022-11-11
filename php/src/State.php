<?php

namespace LogicAndTrick\WikiCodeParser;

/**
 * Character-indexed state tracker for a string
 */
class State
{
    public string $text;
    public int $length;
    public int $index;
    public function Done() : bool { return $this->index >= $this->length; }

    public function __construct(string $text)
    {
        $this->text = $text;
        $this->length = strlen($text);
        $this->index = 0;
    }

    public function ScanTo(string $find, bool $ignoreCase = false) : string
    {
        $pos = $ignoreCase
            ? strpos(strtolower($this->text), strtolower($find), $this->index)
            : strpos($this->text, $find, $this->index);
        if ($pos === false) $pos = $this->length;
        $ret = substr($this->text, $this->index, $pos - $this->index);
        $this->index = $pos;
        return $ret;
    }

    public function SkipWhitespace() : void
    {
        while ($this->index < $this->length && trim($this->text[$this->index]) == '') $this->index++;
    }

    public function PeekTo(string $find) : string | null
    {
        $pos = strpos($this->text, $find);
        if ($pos === false) return null;
        return substr($this->text, $this->index, $pos - $this->index);
    }

    public function Seek(int $index, bool $fromStart) : void
    {
        $this->index = $fromStart ? $index : $this->index + $index;
    }

    public function Peek(int $count) : string
    {
        if ($this->index + $count > $this->length) $count = $this->length - $this->index;
        return substr($this->text, $this->index, $count);
    }

    public function  Next() : string
    {
        if ($this->index >= $this->length) return '\0';
        return $this->text[$this->index++];
    }

    public function GetToken() : string | null
    {
        if ($this->Done() || $this->text[$this->index] != '[') return null;
        $found = false;
        $tok = "";
        for ($i = $this->index + 1; $i < min($this->index + 10, $this->length); $i++)
        {
            $c = $this->text[$i];
            if ($c == ' ' || $c == '=' || $c == ']')
            {
                $found = strlen($tok) > 0;
                break;
            }

            $tok .= $c;
        }

        return $found ? strtolower($tok) : null;
    }
}