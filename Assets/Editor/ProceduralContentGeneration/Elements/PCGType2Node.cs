using UnityEngine;
using UnityEngine.UIElements;


namespace PCG.Elements
{
    using Enumerations;
    using Utilities;
    using UnityEditor.Experimental.GraphView;

    public class PCGType2Node : PCGNode
    {

        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);

            NodeType = PCGNodeType.Type2;

            Choices.Add("New Choice");
        }
        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = PCGElementUtility.CreateButton("Add", () =>
            {
                Port choicePort = CreateChoicePort("New Choice");
                Choices.Add("New Choice");
                outputContainer.Add(choicePort);
            });

            addChoiceButton.AddClasses("ds-node__button");

            mainContainer.Insert(1, addChoiceButton);

            foreach (var choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }

        #region Elements Creation
        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();
            Button deleteChoiceButton = PCGElementUtility.CreateButton("X");
            deleteChoiceButton.AddClasses("ds-node__button");

            TextField choiseTextField = PCGElementUtility.CreateTextField(choice);

            choiseTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden"
                );

            choicePort.Add(choiseTextField);
            choicePort.Add(deleteChoiceButton);
            return choicePort;
        }

        #endregion
    }
}
