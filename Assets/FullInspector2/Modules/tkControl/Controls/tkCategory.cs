using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FullInspector
{

    public partial class tk<T, TContext>
    {

        /// <summary>
        /// Will draw a GUI.Toolbar which enables a single Element as given by Add.
        /// </summary>
        public class Categories : tkControl<T, TContext>, IEnumerable
        {

            private class Category
            {
                public string name;
                public tkControl<T, TContext> element;
            }

            private readonly List<Category> _categories;
            private readonly VerticalGroup _holder;
            private Toolbar _toolbar;

            public Categories()
            {
                _toolbar = new Toolbar();
                _holder = new VerticalGroup();
                _categories = new List<Category>();

                _holder.Add(_toolbar);
            }

            protected override T DoEdit(Rect rect, T obj, TContext context, fiGraphMetadata metadata)
            {
                return _holder.Edit(rect, obj, context, metadata);
            }

            protected override float DoGetHeight(T obj, TContext context, fiGraphMetadata metadata)
            {
                return _holder.GetHeight(obj, context, metadata);
            }

            public IEnumerator GetEnumerator()
            {
                return _categories.GetEnumerator();
            }

            /// <summary>
            /// Add the given element to this Categories Tool bar.
            /// </summary>
            public void Add(string name, tkControl<T, TContext> element)
            {
                int idx = _categories.Count;
                _categories.Add(new Category { name = name, element = element });

                _toolbar.Options = _categories.Select(category => category.name).ToArray();
                _holder.Add(new ShowIf((input, context) => _toolbar.Selected == idx, element));
            }

        }
    }

}
