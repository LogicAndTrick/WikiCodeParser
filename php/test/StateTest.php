<?php

namespace LogicAndTrick\WikiCodeParser;

use PHPUnit\Framework\TestCase;

class StateTest extends TestCase
{
    public function testScanTo()
    {
        $st = new State('A B C');
        self::assertEquals('A ', $st->ScanTo('B'));
        self::assertEquals('B ', $st->ScanTo('C'));
        self::assertEquals('C', $st->ScanTo('D'));
    }

    public function testSkipWhitespace()
    {
        $st = new State("A B C");
        $st->SkipWhitespace();
        self::assertEquals(0, $st->index);
        $st->Seek(1, false);
        $st->SkipWhitespace();
        self::assertEquals(2, $st->index);
        $st->Seek(1, false);
        $st->SkipWhitespace();
        self::assertEquals(4, $st->index);
    }

    public function testPeekTo()
    {
        $st = new State("A B C");
        self::assertEquals("A ", $st->PeekTo("B"));
        self::assertEquals("A B ", $st->PeekTo("C"));
        self::assertEquals(null, $st->PeekTo("D"));
        $st->Seek(2, false);
        self::assertEquals("", $st->PeekTo("B"));
        self::assertEquals("B ", $st->PeekTo("C"));
        self::assertEquals(null, $st->PeekTo("D"));

        $st = new State("Hello, [[AAAAA]] [[BB]]");
        self::assertEquals("Hello, ", $st->ScanTo('[['));
        $st->Seek(2, false);
        self::assertEquals("AAAAA", $st->ScanTo(']]'));
        $st->Seek(3, false);
        self::assertEquals("[[BB", $st->PeekTo(']]'));
    }

    public function testSeek()
    {
        $st = new State("A B C");
        self::assertEquals(0, $st->index);
        $st->Seek(1, false);
        self::assertEquals(1, $st->index);
        $st->Seek(1, false);
        self::assertEquals(2, $st->index);
        $st->Seek(3, false);
        self::assertEquals(5, $st->index);
        $st->Seek(3, true);
        self::assertEquals(3, $st->index);
    }

    public function testPeek()
    {
        $st = new State("A B C");
        self::assertEquals("A ", $st->Peek(2));
        self::assertEquals("A B ", $st->Peek(4));
        self::assertEquals("A B C", $st->Peek(6));
        $st->Seek(2, false);
        self::assertEquals("B ", $st->Peek(2));
        self::assertEquals("B C", $st->Peek(4));
        self::assertEquals("B C", $st->Peek(6));
    }

    public function testNext()
    {
        $st = new State("A B C");
        self::assertEquals('A', $st->Next());
        self::assertEquals(' ', $st->Next());
        self::assertEquals('B', $st->Next());
        self::assertEquals(' ', $st->Next());
        self::assertEquals('C', $st->Next());
        self::assertEquals('\0', $st->Next());
        self::assertEquals('\0', $st->Next());
    }

    public function testGetToken()
    {
        self::assertEquals(null, (new State("none"))->GetToken());
        self::assertEquals("some", (new State("[some]"))->GetToken());
        self::assertEquals("some", (new State("[some=thing]"))->GetToken());
        self::assertEquals("some", (new State("[some thing]"))->GetToken());
        $st = new State("a [some] b");
        $st->ScanTo("[");
        self::assertEquals(2, $st->index);
        self::assertEquals("some", $st->GetToken());
        self::assertEquals(2, $st->index);
    }
}
