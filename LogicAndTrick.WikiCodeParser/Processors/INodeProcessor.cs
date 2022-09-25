using System.Collections.Generic;
using LogicAndTrick.WikiCodeParser.Nodes;

namespace LogicAndTrick.WikiCodeParser.Processors
{
    public interface INodeProcessor
    {
        /// <summary>
        /// Higher priority processors are run first.
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// Return true if the given node should be processed by this processor
        /// </summary>
        bool ShouldProcess(INode node, string scope);

        /// <summary>
        /// Process a node and return the nodes that will replace it.
        /// </summary>
        /// <returns>The nodes to replace the given node with</returns>
        IEnumerable<INode> Process(Parser parser, ParseData data, INode node, string scope);
    }
}
