<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

abstract class Element
{
    /**
     * @var string[]
     */
    public array $scopes = [];

    public int $priority = 0;

    public function InScope(string $scope) : bool {
        return !$scope || trim($scope) == '' || in_array($scope, $this->scopes, true);
    }

    public abstract function Matches(Lines $lines) : bool;
    public abstract function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope) : ?INode;
}