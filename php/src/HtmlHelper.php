<?php

namespace LogicAndTrick\WikiCodeParser;

class HtmlHelper
{
    private const ALLOWED_URL_SCHEMES = ['http', 'https', 'mailto', 'ftp'];

    public static function Encode(string $text): string
    {
        $text = str_replace('&', '&amp;', $text);
        $text = str_replace('<', '&lt;', $text);
        $text = str_replace('>', '&gt;', $text);
        $text = str_replace('"', '&quot;', $text);
        $text = str_replace("'", '&#39;', $text);
        return $text;
    }

    public static function UrlEncode(string $text): string
    {
        return urlencode($text);
    }

    public static function AttributeEncode(string $text): string
    {
        return self::Encode($text);
    }

    public static function StripControlCharacters(string $text): string
    {
        return preg_replace('/[\x00-\x1F\x7F]/', '', $text) ?? '';
    }

    public static function GetUrlScheme(string $url): ?string
    {
        return preg_match('/^([a-zA-Z][a-zA-Z0-9+.-]*):/', $url, $match) ? strtolower($match[1]) : null;
    }

    public static function ValidateUrl(string $url): bool
    {
        $scheme = self::GetUrlScheme(self::StripControlCharacters($url));
        return $scheme === null || in_array($scheme, self::ALLOWED_URL_SCHEMES, true);
    }
}
