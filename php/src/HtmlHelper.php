<?php

namespace LogicAndTrick\WikiCodeParser;

class HtmlHelper
{
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

    private const ALLOWED_URL_SCHEMES = ['http', 'https', 'mailto', 'ftp'];

    public static function ValidateUrl(string $url): bool
    {
        $scheme = self::GetUrlScheme(trim(self::StripControlCharacters($url)));
        return $scheme === null || in_array($scheme, self::ALLOWED_URL_SCHEMES, true);
    }

    /**
     * Sanitises a url such that it always has one of our allowed url schemes (http, https, mailto, ftp).
     * Actual validity of the url is not a consideration for this method - if the URL has no valid scheme,
     * then https:// is added to the front by default.
     */
    public static function SanitiseUrl(?string $url): string
    {
        if ($url === null) return '';
        $scheme = self::GetUrlScheme(trim(self::StripControlCharacters($url)));
        $hasValidScheme = $scheme !== null && in_array($scheme, self::ALLOWED_URL_SCHEMES, true);
        return $hasValidScheme ? trim($url) : 'https://' . trim($url);
    }

    //@formatter:off
    private const ALLOWED_TAG_NAMES = [
        'div', 'span',
        'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
        'a', 'hr', 'br',
        'img', 'video', 'audio',
        'pre', 'code',
        'ul', 'ol', 'li',
        'blockquote',
        'strong', 'em',
        'table', 'thead', 'tbody', 'tfoot', 'tr', 'th', 'td',
    ];
    private const ALLOWED_ATTRIBUTE_NAMES = [
        'id', 'class', 'style',
        'href', 'src',
        'alt', 'title',
        'playsinline', 'controls', 'autoplay', 'loop', 'muted',
    ];
    //@formatter:on

    private const ALLOWED_STYLE_PROPERTY_NAMES = ['font-size', 'color'];

    private const UNSAFE_STYLE_VALUE_REGEX = '/[^#a-zA-Z0-9.]/';

    public static function ValidateTagName(string|null $name): bool
    {
        return $name !== null && in_array($name, self::ALLOWED_TAG_NAMES, true);
    }

    public static function ValidateAttributeName(string|null $name): bool
    {
        if ($name === null) return false;
        if (preg_match('/[^-0-9a-z]/', $name)) return false;
        if (strpos($name, 'data-') === 0) return true;
        return in_array($name, self::ALLOWED_ATTRIBUTE_NAMES, true);
    }

    public static function ValidateStylePropertyName(string|null $name): bool
    {
        return $name !== null && in_array($name, self::ALLOWED_STYLE_PROPERTY_NAMES, true);
    }

    public static function ValidateStyleValue(string|null $value): bool
    {
        return $value !== null && !preg_match(self::UNSAFE_STYLE_VALUE_REGEX, $value);
    }
}
