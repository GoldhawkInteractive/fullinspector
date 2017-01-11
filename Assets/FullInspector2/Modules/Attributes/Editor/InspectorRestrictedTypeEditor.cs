using UnityEditor;
using UnityEngine;
using System;

namespace FullInspector.Modules.Common
{

    /// <summary>
    /// See <see cref="InspectorRestrictTypeAttribute"/>
    /// </summary>
    public class RestrictedTypeMetaData : IGraphMetadataItemNotPersistent
    {

        public bool Inherited;

        public Type[] RestrictedTypes = { };

        /// <summary>
        /// Returns whether the given Type is filtered out by this RestrictedTypeMetaData
        /// </summary>
        public bool IsAllowed(Type type)
        {
            foreach (var restrictedType in RestrictedTypes)
            {
                if (Inherited
                    ? restrictedType.IsAssignableFrom(type)
                    : type == restrictedType)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Adds in the appropriate Metadata from the Attribute 
    /// </summary>
    [CustomAttributePropertyEditor(typeof(InspectorRestrictTypeAttribute), ReplaceOthers = false)]
    public class InspectorRestrictedTypeAttributeEditor : AttributePropertyEditor<object, InspectorRestrictTypeAttribute>
    {
        protected override object Edit(Rect region, GUIContent label, object element, InspectorRestrictTypeAttribute attribute, fiGraphMetadata metadata)
        {

            var metaParent = metadata.Parent;
            var restrictedMeta = metaParent.GetMetadata<RestrictedTypeMetaData>();
            restrictedMeta.Inherited = attribute.Inherited;
            restrictedMeta.RestrictedTypes = attribute.RestrictedTypes;

            return element;
        }

        protected override float GetElementHeight(GUIContent label, object element, InspectorRestrictTypeAttribute attribute,
            fiGraphMetadata metadata)
        {
            return 0;
        }
    }
}