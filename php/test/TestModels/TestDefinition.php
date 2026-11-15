<?php

namespace LogicAndTrick\WikiCodeParser\TestModels;

class TestDefinition
{
    public TestConfig $config;
    public string $input;
    public string $html;
    public string $plain;
    public TestNode $nodes;
}
