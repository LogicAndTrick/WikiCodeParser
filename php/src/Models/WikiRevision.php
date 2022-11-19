<?php

namespace LogicAndTrick\WikiCodeParser\Models;

class WikiRevision
{
    public static function CreateSlug(string $text): string
    {
        $text = str_replace(' ', '_', $text);
        $text = preg_replace('/[^-$_.+!*\'"(),:;<>^{}|~0-9a-z[\]]/i', '', $text);
        return $text;
    }
}