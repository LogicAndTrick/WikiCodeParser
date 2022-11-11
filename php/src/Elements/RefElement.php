<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\Nodes\PlainTextNode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class RefElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        $value = trim($lines->Value());
        return strlen($value) > 4 && str_starts_with($value, '[ref=') && preg_match('/\[ref=[a-z0-9 ]+\]/i', $value);
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();
        $arr = [];

        $line = trim($lines->Value());
        $success = preg_match('/\[ref=([a-z0-9 ]+)\]/i', $line, $res);
        if (!$success) {
            $lines->SetCurrent($current);
            return null;
        }

        $line = substr($line, strlen($res[0]));

        $name = $res[1];

        if (str_ends_with($line, '[/ref]')) {
            $arr[] = substr($line, 0, strlen($line) - 6);
        } else {
            if (strlen($line) > 0) $arr[] = $line;
            $found = false;
            while ($lines->Next()) {
                $value = rtrim($lines->Value());
                if (str_ends_with($value, '[/ref]')) {
                    $lastLine = substr($value, 0, strlen($value) - 6);
                    $arr[] = $lastLine;
                    $found = true;
                    break;
                } else {
                    $arr[] = $value;
                }
            }

            if (!$found || count($arr) == 0) {
                $lines->SetCurrent($current);
                return null;
            }
        }

        // Store the ref node
        $node = $parser->ParseElements($data, trim(implode("\n", $arr)), $scope);
        $data->Set("Ref::$name", $node);

        // Return nothing
        return PlainTextNode::Empty();
    }
}