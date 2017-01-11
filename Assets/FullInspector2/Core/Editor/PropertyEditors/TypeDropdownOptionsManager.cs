using System;
using System.Collections.Generic;
using System.Linq;
using FullInspector.Modules.Common;
using FullSerializer;
using FullSerializer.Internal;
using UnityEngine;
using DisplayedType = FullInspector.Internal.fiReflectionUtility.DisplayedType;

namespace FullInspector.Internal {
    /// <summary>
    /// Manages the options that are displayed to the user in the instance
    /// selection drop-down.
    /// </summary>
    internal class TypeDropdownOptionsManager {

        private List<DisplayedType> _options;

        /// <summary>
        /// Setup the instance option manager for the given type.
        /// </summary>
        public TypeDropdownOptionsManager(Type baseType, bool allowUncreatableTypes) {
            if (allowUncreatableTypes)
                _options = fiReflectionUtility.GetTypesDeriving(baseType);
            else
                _options = fiReflectionUtility.GetCreatableTypesDeriving(baseType);

            _options.ForEach(e => e.Content = new GUIContent(e.DisplayName));

            _options.Insert(0, new DisplayedType
            {
                Type = null,
                DisplayName = string.Empty,
                Content = new GUIContent("null (" + baseType.CSharpName() + ")")
            });
        }

        private static GUIContent GetOptionName(DisplayedType type, bool addSkipCtorMessage) {
            if (addSkipCtorMessage &&
                type.Type.IsValueType == false &&
                type.Type.GetConstructor(fsPortableReflection.EmptyTypes) == null) {
                return new GUIContent(type.DisplayName + " (skips ctor)");
            }

            return new GUIContent(type.DisplayName);
        }

        /// <summary>
        /// Returns an array of options that should be displayed.
        /// </summary>
        public GUIContent[] GetDisplayOptions(RestrictedTypeMetaData meta = null) {
            // TODO: Cache on meta / internally
            if (meta == null || meta.RestrictedTypes == null || !meta.RestrictedTypes.Any())
                return _options.Select(type => type.Content).ToArray();

            return _options.Where(content => content.Type == null || meta.IsAllowed(content.Type)).Select(type => type.Content).ToArray();

        }

        public Type GetTypeForIndex(int index, Type existingValue) {
            if (index == 0) return null;

            index -= 1; // For the null item
            if (index < _options.Count) return _options[index].Type;
            index -= _options.Count;

            return existingValue;
        }

        public int GetIndexForType(Type type) {
            int offset = 1;

            // try the regular options
            for (int i = 0; i < _options.Count; ++i) {
                Type option = _options[i].Type;
                if (type == option) {
                    return offset + i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the option that should be displayed (from
        /// GetDisplayOptions()) based on the current object instance.
        /// </summary>
        public int GetDisplayOptionIndex(object instance, RestrictedTypeMetaData meta) {
            if (instance == null) {
                return 0;
            }
            

            int offsetI = 0;
            Type instanceType = instance.GetType();
            for (int i = 1; i < _options.Count; ++i)
            {
                Type option = _options[i].Type;

                // Calculate the offset into the list, because these types could be filtered out.
                if (option != null && !meta.IsAllowed(option)) offsetI++;

                if (instanceType == option){
                    return i - offsetI;
                }
            }

            // we need a new display option
            _options.Add(new DisplayedType
            {
                Type = instance.GetType(),
                DisplayName = instance.GetType().CSharpName(),
                Content = new GUIContent(instance.GetType().CSharpName() + " (cannot reconstruct)")
            });

            return _options.Count - 1;
        }

        /// <summary>
        /// Changes the instance of the given object, if necessary.
        /// </summary>
        public object UpdateObjectInstance(object current, int currentIndex, int updatedIndex) {
            // the index has not changed - there will be no change in object
            // instance
            if (currentIndex == updatedIndex) {
                return current;
            }

            // index 0 is always null
            if (updatedIndex == 0) {
                return null;
            }

            // create an instance of the object
            Type currentType = null;
            if (current != null) currentType = current.GetType();

            Type newType = GetTypeForIndex(updatedIndex, currentType);
            if (newType == null) return null;
            return InspectedType.Get(newType).CreateInstance();
        }
    }
}