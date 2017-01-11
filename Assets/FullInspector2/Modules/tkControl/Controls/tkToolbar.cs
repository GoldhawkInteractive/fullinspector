using System;
using UnityEngine;

namespace FullInspector
{

    public partial class tk<T, TContext>
    {

        /// <summary>
        /// Will draw a GUI.Toolbar inside the given rectangle.
        /// </summary>
        public class Toolbar : tkControl<T, TContext>
        {

            protected Action<T, TContext, int, string> _onClick;

            public string[] Options { get; set; }
            public int Selected { get; private set; }

            public Toolbar(Action<T, TContext, int, string> onClick, params string[] options)
            {
                _onClick = onClick;
                Options = options;
            }

            public Toolbar(Action<int, string> onClick, params string[] options) : this((t, cont, sel, name) => onClick(sel, name), options)
            {
            }

            public Toolbar(params string[] options) : this((arg1, context, arg3, arg4) => { }, options)
            {
            }

            protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
            {
                var newSelected = GUI.Toolbar(rect, Selected, Options);
                if (newSelected == Selected) return obj;

                Selected = newSelected;

                if (_onClick != null){
                    _onClick(obj, context, Selected, Options[Selected]);
                }
                return obj;
            }

            protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
            {
                return 18;
            }
        }
    }

}
