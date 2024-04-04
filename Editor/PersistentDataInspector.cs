using Codice.CM.Common.Tree.Partial;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MischievousByte.ScaffoldingEditor
{
    public sealed class PersistentDataInspector : EditorWindow
    {
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


            VisualElement keys = BuildKeyPanel();
            keys.style.minWidth = 100;

            splitView.Add(keys);
            splitView.Add(new VisualElement());
            root.Add(splitView);

            
        }


        private VisualElement BuildKeyPanel()
        {
            VisualElement root = new VisualElement();
            root.style.minWidth = 100;

            TextField search = new TextField();
            root.Add(search);

            ScrollView list = new ScrollView();
            root.Add(list);
            list.style.width = new StyleLength(StyleKeyword.Auto);
            list.style.minHeight = minHeight;

            Box box = new Box();
            box.Add(new Label() { text = "Hello World! Hello World! Hello World! Hello World! Hello World!" });
            list.Add(box);
            box.style.borderBottomWidth = 1;
            box.style.borderBottomColor = Color.white;
            box.style.paddingTop = 5;
            box.style.paddingBottom = 5;

            
            return root;
        }
    }
}
