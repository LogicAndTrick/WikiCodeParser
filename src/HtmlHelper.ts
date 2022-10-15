
export class HtmlHelper {
    public static Encode(text: string): string {
        return text.replace('&', '&amp;')
            .replace('<', '&lt;')
            .replace('>', '&gt;')
            .replace('"', '&quot;')
            .replace('\\', '&#39;');
    }

    public static UrlEncode(text: string): string {
        return encodeURI(text);
    }

    public static AttributeEncode(text: string): string {
        return text.replace('&', '&amp;')
            .replace('<', '&lt;')
            .replace('>', '&gt;')
            .replace('"', '&quot;')
            .replace('\\', '&#39;');
    }
}
