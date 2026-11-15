import { describe, expect, test } from '@jest/globals';
import { readFileSync } from 'fs';
import {
    AlignTag,
    AutoLinkingProcessor,
    CodeTag,
    ColorTag,
    Element,
    FontTag,
    HtmlNode,
    ImageTag,
    INode,
    Lines,
    LinkTag,
    ListTag,
    MarkdownTextProcessor,
    MdCodeElement,
    MdColumnsElement,
    MdHeadingElement,
    MdLineElement,
    MdListElement,
    MdPanelElement,
    MdQuoteElement,
    MdTableElement,
    MetadataNode,
    NewLineProcessor,
    NodeCollection,
    ParseData,
    Parser,
    ParserConfiguration,
    ParseResult,
    PlainTextNode,
    PreElement,
    PreTag,
    QuickLinkTag,
    QuoteElement,
    QuoteTag,
    RefElement,
    RemovedNode,
    SizeTag,
    SmiliesProcessor,
    SpoilerTag,
    Tag,
    TrimWhitespaceAroundBlockNodesProcessor,
    UnprocessablePlainTextNode,
    VaultEmbedTag,
    WikiArchiveTag,
    WikiBookTag,
    WikiCategoryTag,
    WikiCreditTag,
    WikiFileTag,
    WikiImageTag,
    WikiLinkTag,
    WikiYoutubeTag,
    YoutubeTag,
} from '../src';
import { JsonNode, TestConfig, TestDefinition, TestNode } from './TestModels';
import { GetTestCaseDirectory, GetTestCaseNames, sanitise } from './Util';

const lineOffsets: number[] = [0, 1, 5, 10];

function GetTestCases(): [string, number][] {
    return GetTestCaseNames('implementations').flatMap(x => lineOffsets.map<[string, number]>(y => [x, y]));
}

function CreateConfig(config: TestConfig): ParserConfiguration {
    let con: ParserConfiguration;

    switch (config.base) {
        case 'twhl':
            con = ParserConfiguration.Twhl('https://twhl.info');
            break;
        case 'snarkpit':
            con = ParserConfiguration.Snarkpit('https://snarkpit.net');
            break;
        default:
            con = new ParserConfiguration('https://example.com');
            break;
    }

    for (const e of config.elements ?? []) {
        switch (e) {
            case 'MdCode':
                con.Elements.push(new MdCodeElement());
                break;
            case 'MdColumns':
                con.Elements.push(new MdColumnsElement());
                break;
            case 'MdHeading':
                con.Elements.push(new MdHeadingElement());
                break;
            case 'MdLine':
                con.Elements.push(new MdLineElement());
                break;
            case 'MdList':
                con.Elements.push(new MdListElement());
                break;
            case 'MdPanel':
                con.Elements.push(new MdPanelElement());
                break;
            case 'MdQuote':
                con.Elements.push(new MdQuoteElement());
                break;
            case 'MdTable':
                con.Elements.push(new MdTableElement());
                break;
            case 'Pre':
                con.Elements.push(new PreElement());
                break;
            case 'Quote':
                con.Elements.push(new QuoteElement());
                break;
            case 'Ref':
                con.Elements.push(new RefElement());
                break;
            default:
                throw new Error(`Unknown element name '${e}'`);
        }
    }

    for (const t of config.tags ?? []) {
        switch (t) {
            case 'Strong':
                con.Tags.push(new Tag('b', 'strong').WithScopes('inline', 'excerpt'));
                break;
            case 'Emphasis':
                con.Tags.push(new Tag('i', 'em').WithScopes('inline', 'excerpt'));
                break;
            case 'Underline':
                con.Tags.push(new Tag('u', 'span', 'underline').WithScopes('inline', 'excerpt'));
                break;
            case 'Strikethrough':
                con.Tags.push(new Tag('s', 'span', 'strikethrough').WithScopes('inline', 'excerpt'));
                break;
            case 'Align':
                con.Tags.push(new AlignTag());
                break;
            case 'Code':
                con.Tags.push(new CodeTag());
                break;
            case 'Color':
                con.Tags.push(new ColorTag());
                break;
            case 'Font':
                con.Tags.push(new FontTag());
                break;
            case 'Image':
                con.Tags.push(new ImageTag());
                break;
            case 'Link':
                con.Tags.push(new LinkTag());
                break;
            case 'List':
                con.Tags.push(new ListTag());
                break;
            case 'Pre':
                con.Tags.push(new PreTag());
                break;
            case 'QuickLink':
                con.Tags.push(new QuickLinkTag());
                break;
            case 'Quote':
                con.Tags.push(new QuoteTag());
                break;
            case 'Size':
                con.Tags.push(new SizeTag());
                break;
            case 'Spoiler':
                con.Tags.push(new SpoilerTag());
                break;
            case 'VaultEmbed':
                con.Tags.push(new VaultEmbedTag());
                break;
            case 'WikiArchive':
                con.Tags.push(new WikiArchiveTag());
                break;
            case 'WikiBook':
                con.Tags.push(new WikiBookTag());
                break;
            case 'WikiCategory':
                con.Tags.push(new WikiCategoryTag());
                break;
            case 'WikiCredit':
                con.Tags.push(new WikiCreditTag());
                break;
            case 'WikiFile':
                con.Tags.push(new WikiFileTag());
                break;
            case 'WikiImage': {
                const tag = new WikiImageTag();
                tag.TwhlBehaviour = true;
                con.Tags.push(tag);
                break;
            }
            case 'WikiLink':
                con.Tags.push(new WikiLinkTag());
                break;
            case 'WikiYoutube':
                con.Tags.push(new WikiYoutubeTag());
                break;
            case 'Youtube':
                con.Tags.push(new YoutubeTag());
                break;
            default:
                throw new Error(`Unknown tag name '${t}'`);
        }
    }

    for (const p of config.processors ?? []) {
        switch (p) {
            case 'AutoLinking':
                con.Processors.push(new AutoLinkingProcessor());
                break;
            case 'MarkdownText':
                con.Processors.push(new MarkdownTextProcessor());
                break;
            case 'NewLine':
                con.Processors.push(new NewLineProcessor());
                break;
            case 'Smilies':
                con.Processors.push(new SmiliesProcessor('/smilies/{0}.png'));
                break;
            case 'TrimWhitespace':
                con.Processors.push(new TrimWhitespaceAroundBlockNodesProcessor());
                break;
            default:
                throw new Error(`Unknown processor name '${p}'`);
        }
    }

    return con;
}

