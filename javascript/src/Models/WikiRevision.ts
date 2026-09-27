export class WikiRevision {
    public static CreateSlug(text: string) {
        text = text.replace(/ /gi, '_');
        text = text.replace(/[^-$_.+!*'"(),:;<>^{}|~0-9a-z[\]]/gi, '');
        return text;
    }
}
