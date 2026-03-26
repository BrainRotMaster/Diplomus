


using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PCG.Windows
{
    using Enumerations;
    using Elements;
    public class PCGSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private PCGGraphView graphView;
        private Texture2D indentationIcon; //для отступа в поиске
        public void Initialize(PCGGraphView pcgGraphView)
        {
            graphView = pcgGraphView;

            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("CreateElement")),
                new SearchTreeGroupEntry(new GUIContent("Node"), 1),
                new SearchTreeEntry(new GUIContent("Type1", indentationIcon))
                {
                    level = 2,
                    userData = PCGNodeType.Type1
                },
                new SearchTreeEntry(new GUIContent("Type2", indentationIcon))
                {
                    level = 2,
                    userData = PCGNodeType.Type2
                },
                new SearchTreeGroupEntry(new GUIContent("Group"), 1),
                new SearchTreeEntry(new GUIContent("Single Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                },
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            //Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);
            //switch (SearchTreeEntry.userData)
            //{
            //    case PCGNodeType.Type1:
            //        {
            //            PCGType1Node type1Node = (PCGType1Node)graphView.CreateNode(PCGNodeType.Type1, localMousePosition);
            //            graphView.AddElement(type1Node);
            //            return true;
            //        }
            //    case PCGNodeType.Type2:
            //        {
            //            PCGType2Node type2Node = (PCGType2Node)graphView.CreateNode(PCGNodeType.Type2, localMousePosition);
            //            graphView.AddElement(type2Node);
            //            return true;
            //        }
            //    case Group _:
            //        {
            //            Group group = graphView.CreateGroup("New Group", localMousePosition);
            //            graphView.AddElement(group);
            //            return true;
            //        }
            //    default:
            //        {
            //            return false;
            //        }
            //}
            return true;
        }
    }
}
