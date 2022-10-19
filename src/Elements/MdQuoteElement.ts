import { Parser } from '..';
import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Element } from './Element';

export class MdQuoteElement extends Element {
    public Matches(lines: Lines): boolean {
        throw new Error('Method not implemented.');
    }
    public Consume(parser: Parser, data: ParseData, lines: Lines, scope: string): INode {
        throw new Error('Method not implemented.');
    }
}