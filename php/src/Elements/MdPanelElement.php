<?php

namespace LogicAndTrick\WikiCodeParser\Elements;

use LogicAndTrick\WikiCodeParser\HtmlHelper;
use LogicAndTrick\WikiCodeParser\Lines;
use LogicAndTrick\WikiCodeParser\Nodes\HtmlNode;
use LogicAndTrick\WikiCodeParser\Nodes\INode;
use LogicAndTrick\WikiCodeParser\ParseData;
use LogicAndTrick\WikiCodeParser\Parser;

class MdPanelElement extends Element
{
    public function Matches(Lines $lines): bool
    {
        return str_starts_with($lines->Value(), '~~~');
    }

    public function Consume(Parser $parser, ParseData $data, Lines $lines, string $scope): ?INode
    {
        $current = $lines->Current();

        $meta = trim(substr($lines->Value(), 3));
        $title = '';

        $found = false;
        $arr = [];
        while ($lines->Next()) {
            $value = rtrim($lines->Value());
            if ($value == '~~~') {
                $found = true;
                break;
            }

            if (strlen($value) > 1 && $value[0] == ':') $title = trim(substr($value, 1));
            else $arr[] = $value;
        }

        if (!$found) {
            $lines->SetCurrent($current);
            return null;
        }

        if ($meta == 'message') $cls = 'card-success';
        else if ($meta == 'info') $cls = 'card-info';
        else if ($meta == 'warning') $cls = 'card-warning';
        else if ($meta == 'error') $cls = 'card-danger';
        else $cls = 'card-default';

        $encTitle = HtmlHelper::Encode($title);
        $before = "<div class=\"embed-panel card $cls\">" .
            ($title != '' ? "<div class=\"card-header\">{$encTitle}</div>" : '') .
            '<div class="card-body">';
        $content = $parser->ParseElements($data, implode("\n", $arr), $scope);
        $after = '</div></div>';

        $node = new HtmlNode($before, $content, $after);
        $node->plainBefore = $title == '' ? '' : $title . "\n" . str_repeat('-', strlen($title)) . "\n";
        return $node;
    }
}