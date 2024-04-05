using Codice.CM.Common.Tree.Partial;
using JetBrains.Annotations;
using MischievousByte.Scaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.ScaffoldingEditor
{
    public sealed class PersistentDataInspector : EditorWindow
    {
        private struct KeyData
        {
            public string label;
            public bool data;
        }

        [MenuItem("Window/Persistent Data Inspector")]
        private static void OpenWindow()
        {
            var window = GetWindow<PersistentDataInspector>();
            window.Show();
        }

        private const int minWidth = 300;
        private const int minHeight = 200;

        private void CreateGUI()
        {
            minSize = new Vector2(minWidth, minHeight);

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            Toolbar toolbar = new Toolbar();
            root.Add(toolbar);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);


            splitView.Add(BuildKeyExplorer());
            splitView.Add(new VisualElement());
            root.Add(splitView);
        }


        
        private VisualElement BuildKeyExplorer()
        {
            VisualElement root = new VisualElement();
            root.style.minWidth = 100;

            TextField search = new TextField();
            root.Add(search);

            ScrollView scroll = new ScrollView();
            root.Add(scroll);

            TreeView tree = new TreeView();
            
            scroll.Add(tree);



            Func<VisualElement> makeItem = () =>
            {
                var container = new VisualElement();
                var image = new Image();
                var label = new Label();

                container.Add(image);
                container.Add(label);

                container.style.flexDirection = FlexDirection.Row;

                image.style.marginRight = 2;
                image.style.width = 16;
                image.style.minWidth = 16;
                image.style.height = 16;
                image.style.minHeight = 16;


                return container;
            };

            Action<VisualElement, int> bindItem = (VisualElement element, int index) =>
            {
                var data = tree.GetItemDataForIndex<KeyData>(index);

                Texture icon;
                if(data.data)
                    //icon = EditorGUIUtility.IconContent(tree.IsExpanded(index) ? "Cubemap Icon" : "PrefabModel Icon").image; 
                    icon = EditorGUIUtility.IconContent("ScriptableObject Icon").image; 
                    //icon = EditorGUIUtility.IconContent("LODGroup Icon").image; 
                else
                    icon = EditorGUIUtility.IconContent(tree.IsExpanded(index) ? "FolderOpened Icon" : "Folder Icon").image;


                element.Q<Image>().image = icon;

                
                
                element.Q<Label>().text = data.label;
            };

            tree.makeItem = makeItem;
            tree.bindItem = bindItem;
            tree.selectionType = SelectionType.Multiple;
            tree.fixedItemHeight = 16;
            

            tree.style.overflow = Overflow.Visible;

            UpdateTree(tree);

            return root;
        }

        

        private void UpdateTree(TreeView tree)
        {
            List<TreeViewItemData<KeyData>> items = new();

            int index = 0;
            TreeViewItemData<KeyData> GoDeeper(Node node)
            {
                int reservedIndex = index;
                index++;
                List<TreeViewItemData<KeyData>> l = new();

                foreach (Node n in node.children)
                    l.Add(GoDeeper(n));

                var r = new TreeViewItemData<KeyData>(reservedIndex, new() { label = node.label, data = node.data }, l.Count > 0 ? l : null);

                return r;
            }

            foreach (var n in GetKeyTree())
            {
                items.Add(GoDeeper(n));
            }

            tree.SetRootItems(items);
            tree.Rebuild();

        }

        private class Node
        {
            public string label;
            public bool data;
            public List<Node> children = new();
        }


        private Node[] GetKeyTree()
        {
            List<Node> roots = new List<Node>();

            Node root = new()
            {
                label = "root",
                children = new()
            };



            foreach (var path in PersistentData.cache.Keys.Select(key => key.Split(".")))
            {
                Node current = root;

                for (int i = 0; i < path.Length; i++)
                {
                    string part = path[i];
                    if (!current.children.Any(c => c.label == part))
                    {
                        current.children.Add(new Node() { label = part });
                    }

                    current = current.children.Where(c => c.label == part).First();

                    if (i == path.Length - 1)
                        current.data = true;
                }
            }

            return root.children.ToArray();
        }
    }
}
