using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Base class for changes monitors.
    /// </summary>
    public abstract class ChangesMonitor : IDisposable
    {
        /// <summary>
        /// Creates proper changes monitor for object.
        /// </summary>
        /// <param name="root">Object to monitor.</param>
        /// <returns>Changes monitor for object.</returns>
        public static ChangesMonitor Create(object root)
        {
            return Create(null, root, false, false);
        }

        /// <summary>
        /// Creates proper changes monitor for object.
        /// </summary>
        /// <param name="id">Id of monitor.</param>
        /// <param name="root">Object to monitor.</param>
        /// <param name="monitorOnlyMarkedProperties">Specifies if only properties marked with MonitorChangesAttribute should be monitored.</param>
        /// <param name="useWeakEvents">Specifies if changes monitor will use weak events when subscribes to monitored objects.</param>
        /// <returns>Changes monitor for object.</returns>
        public static ChangesMonitor Create(string id, object root, bool monitorOnlyMarkedProperties, bool useWeakEvents)
        {
            switch (root)
            {
                // notifying collection should always be monitored and has precedence over notifying object
                case INotifyCollectionChanged rootCollection:
                    return new CollectionChangesMonitor(rootCollection, id, null, monitorOnlyMarkedProperties, useWeakEvents, true);

                // in greedy mode (when all properties are monitored regardless of attributes) notifying object always requires monitoring
                // in strict mode it requires monitoring only if it has properties to monitor
                case INotifyPropertyChanged rootObject
                    when !monitorOnlyMarkedProperties || PropertyChangesMonitor.GetMonitoredTypeMetadata(rootObject.GetType(), id, true).HasMonitoredProperties:
                    return new PropertyChangesMonitor(rootObject, id, null, monitorOnlyMarkedProperties, useWeakEvents);

                // no suitable monitor can be created
                default:
                    return null;
            }
        }

        /// <summary>
        /// Creates nested changes monitor for object.
        /// </summary>
        /// <param name="root">Object to monitor.</param>
        /// <param name="parentMonitor">External parent monitor.</param>
        /// <param name="propertyName">Name of property this changes monitor is created for.</param>
        /// <param name="monitorOnlyMarkedPropertiesOverride">Specifies if only properties marked with MonitorChangesAttribute should be monitored.</param>
        /// <param name="monitorSublevels">Specifies if sublevels should be monitored. Used only for collections changes monitoring.</param>
        /// <returns>Changes monitor for object.</returns>
        protected static ChangesMonitor Create(object root, ChangesMonitor parentMonitor, string propertyName, bool? monitorOnlyMarkedPropertiesOverride, bool monitorSublevels)
        {
            switch (root)
            {
                // notifying collection should always be monitored and has precedence over notifying object
                case INotifyCollectionChanged rootCollection:
                    return new CollectionChangesMonitor(rootCollection, parentMonitor, propertyName, monitorOnlyMarkedPropertiesOverride, monitorSublevels);

                // in greedy mode (when all properties are monitored regardless of attributes) notifying object always requires monitoring
                // in strict mode it requires monitoring only if it has properties to monitor
                case INotifyPropertyChanged rootObject
                    when !parentMonitor.MonitorOnlyMarkedProperties || PropertyChangesMonitor.GetMonitoredTypeMetadata(rootObject.GetType(), parentMonitor.Id, true).HasMonitoredProperties:
                    return new PropertyChangesMonitor(rootObject, parentMonitor, propertyName, monitorOnlyMarkedPropertiesOverride);

                // no suitable monitor can be created
                default:
                    return null;
            }
        }

        /// <summary>
        /// Initializes changes monitor.
        /// </summary>
        /// <param name="root">Object to monitor.</param>
        /// <param name="id">Id of monitor.</param>
        /// <param name="propertyName">Name of property this changes monitor is created for.</param>
        /// <param name="monitorOnlyMarkedProperties">Specifies if only properties marked with MonitorChangesAttribute should be monitored.</param>
        /// <param name="useWeakEvents">Specifies if changes monitor will use weak events when subscribes to monitored objects.</param>
        protected ChangesMonitor(object root, string id, string propertyName, bool monitorOnlyMarkedProperties, bool useWeakEvents)
        {
            Root = root ?? throw new ArgumentNullException(nameof(root));
            Id = id;
            PropertyName = propertyName;
            MonitorOnlyMarkedProperties = monitorOnlyMarkedProperties;
            UseWeakEvents = useWeakEvents;
        }

        /// <summary>
        /// Initializes changes monitor.
        /// </summary>
        /// <param name="root">Object to monitor.</param>
        /// <param name="parentMonitor">External parent monitor.</param>
        /// <param name="propertyName">Name of property this changes monitor is created for.</param>
        /// <param name="monitorOnlyMarkedPropertiesOverride">Specifies if only properties marked with MonitorChangesAttribute should be monitored.</param>
        protected ChangesMonitor(object root, ChangesMonitor parentMonitor, string propertyName, bool? monitorOnlyMarkedPropertiesOverride)
            : this(root, parentMonitor?.Id, propertyName,
                monitorOnlyMarkedPropertiesOverride ?? parentMonitor != null && parentMonitor.MonitorOnlyMarkedProperties,
                parentMonitor != null && parentMonitor.UseWeakEvents)
        {
            ParentMonitor = parentMonitor;
        }

        /// <summary>
        /// Root object to monitor changes for.
        /// </summary>
        public object Root { get; }

        /// <summary>
        /// Id of monitor. May be null.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Parent changes monitor.
        /// </summary>
        public ChangesMonitor ParentMonitor { get; }

        /// <summary>
        /// Name of property this changes monitor is created for in parent monitor.
        /// </summary>
        /// <remarks>
        /// Name of property in ParentMonitor.Root object where this.Root seats (value of ParentMonitor.Root.PropertyName is Root).
        /// </remarks>
        public string PropertyName { get; }

        /// <summary>
        /// Specifies if only properties marked with MonitorChangesAttribute should be monitored.
        /// </summary>
        public bool MonitorOnlyMarkedProperties { get; }

        /// <summary>
        /// Specifies if changes monitor will use weak events when subscribes to monitored objects.
        /// </summary>
        public bool UseWeakEvents { get; }

        /// <summary>
        /// Detaches changes monitoring.
        /// </summary>
        public abstract void Dispose();

        #region Change Notification

        /// <summary>
        /// Event raised when one of monitored object changes.
        /// </summary>
        public event EventHandler<MonitoredObjectChangedEventArgs> Changed;

        /// <summary>
        /// Raises Changed event.
        /// </summary>
        /// <param name="args">Event args.</param>
        protected virtual void RaiseChanged(MonitoredObjectChangedEventArgs args)
        {
            if (IsNotificationSuppressed)
                return;

            Changed?.Invoke(this, args);
        }

        /// <summary>
        /// Counter for notification suppression.
        /// </summary>
        private int _NotificationSuppressionCounter;

        /// <summary>
        /// Checks if notification is suppressed.
        /// </summary>
        public bool IsNotificationSuppressed => _NotificationSuppressionCounter > 0;

        /// <summary>
        /// Suppresses notification.
        /// </summary>
        public void SuppressNotification()
        {
            _NotificationSuppressionCounter++;
        }

        /// <summary>
        /// Resumes notification.
        /// </summary>
        public void ResumeNotification()
        {
            _NotificationSuppressionCounter--;
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Provides enumeration to hierarchy monitors from current to the topmost.
        /// </summary>
        /// <returns>Enumeration with monitors from current to topmost of the whole hierarchy.</returns>
        public IEnumerable<ChangesMonitor> GetHierarchyMonitors()
        {
            var changesMonitor = this;

            do
            {
                yield return changesMonitor;
                changesMonitor = changesMonitor.ParentMonitor;
            } while (changesMonitor != null);
        }

        /// <summary>
        /// Provides enumeration to monitored hierarchy root objects from current root to the topmost monitored root.
        /// </summary>
        /// <returns>Enumeration with root objects from current to topmost root of the whole hierarchy.</returns>
        public IEnumerable<object> GetHierarchyRoots()
        {
            return GetHierarchyMonitors().Select(monitor => monitor.Root);
        }

        /// <summary>
        /// Checks if this monitor is inside monitoring hierarchy for specified root object.
        /// </summary>
        /// <param name="root">Root object of hierarchy to check.</param>
        /// <returns>true if monitor is inside monitored hierarchy rooted by specified object; false otherwise.</returns>
        public bool IsMonitoringInsideObject(object root)
        {
            return GetHierarchyRoots().Any(intermediateRoot => ReferenceEquals(intermediateRoot, root));
        }

        /// <summary>
        /// Checks if this monitor is inside monitoring hierarchy for specified root object type.
        /// </summary>
        /// <param name="rootType">Root object type of hierarchy to check.</param>
        /// <returns>true if monitor is inside monitored hierarchy rooted by object of the specified type; false otherwise.</returns>
        public bool IsMonitoringInsideObjectOfType(Type rootType)
        {
            return GetHierarchyRoots().Any(rootType.IsInstanceOfType);
        }

        /// <summary>
        /// Composes property path from topmost root to this changes monitor.
        /// </summary>
        /// <returns>Composed property path.</returns>
        /// <remarks>
        /// 1. For topmost root changes monitor it will be String.Empty.
        /// 2. For each level it will add ".PropertyName" (for property) or ".Item[]" (for collection item).
        /// 3. Example: ".Persons.Item[].Address"
        /// </remarks>
        public string GetPropertyPath()
        {
            var pathBuilder = new StringBuilder();
            foreach (var changesMonitor in GetHierarchyMonitors())
            {
                pathBuilder.Insert(0, changesMonitor.PropertyName != null ? "." + changesMonitor.PropertyName : null);
            }

            return pathBuilder.ToString();
        }

        #endregion
    }
}