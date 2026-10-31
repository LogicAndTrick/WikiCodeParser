import { expect } from '@jest/globals';
import DOMPurify, { RemovedAttribute, RemovedElement } from 'dompurify';
import { JSDOM } from 'jsdom';

const jsdom = new JSDOM('');
const window = jsdom.window;
const purify = DOMPurify(window);

purify.setConfig({
    USE_PROFILES: {
        html: true,
    },
    FORCE_BODY: true,
});

export function sanitise(html: string): string {
    if (!html || html.trim() == '') return '';
    purify.removed.splice(0, purify.removed.length);
    const ret = purify.sanitize(html);
    const removed = purify.removed
        .map<Partial<RemovedElement & RemovedAttribute>>(r => r)
        .filter(r => !r.element || r.element.nodeName != 'REMOVE') // FORCE_BODY adds a fake tag called <remove>
        .map(r => (r.element ? `removed element <${r.element.nodeName}>` : `removed attribute '${r.attribute?.name}' from <${r.from?.nodeName}>`));
    expect(removed).toEqual([]);
    return ret;
}