const commonNames: Record<string, string> = {
    list: 'NodeCollection',
    plain: 'PlainTextNode',
    html: 'HtmlNode',
    meta: 'MetadataNode',
    ref: 'RefNode',
    removed: 'RemovedNode',
};

function AssertMetadata(expected: JsonNode | null, actual: JsonNode | null): void {
    if (expected === null) {
        expect(actual).toBeNull();
        return;
    }

    expect(actual).not.toBeNull();
    expect(typeof expected).toBe(typeof actual);

    if (typeof expected == 'string') {
        expect(JSON.stringify(actual)).toBe(JSON.stringify(expected));
    } else if (typeof expected == 'object') {
        const eobj = expected;
        const robj = actual as Record<string, JsonNode | null>;

        expect(robj).not.toBeNull();
        expect(eobj).not.toBeNull();

        const rkeys = Object.entries(robj)
            .filter(([, value]) => value !== null && value != '0' && value !== '')
            .map(([key]) => key)
            .sort();
        const ekeys = Object.keys(eobj).sort();
        expect(rkeys).toEqual(ekeys);

        if (JSON.stringify(expected) === JSON.stringify(actual)) return;
        for (const [key, ev] of Object.entries(eobj)) {
            const rv = robj[key];
            expect(JSON.stringify(rv)).toBe(JSON.stringify(ev));
        }
    }
}

