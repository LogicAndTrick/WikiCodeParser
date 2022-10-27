import browserify from 'browserify';
import { execSync } from 'child_process';
import exorcist from 'exorcist';
import { createWriteStream, mkdirSync, readFileSync, rmSync, writeFileSync } from 'fs';
import tsify from 'tsify';
import { minify } from 'uglify-js';

rmSync('./build', { recursive: true, force: true });
mkdirSync('./build');
mkdirSync('./build/browser');

execSync('tsc');

browserify({ debug: true })
    .add('src/index.browser.ts')
    .plugin(tsify, { noImplicitAny: true })
    .bundle()
    .on('error', function (error) { console.error(error.toString()); })
    .pipe(exorcist('./build/browser/twhl-wikicode-parser.js.map'))
    .pipe(createWriteStream('./build/browser/twhl-wikicode-parser.js', 'utf-8'))
    .on('finish', done);

function done() {
    var pickerJs = {
        'twhl-wikicode-parser.js': readFileSync('./build/browser/twhl-wikicode-parser.js', 'utf8')
    };
    
    var min = minify(pickerJs, {
        output: {
            ascii_only: true
        }
    });
    
    if (min.error) {
        console.log(min.error);
        return;
    }
    
    writeFileSync('./build/browser/twhl-wikicode-parser.min.js', min.code, 'utf8');    
}
