using System;

namespace FullInspector
{

    /// <summary>
    /// Places restrictions on the allowed types to be selected when selecting a type for instantiation of a field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InspectorRestrictTypeAttribute : Attribute
    {

        /// <summary>
        /// Whether or not the RestrictedTypes will be enforced based on inheritance from, or specific matching.
        /// </summary>
        public bool Inherited;

        /// <summary>
        /// All types which will be removed from selection whenever a Type for the annotated Element needs to be selected.
        /// </summary>
        public Type[] RestrictedTypes;

    }
}
