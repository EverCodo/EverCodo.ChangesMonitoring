using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Monitors collection changes and changes in the collection items.
    /// </summary>
    public class CollectionChangesMonitor : ChangesMonitor
    {
        internal CollectionChangesMonitor(INotifyCollectionChanged root, ChangesMonitor parentMonitor, string propertyName,
            bool? monitorOnlyMarkedPropertiesOverride, bool monitorCollectionItemSublevels)
            : base(root, parentMonitor, propertyName, monitorOnlyMarkedPropertiesOverride)
        {
            _Root = root;
            _MonitorCollectionItemSublevels = monitorCollectionItemSublevels;
            Attach();
        }

        internal CollectionChangesMonitor(INotifyCollectionChanged root, string id, string propertyName,
            bool monitorOnlyMarkedProperties, bool useWeakEvents, bool monitorCollectionItemSublevels)
            : base(root, id, propertyName, monitorOnlyMarkedProperties, useWeakEvents)
        {
            _Root = root;
            _MonitorCollectionItemSublevels = monitorCollectionItemSublevels;
            Attach();
        }

        /// <summary>
        /// Detaches changes monitoring.
        /// </summary>
        public override void Dispose()
        {
            Detach();
        }

        #region Root Attaching and Detaching

        /// <summary>
        /// Root as notifying collection.
        /// </summary>
        private readonly INotifyCollectionChanged _Root;

        /// <summary>
        /// Weak event subscription handler.
        /// </summary>
        private CollectionChangedWeakEventSubscription _RootCollectionChangedWeakEventSubscription;

        /// <summary>
        /// Attaches monitoring to collection elements.
        /// </summary>
        private void Attach()
        {
            // subscribe to root properties changes
            if (UseWeakEvents)
                _RootCollectionChangedWeakEventSubscription = new CollectionChangedWeakEventSubscription(_Root, Root_CollectionChanged);
            else
                _Root.CollectionChanged += Root_CollectionChanged;

            // attach to all items
            if (_MonitorCollectionItemSublevels)
                AttachToAllItems();
        }

        /// <summary>
        /// Detaches monitoring from collection.
        /// </summary>
        private void Detach()
        {
            // unsubscribe from root properties changes
            if (UseWeakEvents)
                _RootCollectionChangedWeakEventSubscription.Dispose();
            else
                _Root.CollectionChanged -= Root_CollectionChanged;

            if (_MonitorCollectionItemSublevels)
                ClearItemsMonitoring();
        }

        #endregion

        #region Collection Items Attaching and Detaching

        /// <summary>
        /// Flag specifying if collection items should be monitored on property changes.
        /// </summary>
        private readonly bool _MonitorCollectionItemSublevels;

        /// <summary>
        /// Monitors for items.
        /// </summary>
        private readonly Dictionary<object, ChangesMonitor> _ItemMonitors
            = new Dictionary<object, ChangesMonitor>(new ReferenceEqualityComparer<object>());

        /// <summary>
        /// Attaches monitoring to the item.
        /// </summary>
        /// <param name="item">Collection item.</param>
        private void AttachToItem(object item)
        {
            if (item == null)
                return;

            // special processing for KeyValuePair items to support observable dictionaries
            // this subscribes for changes inside key and value of provided pair
            if (IsKeyValuePair(item.GetType()))
            {
                AttachToKeyValuePair(item);
                return;
            }

            // create changes monitor for the item
            var itemMonitor = Create(item, this, "Item[]", null, true);

            // validate
            if (itemMonitor == null)
                return;

            // subscribe
            itemMonitor.Changed += ItemMonitor_Changed;

            // add to the registry
            _ItemMonitors.Add(item, itemMonitor);
        }

        /// <summary>
        /// Checks if provided item is KeyValuePair.
        /// </summary>
        /// <param name="itemType">Item type to check.</param>
        /// <returns>true if item is key value pair; false otherwise.</returns>
        private static bool IsKeyValuePair(Type itemType)
        {
            if (itemType.IsGenericType && itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                return true;

            return itemType.BaseType != null && IsKeyValuePair(itemType.BaseType);
        }

        /// <summary>
        /// Attaches to key and value of KeyValuePair.
        /// </summary>
        /// <param name="item">KeyValuePair item to attach monitoring.</param>
        private void AttachToKeyValuePair(object item)
        {
            var itemType = item.GetType();
            var key = itemType.GetProperty("Key").GetValue(item, null);
            AttachToItem(key);

            var value = itemType.GetProperty("Value").GetValue(item, null);
            AttachToItem(value);
        }

        /// <summary>
        /// Detaches monitoring from the item.
        /// </summary>
        /// <param name="item">Collection item.</param>
        private void DetachFromItem(object item)
        {
            if (item == null)
                return;

            // special processing for KeyValuePair items to support observable dictionaries
            // this unsubscribes from changes inside key and value of provided pair
            if (IsKeyValuePair(item.GetType()))
            {
                DetachFromKeyValuePair(item);
                return;
            }

            if (_ItemMonitors.TryGetValue(item, out var itemMonitor))
            {
                itemMonitor.Changed -= ItemMonitor_Changed;
                itemMonitor.Dispose();
                _ItemMonitors.Remove(item);
            }
        }

        /// <summary>
        /// Detaches from key and value of KeyValuePair.
        /// </summary>
        /// <param name="item">KeyValuePair item to detach monitoring.</param>
        private void DetachFromKeyValuePair(object item)
        {
            var itemType = item.GetType();
            var key = itemType.GetProperty("Key").GetValue(item, null);
            DetachFromItem(key);

            var value = itemType.GetProperty("Value").GetValue(item, null);
            DetachFromItem(value);
        }

        /// <summary>
        /// Attaches monitoring to all items.
        /// </summary>
        private void AttachToAllItems()
        {
            if (!(_Root is IEnumerable enumerable))
                return;

            foreach (var item in enumerable)
                AttachToItem(item);
        }

        /// <summary>
        /// Disposes all item monitors.
        /// </summary>
        private void ClearItemsMonitoring()
        {
            foreach (var itemMonitor in _ItemMonitors)
            {
                itemMonitor.Value.Changed -= ItemMonitor_Changed;
                itemMonitor.Value.Dispose();
            }

            _ItemMonitors.Clear();
        }

        #endregion

        #region Collection Items Changes Handling

        /// <summary>
        /// Maintains monitoring on root collection changes.
        /// </summary>
        private void Root_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (_MonitorCollectionItemSublevels)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Replace:
                        // detach from old items
                        if (args.OldItems != null)
                        {
                            foreach (var oldItem in args.OldItems)
                                DetachFromItem(oldItem);
                        }

                        // attach to new items
                        if (args.NewItems != null)
                        {
                            foreach (var newItem in args.NewItems)
                                AttachToItem(newItem);
                        }
                        break;

                    case NotifyCollectionChangedAction.Move:
                        // no need to attach/detach on items moving
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        // detach from all items
                        ClearItemsMonitoring();
                        // attach to all items
                        AttachToAllItems();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            RaiseChanged(new MonitoredObjectChangedEventArgs(this, sender, args));
        }

        /// <summary>
        /// Redirects change notifications from items to subscribers.
        /// </summary>
        private void ItemMonitor_Changed(object sender, MonitoredObjectChangedEventArgs args)
        {
            RaiseChanged(args);
        }

        #endregion
    }
}