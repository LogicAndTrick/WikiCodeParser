import terser from '@rollup/plugin-terser';
import typescript from '@rollup/plugin-typescript';

export default {
    input: 'src/index.ts',
    output: [
        {
            file: 'build/browser/twhl-wikicode-parser.js',
            sourcemap: true,
            format: 'iife',
            name: 'TwhlWikiCodeParser'
        },
        {
            file: 'build/browser/twhl-wikicode-parser.min.js',
            sourcemap: true,
            format: 'iife',
            name: 'TwhlWikiCodeParser',
            plugins: [terser()]
        },
        {
            file: 'build/index.js',
            sourcemap: false,
            format: 'cjs'
        }
    ],
    plugins: [
        typescript({
            tsconfig: 'tsconfig.json',
            declaration: false,
        })
    ]
};