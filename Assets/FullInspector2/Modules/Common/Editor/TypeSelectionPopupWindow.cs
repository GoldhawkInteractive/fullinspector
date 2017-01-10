using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FullInspector.Internal;
using FullSerializer;
using FullSerializer.Internal;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Modules {
    public class TypeSelectionPopupWindow : ScriptableWizard {


        // Restrict suggested types to this type
        public Type superType;

        public Type InitialType;
        private Action<Type> _onSelectType;
        private bool _useGlobalFilter = true;
        private bool _showGenericTypes = false;
        private static Dictionary<Type, string> _typeNames = new Dictionary<Type, string>();

        private List<Type> targetList;

        public static TypeSelectionPopupWindow CreateSelectionWindow(Type initialType, Type superType, Action<Type> onSelectType, bool limitToStatics = true) {
            var window = ScriptableWizard.DisplayWizard<TypeSelectionPopupWindow>(limitToStatics ? "Type (with statics) Selector" : "Type Selector");
            window.InitialType = initialType;
            window.minSize = new Vector2(600, 500);
            window._onSelectType = onSelectType;
            window.superType = superType;

            targetList = (limitToStatics ? _allTypesWithStatics : _allTypes);

            var filters = fiSettings.TypeSelectionDefaultFilters;
            if (filters != null) {
                targetList = (from type in targetList
                              where filters.Any(t => type.FullName.ToUpper().Contains(t.ToUpper())) && (superType == null || superType.IsAssignableFrom(type))
                              select type).ToList();
            } else {
                if(superType != null)
                    targetList = targetList.Where(t => superType.IsAssignableFrom(t)).ToList();
                window._useGlobalFilter = false;
            }

            fiEditorUtility.ShouldInspectorRedraw.Push();
            return window;
        }

        public void OnDestroy() {
            fiEditorUtility.ShouldInspectorRedraw.Pop();
        }

        public static List<Type> AllTypes { get { return _allTypes; } }

        private static List<Type> _allTypesWithStatics;
        private static List<Type> _filteredTypesWithStatics;

        private static List<Type> _allTypes;
        private static List<Type> _filteredTypes;

        static TypeSelectionPopupWindow() {
            _allTypesWithStatics = new List<Type>();
            _allTypes = new List<Type>();
            var blackList = fiSettings.TypeSelectionBlacklist;

            foreach (Assembly assembly in fiRuntimeReflectionUtility.GetUserDefinedEditorAssemblies()) {
                foreach (Type type in assembly.GetTypesWithoutException()){
                    _allTypes.add(type);
                    var inspected = InspectedType.Get(type);
                    if (inspected.IsCollection == false) {
                        var shouldAdd = blackList == null ||
                                        !blackList.Any(t => type.FullName.ToUpper().Contains(t.ToUpper()));

                        if (shouldAdd) {
                            _allTypesWithStatics.Add(type);
                            _typeNames.Add(type, type.CSharpName());
                        }
                    }
                }
            }

            _allTypesWithStatics = (from type in _allTypesWithStatics
                                    orderby type.CSharpName()
                                    orderby type.Namespace
                                    select type).ToList();


            _allTypes = (from type in _allTypes
                         orderby type.CSharpName()
                         orderby type.Namespace
                         select type).ToList();
        }

        private Vector2 _scrollPosition;
        private string _searchString = string.Empty;

        private bool PassesSearchFilter(Type type) {
            if (!_showGenericTypes && type != null && type.IsGenericTypeDefinition) {
                return false;
            }

            string typeName = type != null ? type.FullName : "null";
            return typeName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string _customTypeName = string.Empty;
        private int _displayedTypes = 0;

        public void OnGUI() {
            EditorGUILayout.LabelField("Manually Locate Type", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            _customTypeName = EditorGUILayout.TextField("Qualified Type Name", _customTypeName, GUILayout.ExpandWidth(true));

            Type foundType = fsTypeCache.GetType(_customTypeName);

            GUILayout.BeginVertical(GUILayout.Width(100));
            EditorGUI.BeginDisabledGroup(foundType == null);
            if (foundType != null) GUI.color = Color.green;
            if (GUILayout.Button("Select type \u2713")) {
                _onSelectType(foundType);
                Close();
            }
            GUI.color = Color.white;
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            fiEditorGUILayout.Splitter(2);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search for Type", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (fiSettings.TypeSelectionDefaultFilters != null) {
                _useGlobalFilter = GUILayout.Toggle(_useGlobalFilter, "Use global filter");
            }

            _showGenericTypes = GUILayout.Toggle(_showGenericTypes, "Show generic");
            GUILayout.EndHorizontal();


            // For the custom search bar, see:
            // http://answers.unity3d.com/questions/464708/custom-editor-search-bar.html

            GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
            GUILayout.Label("Filter", GUILayout.ExpandWidth(false));
            _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
                // Remove focus if cleared
                _searchString = "";
                GUI.FocusControl(null);
            }
            GUILayout.Label("Found " + _displayedTypes, GUILayout.ExpandWidth(false));
            _displayedTypes = 0;
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            string lastNamespace = string.Empty;

            if (PassesSearchFilter(null)) {
                _displayedTypes++;
                GUILayout.BeginHorizontal();
                GUILayout.Space(35);
                if (GUILayout.Button("null")) {
                    _onSelectType(null);
                    Close();
                }
                GUILayout.EndHorizontal();
            }

            foreach (Type type in targetList) {
                if (PassesSearchFilter(type)) {
                    _displayedTypes++;

                    if (lastNamespace != type.Namespace) {
                        lastNamespace = type.Namespace;
                        GUILayout.Label(type.Namespace ?? "<no namespace>", EditorStyles.boldLabel);
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(35);

                    if (InitialType == type) {
                        GUI.color = Color.green;
                    }

                    EditorGUI.BeginDisabledGroup(type.IsGenericTypeDefinition);

                    string buttonLabel = _typeNames[type];
                    if (type.IsGenericTypeDefinition) buttonLabel += " (generic type definition)";

                    if (GUILayout.Button(buttonLabel)) {
                        _onSelectType(type);
                        Close();
                    }

                    GUI.color = Color.white;

                    EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
