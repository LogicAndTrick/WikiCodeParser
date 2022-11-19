<?php

namespace LogicAndTrick\WikiCodeParser\Models;

class WikiRevisionBook
{
    public ?int $ID;
    public ?int $RevisionID;
    public ?string $BookName;
    public ?string $ChapterName;
    public ?int $ChapterNumber;
    public ?int $PageNumber;
}