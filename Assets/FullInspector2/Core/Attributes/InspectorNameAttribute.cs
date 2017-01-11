using System;

namespace FullInspector {
    /// <summary>
    /// Override the default name that is used for display in the inspector and
    /// use a custom name instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InspectorNameAttribute : Attribute {
        /// <summary>
        /// The name of the field, property, or button. If this is null or the
        /// empty string, then a default name generated off of the reflected name
        /// will be used instead.
        /// </summary>
        public string DisplayName;


        /// <summary>
        /// If set to true, will disable the drawing of a name label for inspected properties.
        /// As a side effect, will collapse the block by removing indentation.
        /// </summary>
        public bool Hide;

        /// <summary>
        /// Set the name of the member.
        /// </summary>
        public InspectorNameAttribute(string displayName) {
            DisplayName = displayName;
        }
    }
}