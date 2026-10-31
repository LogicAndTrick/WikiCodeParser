# TWHL WikiCode Parser

This project is used on the TWHL website to parse its WikiCode format, which is some sort of abomination that combines BBCode with Markdown and a few other random ideas I had. It's written in 3 languages: C#, PHP, and TypeScript.

Usage in each one is the same, here's an example in each:

```cs
// C#
var wikiCode = "[b]Hello, world![/b]";

var config = new ParserConfiguration();
config.Tags.Add(new Tag("b", "strong").WithScopes("inline", "excerpt"));

var parser = new Parser(config);
var result = parser.ParseResult(wikiCode);
var html = result.ToHtml(SanitiseHtml); // more about this below
```

```js
// JavaScript / TypeScript
const wikiCode = '[b]Hello, world![/b]';

const config = new ParserConfiguration();
config.Tags.push(new Tag('b', 'strong').WithScopes('inline', 'excerpt'));

const parser = new Parser(config);
const result = parser.ParseResult(wikiCode);
const html = result.ToHtml(SanitiseHtml);
```

```php
// PHP
$wikiCode = '[b]Hello, world![/b]';

$config = new ParserConfiguration();
$config->tags[] = (new Tag('b', 'strong'))->WithScopes('inline', 'excerpt');

$parser = new Parser($config);
$result = $parser->ParseResult($wikiCode);
$html = $result->ToHtml(TestCaseUtils::Sanitise(...));
```

## Configuration

To configure, create an instance of `ParserConfiguration` and add elements, tags, and processors to it.
You can use the TWHL settings by using `ParserConfiguration.Twhl()` which will return a pre-configured
instance of the configuration. Take a look at its implementation for more details.

You can implement custom elements (block-level elements), tags (inline-level elements),
and processors (post processors that run on the parsed node tree). See the source code in your preferred
language for more details on how to implement any of those.

## HTML Sanitiser

I kept finding edge cases which would allow malformed WikiCode to create various bugs in the resulting HTML
output. Obviously, this is less than ideal if any of those cases lets people create unexpected HTML output
and post it on the website. Because of this, I have added a second safety net- the HTML sanitisation callback.
It is the responsibility of the caller to pass a valid sanitisation callback with the signature `string => string`.
You can skip this by passing a dummy callback, or by using `ToHtmlUnsanitised`, but of course this is not
recommended. This is the second layer of protection - I've tried as much as possible to remove bugs in the
parser where possible.

I have tested each implementation with the following sanitisation libraries:
- C#: `Ganss.Xss.HtmlSanitizer`, with this config:
    ```cs
    new HtmlSanitizer
    {
        AllowDataAttributes = true,
        AllowedTags = { "video", "audio" },
        AllowedAttributes = { "id", "class", "playsinline", "controls", "autoplay", "loop", "muted" },
        AllowedSchemes = { "ftp", "mailto" }
    };
    ```
- JavaScript: `dompurify`, with this config:
    ```js
    purify.setConfig({
        USE_PROFILES: {
            html: true,
        },
        FORCE_BODY: true,
    });
    ```
- PHP: `symfony/html-sanitizer`, with this config:
    ```php
    new HtmlSanitizer(
        new HtmlSanitizerConfig()
            ->allowSafeElements()
            ->allowLinkSchemes(['http', 'https', 'ftp', 'mailto'])
            ->allowAttribute('class', '*')
            ->allowAttribute('id', '*')
            ->allowAttribute('style', '*')
            ->allowAttribute('data-embed-type', ['div'])
            ->allowAttribute('data-vault-id', ['div'])
            ->allowAttribute('data-info', ['span'])
            ->allowAttribute('data-youtube-id', ['div'])
    );
    ```

# Unit Tests

The majority of the unit tests are in `tests/isolated`. Adding a new file to this folder will automatically add
that test case to every language's test framework. The structure of each file is:

```
[top part: the wikicode input]

###

[second part: the expected html output]

###

[third part: the expected plaintext output]
```