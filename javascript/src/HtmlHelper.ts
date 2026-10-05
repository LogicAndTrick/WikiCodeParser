const ALLOWED_URL_SCHEMES = ['http', 'https', 'mailto', 'ftp'];

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
        const scheme = HtmlHelper.GetUrlScheme(HtmlHelper.StripControlCharacters(url));
        return scheme == null || ALLOWED_URL_SCHEMES.includes(scheme);
    }
}
