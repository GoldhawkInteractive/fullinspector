using System;

namespace FullInspector {
    /// <summary>
    /// Adds whitespace above the given field, property or method (button).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class InspectorMarginAttribute : Attribute, IInspectorAttributeOrder {
        public int Margin;

        public double Order = 0;

        public InspectorMarginAttribute(int margin) {
            Margin = margin;
        }

        double IInspectorAttributeOrder.Order {
            get {
                return Order;
            }
        }
    }
}