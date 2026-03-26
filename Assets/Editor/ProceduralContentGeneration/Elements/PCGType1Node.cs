using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PCG.Elements
{
    using Enumerations;
    using Utilities;

    public class PCGType1Node : PCGNode
    {
        
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            NodeType = PCGNodeType.Type1;

            Choices.Add("Out");
        }
        public override void Draw()
        {
            base.Draw();

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice, Orientation.Horizontal, Direction.Output, Port.Capacity.Single);
                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
