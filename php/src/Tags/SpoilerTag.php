<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

class SpoilerTag extends Tag
{

    public function __construct()
    {
        parent::__construct();
        $this->token = null;
        $this->element = 'a';

    }
}