<?php

namespace LogicAndTrick\WikiCodeParser;

class HtmlHelper
{
    private static function escapeEmoji(string $text): string
    {
        $re = '/[\x{1F600}-\x{1F64F}\x{1F680}-\x{1F6FF}\x{24C2}-\x{1F251}\x{1F900}-\x{1F9FF}\x{1F300}-\x{1F5FF}\x{1FA70}-\x{1FAF6}]/u';
        return preg_replace_callback($re, fn (array $match) => '&#' . mb_ord($match[0]) . ';', $text);
    }

    public static function Encode(string $text): string
    {
        $text = str_replace('&', '&amp;', $text);
        $text = str_replace('<', '&lt;', $text);
        $text = str_replace('>', '&gt;', $text);
        $text = str_replace('"', '&quot;', $text);
        $text = str_replace("'", '&#39;', $text);
        return HtmlHelper::escapeEmoji($text);
    }

    public static function UrlEncode(string $text): string
    {
        return urlencode($text);
    }

    public static function AttributeEncode(string $text): string
    {
        $text = str_replace('&', '&amp;', $text);
        $text = str_replace('<', '&lt;', $text);
        $text = str_replace('>', '&gt;', $text);
        $text = str_replace('"', '&quot;', $text);
        $text = str_replace("'", '&#39;', $text);
        return HtmlHelper::escapeEmoji($text);
    }
}
