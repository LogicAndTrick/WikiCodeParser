<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

require_once __DIR__.'/TestCaseUtils.php';

class IsolatedTest extends TestCase
{
    public function testmissingtag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'missing-tag'); }
    public function testunicodeescape() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'unicode-escape'); }

    public function testrefsimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'ref-simple'); }

    public function testpresimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'pre-simple'); }
    public function testprelang() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'pre-lang'); }
    public function testprehighlight() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'pre-highlight'); }

    public function testmdcodesimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'mdcode-simple'); }
    public function testmdcodelang() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'mdcode-lang'); }

    public function testheadingsimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'heading-simple'); }

    public function testmdlinesimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'mdline-simple'); }

    public function testcolumnssimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'columns-simple'); }
    public function testcolumnsinvalid() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'columns-invalid'); }

    public function testpanelsimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'panel-simple'); }

    public function testmdquotesimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'mdquote-simple'); }
    public function testmdquotenested() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'mdquote-nested'); }

    public function testlistsimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'list-simple'); }
    public function testlistnested() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'list-nested'); }
    public function testlistcontinuation() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'list-continuation'); }

    public function testtablesimple() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'table-simple'); }
    public function testtableref() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'table-ref'); }

    public function testtagsplain() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'tags-plain'); }
    public function testcodetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'code-tag'); }
    public function testfonttag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'font-tag'); }
    public function testhtag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'h-tag'); }
    public function testpretag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'pre-tag'); }
    public function testquotetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'quote-tag'); }
    public function testquotewithheadings() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'quote-with-headings'); }
    public function testimagetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'image-tag'); }
    public function testlinktag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'link-tag'); }
    public function testvaulttag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'vault-tag'); }
    public function testquicklinktag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'quick-link-tag'); }
    public function testspoilertag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'spoiler-tag'); }
    public function testyoutubetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'youtube-tag'); }

    public function testwikicategorytag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-category-tag'); }
    public function testwikiimagetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-image-tag'); }
    public function testwikifiletag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-file-tag'); }
    public function testwikicredittag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-credit-tag'); }
    public function testwikibooktag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-book-tag'); }
    public function testwikiarchivetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-archive-tag'); }
    public function testwikiyoutubetag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-youtube-tag'); }
    public function testwikilinktag() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'wiki-link-tag'); }

    public function testprocessornewlines() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'processor-newlines'); }
    public function testprocessorsmiliesbasic() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'processor-smilies-basic'); }
    public function testprocessorsmiliestoomany() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'processor-smilies-toomany'); }
    public function testprocessormarkdowntext() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'processor-markdowntext'); }
    public function testprocessorautolinking() { TestCaseUtils::RunTestCase(ParserConfiguration::Twhl(), 'isolated', 'processor-autolinking'); }
}
