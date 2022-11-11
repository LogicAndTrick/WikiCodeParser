<?php

namespace LogicAndTrick\WikiCodeParser;

enum TagParseContext
{
    case Block;
    case Inline;
}