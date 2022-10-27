
function escapeEmoji(str : string) {
    return str.replace(/\p{Emoji_Presentation}/ugm, s => '&#' +s.codePointAt(0) + ';');
}

export class HtmlHelper {
    public static Encode(text: string): string {
        text = text.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
        return escapeEmoji(text);
    }

    public static UrlEncode(text: string): string {
        return encodeURI(text);
    }

    public static AttributeEncode(text: string): string {
        text = text.replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
        return escapeEmoji(text);
    }
}
