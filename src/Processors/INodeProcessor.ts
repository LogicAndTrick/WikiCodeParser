import { INode } from '../Nodes/INode';
import { ParseData } from '../ParseData';
import { Parser } from '../Parser';

export interface INodeProcessor {
    /**
     * Higher priority processors are run first.
     */
    Priority : number;

    /**
     * Return true if the given node should be processed by this processor
     */
    ShouldProcess(node : INode, scope : string) : boolean;

    /**
     * Process a node and return the nodes that will replace it.
     * @returns The nodes to replace the given node with
     */
    Process(parser : Parser, data : ParseData, node : INode, scope : string) : INode[];
}
