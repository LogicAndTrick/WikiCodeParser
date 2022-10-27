using System;
using System.Linq;

namespace LogicAndTrick.WikiCodeParser.Nodes
{
    public static class NodeExtensions
    {
        /// <summary>
        /// Remove a child node from underneath the given root. The root cannot be removed using this method.
        /// </summary>
        /// <param name="root">The root node for the search</param>
        /// <param name="remove">The node to remove</param>
        /// <returns>True if the node was found and removed</returns>
        public static bool Remove(this INode root, INode remove)
        {
            var children = root.GetChildren();
            var idx = children.IndexOf(remove);

            if (idx >= 0)
            {
                root.ReplaceChild(idx, new RemovedNode(remove));
                return true;
            }

            foreach (var ch in children)
            {
                if (Remove(ch, remove)) return true;
            }

            return false;
        }

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

        /// <summary>
        /// Walk the tree of nodes depth-first and run the given action on each one.
        /// </summary>
        /// <param name="node">Root node</param>
        /// <param name="visitor">The visitor action to run, return false to stop walking</param>
        public static bool Walk(this INode node, Predicate<INode> visitor)
        {
            if (!visitor.Invoke(node)) return false;

            foreach (var child in node.GetChildren())
            {
                if (!Walk(child, visitor)) return false;
            }

            return true;
        }

        /// <summary>
        /// Walk the tree of nodes in reverse depth-first and run the given action on each one.
        /// </summary>
        /// <param name="node">Root node</param>
        /// <param name="visitor">The visitor action to run, return false to stop walking</param>
        public static bool WalkBack(this INode node, Predicate<INode> visitor)
        {
            foreach (var child in node.GetChildren().Reverse())
            {
                if (!WalkBack(child, visitor)) return false;
            }

            if (!visitor.Invoke(node)) return false;

            return true;
        }
    }
}