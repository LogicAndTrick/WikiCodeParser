<?php

namespace LogicAndTrick\WikiCodeParser\Models;

class WikiRevisionCredit
{
    public const TypeCredit = 'c';
    public const TypeArchive = 'a';
    public const TypeFull = 'f';

    public ?int $ID = null;
    public string $Type;
    public ?int $RevisionID = null;
    public ?string $Description = null;
    public ?int $UserID = null;
    public ?string $Name = null;
    public ?string $Url = null;
    public ?string $WaybackUrl = null;
}