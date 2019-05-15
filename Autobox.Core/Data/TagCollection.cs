using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

namespace Autobox.Data
{
    public class TagCollection : HashSet<string>, INotifyCollectionChanged
    {
        public TagCollection() { }
        public TagCollection(IEnumerable<string> collection) : base(collection) { }

        public new bool Add(string item)
        {
            if (base.Add(item))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                return true;
            }
            return false;
        }

        public new bool Remove(string item)
        {
            if (base.Remove(item))
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            return false;
        }

        public new void IntersectWith(IEnumerable<string> other)
        {
            base.IntersectWith(other);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public new void UnionWith(IEnumerable<string> other)
        {
            base.UnionWith(other);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // ##### Attributes
        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
