import { INode } from './INode';
import { PlainTextNode } from './PlainTextNode';
import { RemovedNode } from './RemovedNode';
import { UnprocessablePlainTextNode } from './UnprocessablePlainTextNode';

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

    public static PreviousSibling(node: INode, parentNode: INode | null): INode | null {
        if (parentNode == null) return null;
        const children = parentNode.GetChildren();
        const idx = children.indexOf(node);
        if (idx <= 0) return null;
        return children[idx - 1];
    }

    public static NextSibling(node: INode, parentNode: INode | null): INode | null {
        if (parentNode == null) return null;
        const children = parentNode.GetChildren();
        const idx = children.indexOf(node);
        if (idx < 0 || idx >= children.length - 1) return null;
        return children[idx + 1];
    }

    public static PreviousNodeEndsWith(node: INode, parentNode: INode | null, chars: Set<string>): boolean {
        let prev: INode | null = node;
        while ((prev = NodeExtensions.PreviousSibling(prev, parentNode)) != null) {
            if (prev instanceof PlainTextNode && prev.Text.length > 0) return chars.has(prev.Text[prev.Text.length - 1]);
            if (prev instanceof UnprocessablePlainTextNode && prev.Text.length > 0) return chars.has(prev.Text[prev.Text.length - 1]);
            if (prev.HasContent()) return false;
        }
        return true;
    }

    public static NextNodeStartsWith(node: INode, parentNode: INode | null, chars: Set<string>): boolean {
        let next: INode | null = node;
        while ((next = NodeExtensions.NextSibling(next, parentNode)) != null) {
            if (next instanceof PlainTextNode && next.Text.length > 0) return chars.has(next.Text[0]);
            if (next instanceof UnprocessablePlainTextNode && next.Text.length > 0) return chars.has(next.Text[0]);
            if (next.HasContent()) return false;
        }
        return true;
    }
}
