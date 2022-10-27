import { Lines } from '../Lines';
import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';

export abstract class Element {
    public Scopes : string[];
    public Priority = 0;

    public InScope(scope : string | null) : boolean {
        return scope == null || scope.trim() == '' || this.Scopes.includes(scope);
    }

    public abstract Matches(lines : Lines) : boolean;
    public abstract Consume(parser : Parser, data : ParseData, lines : Lines, scope : string) : INode;
}