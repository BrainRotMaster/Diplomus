using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace PCG.Elements
{
    using Enumerations;
    using Utilities;

    public class PCGNode : Node
    {
        public string NodeName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public PCGNodeType NodeType { get; set; }

        public virtual void Initialize(Vector2 position)
        {
            NodeName = "NodeName";
            Choices = new List<string>();
            Text = "text";

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            /*NODE NAME*/
            TextField dialogueNameTextField = PCGElementUtility.CreateTextField(NodeName);

            dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__filename-textfield",
                "ds-node__textfield__hidden"
                );

            titleContainer.Insert(0, dialogueNameTextField);

            /*INPUT PORT*/
            Port inputPort = this.CreatePort("in", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            /*EXTENSION*/
            VisualElement customDataContainer = new VisualElement();
            customDataContainer.AddClasses("ds-node__custom-data-container");

            Foldout textFoldout = PCGElementUtility.CreateFoldout("Extension info");
            TextField textTextField = PCGElementUtility.CreateTextArea(Text);

            textTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__quote-textfield"
                );            

            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);


        }

    }

}