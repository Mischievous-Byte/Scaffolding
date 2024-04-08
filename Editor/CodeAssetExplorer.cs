using MischievousByte.Scaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.ScaffoldingEditor
{
    public sealed partial class CodeAssetExplorer : EditorWindow
    {
        private struct KeyData
        {
            public string label;
            public bool data;
        }

        [MenuItem("Window/Code Asset Explorer")]
        private static void OpenWindow()
        {
            var window = GetWindow<CodeAssetExplorer>();
           
            window.Show();
        }


        private const int minWidth = 300;
        private const int minHeight = 200;

        private void CreateGUI()
        {
            titleContent = new GUIContent("Code Asset Explorer");
            titleContent.image = EditorGUIUtility.IconContent("CustomTool").image;

            minSize = new Vector2(minWidth, minHeight);

            VisualElement root = rootVisualElement;

            Toolbar toolbar = new Toolbar();
            root.Add(toolbar);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);

            KeyExplorer explorer = new KeyExplorer();
            Inspector inspector = new Inspector();

            inspector.property = new SerializedObject(this).FindProperty("editableData");
            explorer.onSelectionChange += inspector.UpdateSelection;

            splitView.Add(explorer);
            splitView.Add(inspector);
            root.Add(splitView);
        }

    }
}
