using System;
using FullInspector.Internal;
using FullSerializer;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules {
    [CustomPropertyEditor(typeof(Type))]
    public class TypePropertyEditor : PropertyEditor<Type> {

        public class TypeLookupOptions : IGraphMetadataItemNotPersistent{
            // To which super Type we need to restrict options
            public Type superType;
            // Whether we should only display Types with statics
            public bool staticsOnly = true;
        }

        public class StateObject : IGraphMetadataItemNotPersistent {
            public fiOption<Type> Type;
        }

        public override Type Edit(Rect region, GUIContent label, Type element, fiGraphMetadata metadata) {
            Rect labelRect, buttonRect = region;

            if (string.IsNullOrEmpty(label.text) == false) {
                fiRectUtility.SplitHorizontalPercentage(region, .3f, 2, out labelRect, out buttonRect);
                GUI.Label(labelRect, label);
            }

            string displayed = "<no type>";
            if (element != null) {
                displayed = element.CSharpName();
            }

            StateObject stateObj = metadata.GetMetadata<StateObject>();
            TypeLookupOptions options = metadata.GetInheritedMetadata<TypeLookupOptions>();

            if (GUI.Button(buttonRect, displayed)) {
                TypeSelectionPopupWindow.CreateSelectionWindow(element, options.superType, type => stateObj.Type = fiOption.Just(type), options.staticsOnly);
            }

            if (stateObj.Type.HasValue) {
                GUI.changed = true;
                var type = stateObj.Type.Value;
                stateObj.Type = fiOption<Type>.Empty;
                return type;
            }

            return element;
        }

        public override float GetElementHeight(GUIContent label, Type element, fiGraphMetadata metadata) {
            return EditorStyles.toolbarButton.CalcHeight(label, Screen.width);
        }
    }
}