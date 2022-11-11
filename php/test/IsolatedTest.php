<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

require_once __DIR__.'./TestCaseUtils.php';

class IsolatedTest extends TestCase
{
    public function testmissingtag() { TestCaseUtils::RunTestCase('isolated', 'missing-tag'); }
    public function testunicodeescape() { TestCaseUtils::RunTestCase('isolated', 'unicode-escape'); }

    public function testrefsimple() { TestCaseUtils::RunTestCase('isolated', 'ref-simple'); }

    public function testpresimple() { TestCaseUtils::RunTestCase('isolated', 'pre-simple'); }
    public function testprelang() { TestCaseUtils::RunTestCase('isolated', 'pre-lang'); }
    public function testprehighlight() { TestCaseUtils::RunTestCase('isolated', 'pre-highlight'); }

    public function testmdcodesimple() { TestCaseUtils::RunTestCase('isolated', 'mdcode-simple'); }
    public function testmdcodelang() { TestCaseUtils::RunTestCase('isolated', 'mdcode-lang'); }

    public function testheadingsimple() { TestCaseUtils::RunTestCase('isolated', 'heading-simple'); }

    public function testmdlinesimple() { TestCaseUtils::RunTestCase('isolated', 'mdline-simple'); }

    public function testcolumnssimple() { TestCaseUtils::RunTestCase('isolated', 'columns-simple'); }
    public function testcolumnsinvalid() { TestCaseUtils::RunTestCase('isolated', 'columns-invalid'); }

    public function testpanelsimple() { TestCaseUtils::RunTestCase('isolated', 'panel-simple'); }

    public function testmdquotesimple() { TestCaseUtils::RunTestCase('isolated', 'mdquote-simple'); }
    public function testmdquotenested() { TestCaseUtils::RunTestCase('isolated', 'mdquote-nested'); }

    public function testlistsimple() { TestCaseUtils::RunTestCase('isolated', 'list-simple'); }
    public function testlistnested() { TestCaseUtils::RunTestCase('isolated', 'list-nested'); }
    public function testlistcontinuation() { TestCaseUtils::RunTestCase('isolated', 'list-continuation'); }

    public function testtablesimple() { TestCaseUtils::RunTestCase('isolated', 'table-simple'); }
    public function testtableref() { TestCaseUtils::RunTestCase('isolated', 'table-ref'); }

    public function testtagsplain() { TestCaseUtils::RunTestCase('isolated', 'tags-plain'); }
    public function testcodetag() { TestCaseUtils::RunTestCase('isolated', 'code-tag'); }
    public function testfonttag() { TestCaseUtils::RunTestCase('isolated', 'font-tag'); }
    public function testhtag() { TestCaseUtils::RunTestCase('isolated', 'h-tag'); }
    public function testpretag() { TestCaseUtils::RunTestCase('isolated', 'pre-tag'); }
    public function testquotetag() { TestCaseUtils::RunTestCase('isolated', 'quote-tag'); }
    public function testimagetag() { TestCaseUtils::RunTestCase('isolated', 'image-tag'); }
    public function testlinktag() { TestCaseUtils::RunTestCase('isolated', 'link-tag'); }
    public function testvaulttag() { TestCaseUtils::RunTestCase('isolated', 'vault-tag'); }
    public function testquicklinktag() { TestCaseUtils::RunTestCase('isolated', 'quick-link-tag'); }
    public function testspoilertag() { TestCaseUtils::RunTestCase('isolated', 'spoiler-tag'); }
    public function testyoutubetag() { TestCaseUtils::RunTestCase('isolated', 'youtube-tag'); }

    public function testwikicategorytag() { TestCaseUtils::RunTestCase('isolated', 'wiki-category-tag'); }
    public function testwikiimagetag() { TestCaseUtils::RunTestCase('isolated', 'wiki-image-tag'); }
    public function testwikifiletag() { TestCaseUtils::RunTestCase('isolated', 'wiki-file-tag'); }
    public function testwikicredittag() { TestCaseUtils::RunTestCase('isolated', 'wiki-credit-tag'); }
    public function testwikibooktag() { TestCaseUtils::RunTestCase('isolated', 'wiki-book-tag'); }
    public function testwikiarchivetag() { TestCaseUtils::RunTestCase('isolated', 'wiki-archive-tag'); }
    public function testwikiyoutubetag() { TestCaseUtils::RunTestCase('isolated', 'wiki-youtube-tag'); }
    public function testwikilinktag() { TestCaseUtils::RunTestCase('isolated', 'wiki-link-tag'); }

    public function testprocessornewlines() { TestCaseUtils::RunTestCase('isolated', 'processor-newlines'); }
    public function testprocessorsmiliesbasic() { TestCaseUtils::RunTestCase('isolated', 'processor-smilies-basic'); }
    public function testprocessorsmiliestoomany() { TestCaseUtils::RunTestCase('isolated', 'processor-smilies-toomany'); }
    public function testprocessormarkdowntext() { TestCaseUtils::RunTestCase('isolated', 'processor-markdowntext'); }
    public function testprocessorautolinking() { TestCaseUtils::RunTestCase('isolated', 'processor-autolinking'); }
}