function AssertNodeHierarchy(expected: TestNode, node: INode, lineOffset: number): void {
    if (expected.pos[0] > 0) {
        expect(node.Line).toBe(expected.pos[0] + lineOffset);
        expect(node.Column).toBe(expected.pos[1]);
    }

    let expectedType = commonNames[expected.type] ?? expected.type;
    if (expectedType === 'UnprocessablePlainTextNode') expectedType = 'PlainTextNode';

    let actualType = node.constructor.name;
    if (actualType === 'UnprocessablePlainTextNode') actualType = 'PlainTextNode';

    expect(actualType).toBe(expectedType);

    switch (expectedType) {
        case 'PlainTextNode': {
            let text: string;
            if (node instanceof PlainTextNode || node instanceof UnprocessablePlainTextNode) text = node.Text;
            else throw new Error('Unexpected node type');
            expect(text).toBe(expected.text);
            break;
        }
        case 'HtmlNode': {
            const hn = node as HtmlNode;
            expect(hn).not.toBeNull();
            expect(expected.html).toHaveLength(2);
            expect(expected.plain).toHaveLength(2);
            expect(hn.HtmlBefore).toBe(expected.html![0]);
            expect(hn.HtmlAfter).toBe(expected.html![1]);
            expect(hn.PlainBefore).toBe(expected.plain![0]);
            expect(hn.PlainAfter).toBe(expected.plain![1]);
            break;
        }
        case 'MetadataNode': {
            const mn = node as MetadataNode;
            expect(mn).not.toBeNull();
            expect(mn.Key).toBe(expected.key);
            AssertMetadata(expected.value!, mn.Value as JsonNode | null);
            break;
        }
    }
    const expectedChildren = expected.children ?? [];
    const children = node.GetChildren().filter(x => !(x instanceof RemovedNode));
    expect(children).toHaveLength(expectedChildren.length ?? 0);

    for (let i = 0; i < expectedChildren.length; i++) {
        AssertNodeHierarchy(expectedChildren[i], children[i], lineOffset);
    }
}

function RunImplementationTest(name: string, lineOffset: number): void {
    const dir = GetTestCaseDirectory('implementations');
    const def = JSON.parse(readFileSync(`${dir}/${name}.json`, 'utf8')) as TestDefinition;

    let input = def.input;
    const config = CreateConfig(def.config);
    const parser = new Parser(config);
    let afterParse = (_: ParseResult): void => {};

    if (lineOffset > 0) {
        // for line offsets: add extra lines to the start of the input,
        // then add a custom element that eats those lines.
        // finally add a check to remove that element from the result.
        input = '\n'.repeat(lineOffset + 1) + input;
        config.Elements.unshift(new LineOffsetEaterElement(lineOffset));
        afterParse = RemoveLineOffsetElement;
    }

    const result = parser.ParseResult(input);
    afterParse(result);

    const resultHtml = result.ToHtml(sanitise);
    const resultPlain = result.ToPlainText();

    const expectedHtml = sanitise(def.html);
    const expectedPlain = def.plain;

    expect(resultHtml).toBe(expectedHtml);
    expect(resultPlain).toBe(expectedPlain);

    AssertNodeHierarchy(def.nodes, result.Content, lineOffset);
}

function RemoveLineOffsetElement(result: ParseResult): void {
    const content = result.Content;

    // remove the guid node
    if (!(content instanceof NodeCollection) || content.Nodes.length < 1) return;
    if (!(content.Nodes[0] instanceof UnprocessablePlainTextNode) || content.Nodes[0].Text !== RemovedContentGuid) return;
    content.Nodes.shift();

    // if there's more nodes, then the guid element node will have a blank line following it, remove that too
    if (content.Nodes.length < 1) return;
    if (!(content.Nodes[0] instanceof UnprocessablePlainTextNode) || content.Nodes[0].Text !== '\n') return;
    content.Nodes.shift();
}

const RemovedContentGuid: string = crypto.randomUUID();

class LineOffsetEaterElement extends Element {
    private readonly lineOffset: number;

    constructor(lineOffset: number) {
        super();
        this.lineOffset = lineOffset;
        this.Priority = Number.MAX_SAFE_INTEGER;
    }

    Matches(lines: Lines): boolean {
        return this.lineOffset > 0 && lines.SourceLine === 1;
    }

    Consume(parser: Parser, data: ParseData, lines: Lines, _scope: string): INode {
        for (let i = 0; i < this.lineOffset; i++) lines.Next();
        return new UnprocessablePlainTextNode(RemovedContentGuid);
    }
}

describe('Implementation tests', () => {
    test.each(GetTestCases())('Implementation: %p', RunImplementationTest);
});
