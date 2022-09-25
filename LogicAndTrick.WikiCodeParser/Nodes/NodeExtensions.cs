using System;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public static class NodeExtensions
    {
        /// <summary>
        /// Walk the tree of nodes depth-first and run the given action on each one.
        /// </summary>
        /// <param name="node">Root node</param>
        /// <param name="visitor">The visitor action to run</param>
        public static void Walk(this INode node, Action<INode> visitor)
        {
            visitor.Invoke(node);
            foreach (var child in node.GetChildren()) Walk(child, visitor);
        }
    }
}