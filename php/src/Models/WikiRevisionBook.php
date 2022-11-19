<?php

namespace LogicAndTrick\WikiCodeParser\Models;

class WikiRevisionBook
{
    public ?int $id;
    public ?int $revisionID;
    public ?string $bookName;
    public ?string $chapterName;
    public ?int $chapterNumber;
    public ?int $pageNumber;
}