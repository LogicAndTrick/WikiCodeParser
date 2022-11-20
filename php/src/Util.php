<?php

namespace LogicAndTrick\WikiCodeParser;

class Util {
    public static function Find(array $array, callable $predicate) : mixed {
        foreach ($array as $el) {
            if ($predicate($el)) return $el;
        }
        return null;
    }

    public static function OrderBy(array $array, callable $selector) : array {
        usort($array, function ($a, $b) use ($selector) {
            $av = call_user_func($selector, $a);
            $bv = call_user_func($selector, $b);
            if ($av < $bv) return -1;
            if ($av > $bv) return 1;
            return 0;
        });
        return $array;
    }

    public static function OrderByDescending(array $array, callable $selector) : array {
        $array = self::OrderBy($array, $selector);
        return array_reverse($array);
    }

    public static function IndexOfAny(string $str, array $searchStrings, int $position = 0) {
        $min = -1;
        foreach ($searchStrings as $searchString) {
            $idx = strpos($str, $searchString, $position);
            if ($idx >= 0) $min = $min < 0 ? $idx : min($min, $idx);
        }
        return $min;
    }

    public static function Template(string $templateString, mixed $obj) {
        return preg_replace_callback('/\{(.*?)\}/ig', function (array $matches) use ($obj) {
            return $obj[$matches[1]];
        }, $templateString);
    }
}