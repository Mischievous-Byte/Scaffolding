using Codice.CM.WorkspaceServer.DataStore.WkTree;
using MischievousByte.Scaffolding;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.ScaffoldingEditor
{
    public sealed partial class CodeAssetExplorer
    {
        internal class KeyExplorer : VisualElement
        {
            public enum SelectionType
            {
                None,
                Valid,
                Multiple
            }

            public event Action<IEnumerable<string>> onSelectionChange;

            private readonly TextField searchBar;
            private readonly TreeView tree;

            public KeyExplorer()
            {
                searchBar = new TextField();
                tree = new TreeView();
                
                SetupTree();


                Add(searchBar);
                Add(tree);

                searchBar.RegisterCallback<ChangeEvent<string>>((e) => UpdateTree(e.newValue));
                tree.selectionChanged += (s) => onSelectionChange?.Invoke(s.Cast<ItemData>().Select(i => i.key));

                UpdateTree("");
            }

            private void SetupTree()
            {
                tree.makeItem = () =>
                {
                    VisualElement container = new();
                    Image icon = new();
                    Label label = new();

                    container.Add(icon);
                    container.Add(label);

                    container.style.flexDirection = FlexDirection.Row;

                    icon.style.marginRight = 2;
                    icon.style.width = 16;
                    icon.style.minWidth = 16;
                    icon.style.height = 16;
                    icon.style.minHeight = 16;

                    return container;
                };

                tree.bindItem = (element, i) =>
                {
                    var itemData = tree.GetItemDataForIndex<ItemData>(i);
                    Texture icon = EditorGUIUtility.IconContent(itemData.data ? "ScriptableObject Icon" : 
                        tree.IsExpanded(i) ? "FolderOpened Icon" : "Folder Icon").image;

                    element.tooltip = itemData.key;

                    element.Q<Image>().image = icon;
                    element.Q<Label>().text = itemData.label;
                };

                tree.selectionType = UnityEngine.UIElements.SelectionType.Multiple;
                tree.fixedItemHeight = 16;
            }

            private void UpdateTree(string filter)
            {
                var roots = GetKeyTree(CodeAsset.cache.Keys.Where(k => k.Contains(filter)));

                tree.SetRootItems(GetTreeItems(roots));
                tree.Rebuild();
            }

            public struct KeyNode
            {
                public string label;
                public bool data;
                public IEnumerable<KeyNode> children;
            }

            private struct ItemData
            {
                public string label;
                public bool data;
                public string key;
            }
            private List<TreeViewItemData<ItemData>> GetTreeItems(IEnumerable<KeyNode> nodes)
            {
                int index = 0;

                TreeViewItemData<ItemData> FromNode(KeyNode node, string parentKey)
                {
                    int i = index++;

                    string key;
                    if (parentKey != string.Empty)
                        key = $"{parentKey}.{node.label}";
                    else
                        key = node.label;

                    List<TreeViewItemData<ItemData>> children = new();

                    foreach (var n in node.children)
                        children.Add(FromNode(n, key));

                    return new(i, new ItemData() { label = node.label, data = node.data, key = key}, children);
                }

                List<TreeViewItemData<ItemData>> items = new();

                foreach (var node in nodes)
                    items.Add(FromNode(node, ""));

                return items;
            }


            private IEnumerable<KeyNode> GetKeyTree(IEnumerable<string> keys)
            {
                var groups = keys
                .Select(k => k.Split("."))
                .GroupBy(parts => parts.First());

                foreach (var group in groups)
                {
                    var subPaths = group.Where(s => s.Length > 1).Select(s => string.Join(".", s.Skip(1)));

                    yield return new()
                    {
                        label = group.Key,
                        data = keys.Contains(group.Key),
                        children = GetKeyTree(subPaths)
                    };
                }
            }

        }
    }
}
