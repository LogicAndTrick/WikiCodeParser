<?php

namespace LogicAndTrick\WikiCodeParser\Models;

class WikiRevisionCredit
{
    public const TypeCredit = 'c';
    public const TypeArchive = 'a';
    public const TypeFull = 'f';

    public ?int $id;
    public string $type;
    public ?int $revisionId;
    public ?string $description;
    public ?int $userId;
    public ?string $name;
    public ?string $url;
    public ?string $waybackUrl;
}