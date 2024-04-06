using MischievousByte.Scaffolding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.ScaffoldingEditor
{
    public sealed partial class PersistentDataInspector : EditorWindow
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

            VisualElement root = rootVisualElement;

            Toolbar toolbar = new Toolbar();
            root.Add(toolbar);

            TwoPaneSplitView splitView = new TwoPaneSplitView(0, 150, TwoPaneSplitViewOrientation.Horizontal);

            KeyExplorer explorer = new KeyExplorer();
            VisualElement inspector = new VisualElement();

            Label label = new Label();
            inspector.Add(label);

            explorer.onSelectionChange += (e) =>
            {
                Debug.Log("Hello World!");
            };

            splitView.Add(explorer);
            splitView.Add(inspector);
            root.Add(splitView);
        }
    }
}
