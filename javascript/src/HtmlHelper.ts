const ALLOWED_URL_SCHEMES = ['http', 'https', 'mailto', 'ftp'];

// prettier-ignore
const ALLOWED_TAG_NAMES = new Set([
    'div', 'span',
    'h1', 'h2', 'h3', 'h4', 'h5', 'h6',
    'a', 'hr', 'br',
    'img', 'video', 'audio',
    'pre', 'code',
    'ul', 'ol', 'li',
    'blockquote',
    'strong', 'em',
    'table', 'thead', 'tbody', 'tfoot', 'tr', 'th', 'td',
]);

// prettier-ignore
const ALLOWED_ATTRIBUTE_NAMES = new Set([
    'id', 'class', 'style',
    'href', 'src',
    'alt', 'title',
    'playsinline', 'controls', 'autoplay', 'loop', 'muted'
]);

const ALLOWED_STYLE_PROPERTY_NAMES = new Set(['font-size', 'color']);

const UNSAFE_STYLE_VALUE_REGEX = /[^#a-zA-Z0-9.]/;

export class HtmlHelper {
    public static Encode(text: string): string {
        text = text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
        return text;
    }

    public static UrlEncode(text: string): string {
        return encodeURI(text);
    }

    public static AttributeEncode(text: string): string {
        return HtmlHelper.Encode(text);
    }

    public static StripControlCharacters(text: string): string {
        // eslint-disable-next-line no-control-regex
        return text == null ? text : text.replace(/[\x00-\x1F\x7F]/g, '');
    }

    public static GetUrlScheme(url: string): string | null {
        if (url == null) return null;
        const match = url.match(/^([a-zA-Z][a-zA-Z0-9+.-]*):/);
        return match ? match[1].toLowerCase() : null;
    }

    public static ValidateUrl(url: string): boolean {
        if (url == null) return false;
        const scheme = HtmlHelper.GetUrlScheme(HtmlHelper.StripControlCharacters(url).trim());
        return scheme == null || ALLOWED_URL_SCHEMES.includes(scheme);
    }

    /**
     * Sanitises a url such that it always has one of our allowed url schemes (http, https, mailto, ftp).
     * Actual validity of the url is not a consideration for this method - if the URL has no valid scheme,
     * then https:// is added to the front by default.
     */
    public static SanitiseUrl(url: string): string {
        if (url == null) return '';
        const scheme = HtmlHelper.GetUrlScheme(HtmlHelper.StripControlCharacters(url).trim());
        const hasValidScheme = scheme != null && ALLOWED_URL_SCHEMES.includes(scheme);
        return hasValidScheme ? url.trim() : 'https://' + url.trim();
    }

    public static ValidateTagName(name: string | null): boolean {
        if (name == null) return false;
        return ALLOWED_TAG_NAMES.has(name);
    }

    public static ValidateAttributeName(name: string | null): boolean {
        if (name == null) return false;
        if (/[^-0-9a-z]/.test(name)) return false;
        if (name.startsWith('data-')) return true;
        return ALLOWED_ATTRIBUTE_NAMES.has(name);
    }

    public static ValidateStylePropertyName(name: string | null): boolean {
        return name != null && ALLOWED_STYLE_PROPERTY_NAMES.has(name);
    }

    public static ValidateStyleValue(value: string | null): boolean {
        return value != null && !UNSAFE_STYLE_VALUE_REGEX.test(value);
    }
}
