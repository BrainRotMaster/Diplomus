using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PCG.Windows
{
    using Elements;
    using Enumerations;
    using Utilities;
    using System.Collections.Generic;

    public class PCGGraphView : GraphView
    {
        private PCGEditorWindow editorWindow;
        private PCGSearchWindow searchWindow;
        public PCGGraphView(PCGEditorWindow pcgEditorWindow) 
        {
            editorWindow = pcgEditorWindow;
            AddManipulators();

            AddSearchWindow();

            AddGridBackground();

            AddStyles();
        }


        #region Overrided Methods
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort.node == port.node)
                {
                    return;
                }
                if (startPort.direction == port.direction)
                {
                    return;
                }
                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }
        #endregion

        #region Manipulators
        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale *2);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Add Node (Single Choice)", PCGNodeType.Type1));
            this.AddManipulator(CreateNodeContextualMenu("Add Node (Multiple Choice)", PCGNodeType.Type2));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, PCGNodeType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode(dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );
            return contextualMenuManipulator;
        }
        #endregion

        #region Element Creation
        public Group CreateGroup(string title, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = title
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            return group;
        }

        public PCGNode CreateNode(PCGNodeType dialogueType, Vector2 position)
        {
            Type nodeType = Type.GetType($"PCG.Elements.PCG{dialogueType}Node");

            PCGNode node = (PCGNode) Activator.CreateInstance(nodeType);
            node.Initialize(position);
            node.Draw();
            return node;
        }
        #endregion

        #region Element Addition
        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<PCGSearchWindow>();
                searchWindow.Initialize(this);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "ProceduralContentGeneration/PCGGraphViewStyles.uss",
                "ProceduralContentGeneration/PCGNodeStyles.uss"
                );
        }
        #endregion

        #region Utilities

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition -= editorWindow.position.position;
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);
            return localMousePosition;
        }

        #endregion
    }
}

