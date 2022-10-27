
export class WikiRevision {
    public static CreateSlug(text : string) {
        text = text.replace(' ', '_');
        text = text.replace(/[^-$_.+!*'"(),:;<>^{}|~0-9a-z[\]]/ig, '');
        return text;
    }
}
