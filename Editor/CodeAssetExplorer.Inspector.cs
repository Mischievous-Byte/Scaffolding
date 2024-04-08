using MischievousByte.Scaffolding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Unity.Properties.UI;
using UnityEngine.UIElements;
using Unity.Properties;

namespace MischievousByte.ScaffoldingEditor
{
    public partial class CodeAssetExplorer
    {
        private class Inspector : VisualElement
        {
            private readonly Label label;
            private readonly InspectorElement inspector;

            public SerializedProperty property;

            [Serializable]
            private struct TestData
            {
                public string text;
                public int number;
            }

            private interface IContainer
            {
                public object Value { get; }
            }
            private struct Container<T> : IContainer
            {
                public T value;
                public object Value => value;

                public Container(T value) => this.value = value;
            }
            public Inspector()
            {
                label = new Label();
                inspector = new();

               
                Add(label);
                Add(inspector);

                UpdateSelection(new string[] { });
            }

            private string key;
            public void UpdateSelection(IEnumerable<string> keys)
            {

                inspector.OnChanged -= OnEdit;
                int count = keys.Count();
                switch(count)
                {
                    case 0:
                        label.text = "Select an entry to start editing";
                        label.style.display = DisplayStyle.Flex;

                        inspector.SetEnabled(false);
                        inspector.style.display = DisplayStyle.None;

                        break;
                    case 1:
                        key = keys.First();
                        if(!CodeAsset.cache.ContainsKey(key))
                        {
                            label.text = "Entry doesn't contain data";
                            label.style.display = DisplayStyle.Flex;

                            inspector.SetEnabled(false);
                            inspector.style.display = DisplayStyle.None;
                        } else
                        {
                            label.style.display = DisplayStyle.None;

                            inspector.SetEnabled(true);
                            inspector.style.display = DisplayStyle.Flex;

                            SetTarget();

                            inspector.OnChanged += OnEdit;

                        }
                        
                        

                        key = keys.First();
                        break;
                    case 2:
                        label.text = "Can't edit multiple entries at once";
                        label.style.display = DisplayStyle.Flex;

                        inspector.SetEnabled(false);
                        inspector.style.display = DisplayStyle.None;
                        break;
                }

            }

            private void OnEdit(BindingContextElement element, PropertyPath path)
            {
                CodeAsset.IWrapper wrapper = CodeAsset.cache[key];

                wrapper.Raw = inspector.GetTarget<IContainer>().Value;
            }

            
            private void SetTarget()
            {
                CodeAsset.IWrapper wrapper = CodeAsset.cache[key];

                Type containerType = typeof(Container<>).MakeGenericType(wrapper.Raw.GetType());

                IContainer container = (IContainer) Activator.CreateInstance(containerType, wrapper.Raw);

                var methodinfo = typeof(Unity.Properties.UI.InspectorElement).GetMethod("SetTarget");

                var mi = methodinfo.MakeGenericMethod(containerType);

                mi.Invoke(inspector, new object[] { container });
            }
        }
    }
}
