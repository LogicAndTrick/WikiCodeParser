
export type WikiRevisionCreditType = 'c' | 'a' | 'f';

export class WikiRevisionCredit {
    public static readonly TypeCredit : WikiRevisionCreditType = 'c';
    public static readonly TypeArchive : WikiRevisionCreditType = 'a';
    public static readonly TypeFull : WikiRevisionCreditType = 'f';

    public ID : number;
    public Type : WikiRevisionCreditType;
    public RevisionID : number;
    public Description : string;
    public UserID : number;
    public Name : string;
    public Url : string;
    public WaybackUrl : string;
}
