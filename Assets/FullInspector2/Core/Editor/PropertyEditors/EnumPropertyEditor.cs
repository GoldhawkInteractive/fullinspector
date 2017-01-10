using System;
using UnityEditor;
using UnityEngine;

namespace FullInspector.Internal
{
    /// <summary>
    /// Provides a property editor for enums.
    /// </summary>
    internal class EnumPropertyEditor : IPropertyEditor, IPropertyEditorEditAPI
    {
        public bool DisplaysStandardLabel
        {
            get { return true; }
        }

        public PropertyEditorChain EditorChain
        {
            get;
            set;
        }

        public object OnSceneGUI(object element)
        {
            return element;
        }

        public object Edit(Rect region, GUIContent label, object element, fiGraphMetadata metadata)
        {
            var selected = (Enum)element;

            if (selected.GetType().IsDefined(typeof(FlagsAttribute), /*inherit:*/ true))
            {
                return DrawBitMaskField(region, (int)(object)selected, selected.GetType(), label);
                //return EditorGUI.EnumMaskField(region, label, selected);
            }

            return EditorGUI.EnumPopup(region, label, selected);
        }

        public float GetElementHeight(GUIContent label, object element, fiGraphMetadata metadata)
        {
            return EditorStyles.popup.CalcHeight(label, 100);
        }

        public GUIContent GetFoldoutHeader(GUIContent label, object element)
        {
            return label;
        }

        public bool CanEdit(Type dataType)
        {
            throw new NotSupportedException();
        }

        public static IPropertyEditor TryCreate(Type dataType)
        {
            if (dataType.IsEnum == false)
            {
                return null;
            }

            return new EnumPropertyEditor();
        }

        public static int DrawBitMaskField(Rect aPosition, int aMask, System.Type aType, GUIContent aLabel)
        {
            var itemNames = Enum.GetNames(aType);
            var itemValues = Enum.GetValues(aType) as int[];

            int val = aMask;
            int maskVal = 0;
            for (int i = 0; i < itemValues.Length; i++)
            {
                if (itemValues[i] != 0)
                {
                    if ((val & itemValues[i]) == itemValues[i])
                        maskVal |= 1 << i;
                }
                else if (val == 0)
                    maskVal |= 1 << i;
            }
            int newMaskVal = EditorGUI.MaskField(aPosition, aLabel, maskVal, itemNames);
            int changes = maskVal ^ newMaskVal;

            for (int i = 0; i < itemValues.Length; i++)
            {
                if ((changes & (1 << i)) != 0) // has this list item changed?
                {
                    if ((newMaskVal & (1 << i)) != 0) // has it been set?
                    {
                        if (itemValues[i] == 0) // special case: if "0" is set, just set the val to 0
                        {
                            val = 0;
                            break;
                        }
                        else
                            val |= itemValues[i];
                    }
                    else // it has been reset
                    {
                        val &= ~itemValues[i];
                    }
                }
            }
            return val;
        }
    }
}