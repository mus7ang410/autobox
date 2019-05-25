using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Autobox.Data
{
    public class TagCollection : HashSet<Tag>, INotifyCollectionChanged
    {
        public TagCollection() { }
        public TagCollection(IEnumerable<Tag> collection) : base(collection) { }

        public new bool Add(Tag tag)
        {
            if (base.Add(tag))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, tag));
                return true;
            }
            return false;
        }

        public new bool Remove(Tag tag)
        {
            if (base.Remove(tag))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, tag));
            }
            return false;
        }

        public new void IntersectWith(IEnumerable<Tag> other)
        {
            base.IntersectWith(other);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new void UnionWith(IEnumerable<Tag> other)
        {
            base.UnionWith(other);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // ##### Events
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
