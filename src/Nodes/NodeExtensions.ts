import { INode } from './INode';
import { RemovedNode } from './RemovedNode';

export class NodeExtensions {
    public static Remove(root: INode, remove: INode): boolean {
        const children = root.GetChildren();
        const idx = children.indexOf(remove);
        if (idx >= 0) {
            root.ReplaceChild(idx, new RemovedNode(remove));
            return true;
        }
        for (const ch of children) {
            if (NodeExtensions.Remove(ch, remove)) return true;
        }
        return false;
    }

    public static Walk(node: INode, visitor: (node: INode) => boolean | undefined): boolean {
        if (visitor(node) === false) return false;
        for (const child of node.GetChildren()) {
            if (NodeExtensions.Walk(child, visitor) === false) return false;
        }
        return true;
    }

    public static WalkBack(node: INode, visitor: (node: INode) => boolean | undefined): boolean {
        for (const child of Array.from(node.GetChildren()).reverse()) {
            if (NodeExtensions.WalkBack(child, visitor) === false) return false;
        }
        if (visitor(node) === false) return false;
        return true;
    }
}
