<?php

namespace LogicAndTrick\WikiCodeParser\Tags;

use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;
use LogicAndTrick\WikiCodeParser\State;
use LogicAndTrick\WikiCodeParser\TagParseContext;

class Tag
{
    public ?string $token = null;
    public ?string $element = null;
    public ?string $elementClass = null;
    public ?string $mainOption = null;
    /**
     * @var string[]
     */
    public array $options = [];
    public bool $allOptionsInMain = false;
    public bool $isBlock = false;
    public bool $isNested = false;

    /**
     * @var string[]
     */
    public array $scopes;
    public int $priority = 0;

    protected function TagContext(): TagParseContext
    {
        return $this->isBlock ? TagParseContext::Block : TagParseContext::Inline;
    }

    public function __construct(string $token = '', string $element = '', ?string $elementClass = null)
    {
        $this->token = $token;
        $this->element = $element;
        $this->elementClass = $elementClass;
        $this->options = [];
        $this->scopes = [];
    }

    public function InScope(string $scope): bool
    {
        return !$scope || trim($scope) == '' || in_array($scope, $this->scopes, true);
    }

    public function Matches(State $state, ?string $token, TagParseContext $context): bool
    {
        return strtolower($token) == $this->token && ($context == TagParseContext::Block || !$this->isBlock);
    }

    public function Parse(Parser $parser, ParseData $data, State $state, string $scope, TagParseContext $context): INode|null
    {
        $index = $state->index;
        $tokenLength = strlen($this->token);

        $state->Seek($tokenLength + 1, false);
        $optionsString = trim($state->ScanTo(']'));
        if ($state->Next() != ']') {
            $state->Seek($index, true);
            return null;
        }

        $options = [];
        if (strlen($optionsString) > 0) {
            if ($optionsString[0] == '=' && $this->allOptionsInMain && $this->mainOption != null) {
                $options[$this->mainOption] = substr($optionsString, 1);
            } else {
                if ($optionsString[0] == '=') $optionsString = $this->mainOption . $optionsString;
                preg_match_all('/(?=\s|^)\s*([^ ]+?)=([^\s]*)(?=\s|$)(?!=)/im', $optionsString, $matches, PREG_SET_ORDER);
                for ($i = 0; $i < count($matches); $i++) {
                    $match = $matches[$i];
                    $name = trim($match[1]);
                    $value = trim($match[2]);
                    $options[$name] = $value;
                }
            }
        }

        if ($this->isNested) {
            $stack = 1;
            $text = '';
            while (!$state->Done()) {
                $text .= $state->ScanTo('[');
                $tok = $state->GetToken();
                if (strtolower($tok) == strtolower($this->token)) $stack++;
                if (strtolower($tok) == '/' . strtolower($this->token) && trim($state->Peek($tokenLength + 3)) == '[/' . strtolower($this->token) . ']') $stack--;
                if ($stack == 0) {
                    $state->Seek(strlen($this->token) + 3, false);
                    if (!$this->Validate($options, $text)) break;
                    return $this->FormatResult($parser, $data, $state, $scope, $options, $text);
                }

                $text .= $state->Next();
            }

            $state->Seek($index, true);
            return null;
        } else {
            $text = $state->ScanTo('[/' . $this->token . ']', true);
            if (trim($state->Peek($tokenLength + 3)) == '[/' . strtolower($this->token) . ']' && $this->Validate($options, $text)) {
                $state->Seek(strlen($this->token) + 3, false);
                return $this->FormatResult($parser, $data, $state, $scope, $options, $text);
            } else {
                $state->Seek($index, true);
                return null;
            }
        }
    }

    public function Validate(array $options, string $text): bool
    {
        return true;
    }

    public function FormatResult(Parser $parser, ParseData $data, State $state, string $scope, array $options, string $text): INode
    {
        $before = '<' . $this->element;
        if ($this->elementClass) $before .= ' class="' . $this->elementClass . '"';
        $before .= '>';
        $after = '</' . $this->element . '>';
        $content = $parser->ParseTags($data, $text, $scope, $this->TagContext());
        $ret = new HtmlNode($before, $content, $after);
        $ret->isBlockNode = $this->isBlock;
        return $ret;
    }

    // Extensions

    /**
     * @param string ...$scopes
     */
    public function WithScopes(string ...$scopes): Tag
    {
        $this->scopes = $scopes;
        return $this;
    }

    public function WithToken(string $token): Tag
    {
        $this->token = $token;
        return $this;
    }

    public function WithElement(string $element): Tag
    {
        $this->element = $element;
        return $this;
    }

    public function WithElementClass(string $elementClass): Tag
    {
        $this->elementClass = $elementClass;
        return $this;
    }

    public function WithBlock(bool $isBlock): Tag
    {
        $this->isBlock = $isBlock;
        return $this;
    }
}