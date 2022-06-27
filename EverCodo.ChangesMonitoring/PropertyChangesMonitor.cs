using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Monitors object properties changes.
    /// </summary>
    public class PropertyChangesMonitor : ChangesMonitor
    {
        internal PropertyChangesMonitor(INotifyPropertyChanged root, ChangesMonitor parentMonitor, string propertyName,
            bool? monitorOnlyMarkedPropertiesOverride)
            : base(root, parentMonitor, propertyName, monitorOnlyMarkedPropertiesOverride)
        {
            _Root = root;
            _RootTypeMetadata = GetMonitoredTypeMetadata(Root.GetType(), Id, MonitorOnlyMarkedProperties);
            Attach();
        }

        internal PropertyChangesMonitor(INotifyPropertyChanged root, string id, string propertyName,
            bool monitorOnlyMarkedProperties, bool useWeakEvents)
            : base(root, id, propertyName, monitorOnlyMarkedProperties, useWeakEvents)
        {
            _Root = root;
            _RootTypeMetadata = GetMonitoredTypeMetadata(Root.GetType(), Id, MonitorOnlyMarkedProperties);
            Attach();
        }

        /// <summary>
        /// Detaches changes monitoring.
        /// </summary>
        public override void Dispose()
        {
            Detach();
        }

        #region Monitored Types Metadata Caching

        /// <summary>
        /// Key to index monitoring metadata in cache.
        /// </summary>
        internal class MonitoredTypeMetadataKey : IEquatable<MonitoredTypeMetadataKey>
        {
            public MonitoredTypeMetadataKey(Type type, string id, bool monitorOnlyMarkedProperties)
            {
                Type = type;
                Id = id;
                MonitorOnlyMarkedProperties = monitorOnlyMarkedProperties;
            }

            /// <summary>
            /// Type of the monitored object.
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Monitor identifier.
            /// </summary>
            public string Id { get; }

            /// <summary>
            /// Specifies if only marked properties of the root type are monitored.
            /// </summary>
            public bool MonitorOnlyMarkedProperties { get; }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <returns>
            /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
            /// </returns>
            /// <param name="other">An object to compare with this object.</param>
            public bool Equals(MonitoredTypeMetadataKey other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return Type == other.Type
                       && string.Equals(Id, other.Id)
                       && MonitorOnlyMarkedProperties == other.MonitorOnlyMarkedProperties;
            }

            /// <summary>
            /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
            /// </summary>
            /// <returns>
            /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
            /// </returns>
            /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>. </param>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj.GetType() != this.GetType())
                    return false;
                return Equals((MonitoredTypeMetadataKey)obj);
            }

            /// <summary>
            /// Serves as a hash function for a particular type. 
            /// </summary>
            /// <returns>
            /// A hash code for the current <see cref="T:System.Object"/>.
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = (Type != null ? Type.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ MonitorOnlyMarkedProperties.GetHashCode();
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Metadata for monitored type.
        /// </summary>
        internal class MonitoredTypeMetadata
        {
            /// <summary>
            /// Loads metadata for monitored type.
            /// </summary>
            /// <param name="monitoredTypeMetadataKey">Key which is used to identify metadata.</param>
            public static MonitoredTypeMetadata Load(MonitoredTypeMetadataKey monitoredTypeMetadataKey)
            {
                // get all public instance properties of the monitored type
                var properties = monitoredTypeMetadataKey.Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                // load monitoring settings for each property
                var propertiesMetadata = new Dictionary<PropertyInfo, MonitoredPropertyMetadata>();
                foreach (var propertyInfo in properties)
                {
                    var propertyMetadata = MonitoredPropertyMetadata.Load(propertyInfo, monitoredTypeMetadataKey);

                    // store metadata for all properties in greedy mode to be able to check which properties were excluded explicitly
                    // or store only metadata for monitored properties in strict mode as we need to track only explicitly included properties
                    if (!monitoredTypeMetadataKey.MonitorOnlyMarkedProperties || propertyMetadata.IsMonitored)
                        propertiesMetadata.Add(propertyInfo, propertyMetadata);
                }

                // index all monitored properties by name for future lookups
                var trackedProperties = propertiesMetadata.ToLookup(keyValue => keyValue.Key.Name, keyValue => keyValue.Key);

                return new MonitoredTypeMetadata(trackedProperties, propertiesMetadata);
            }

            /// <summary>
            /// Initialize monitored type metadata.
            /// </summary>
            /// <param name="trackedProperties">Index of tracked properties.</param>
            /// <param name="propertiesMetadata">Index of tracked properties metadata.</param>
            private MonitoredTypeMetadata(ILookup<string, PropertyInfo> trackedProperties, IReadOnlyDictionary<PropertyInfo, MonitoredPropertyMetadata> propertiesMetadata)
            {
                TrackedProperties = trackedProperties;
                PropertiesMetadata = propertiesMetadata;
            }

            /// <summary>
            /// Checks if metadata object contains descriptors for properties to monitor.
            /// </summary>
            public bool HasMonitoredProperties => PropertiesMetadata.Values.Any(metadata => metadata.IsMonitored);

            /// <summary>
            /// Tracked properties descriptors indexed by name.
            /// </summary>
            public ILookup<string, PropertyInfo> TrackedProperties { get; }

            /// <summary>
            /// Metadata for each monitored property.
            /// </summary>
            public IReadOnlyDictionary<PropertyInfo, MonitoredPropertyMetadata> PropertiesMetadata { get; }
        }

        /// <summary>
        /// Metadata for specific monitored property.
        /// </summary>
        internal class MonitoredPropertyMetadata
        {
            /// <summary>
            /// Loads property monitoring settings from attributes.
            /// </summary>
            /// <param name="propertyInfo">Property info.</param>
            /// <param name="monitoredTypeMetadataKey">Key which is used to identify metadata.</param>
            public static MonitoredPropertyMetadata Load(PropertyInfo propertyInfo, MonitoredTypeMetadataKey monitoredTypeMetadataKey)
            {
                // NOTE: Attribute.GetCustomAttributes() is used instead of propertyInfo.GetCustomAttributes() to workaround known .NET behavior: http://social.msdn.microsoft.com/Forums/en-US/winforms/thread/e6bb4146-eb1a-4c1b-a5b1-f3528d8a7864
                var attributes = Attribute.GetCustomAttributes(propertyInfo, typeof(MonitorChangesAttribute), true).Cast<MonitorChangesAttribute>();

                // get attribute with the most priority
                MonitorChangesAttribute actualMonitoringAttribute;
                if (monitoredTypeMetadataKey.MonitorOnlyMarkedProperties)
                {
                    // only attributes with equal Id are considered in strict mode
                    actualMonitoringAttribute = attributes.Where(attribute => attribute.Id == monitoredTypeMetadataKey.Id)
                        .OrderByDescending(attribute => attribute, MonitorChangesAttributePriorityComparer.Instance).FirstOrDefault();
                }
                else
                {
                    // attributes related to all monitors (with Id == null) or this monitor specifically
                    actualMonitoringAttribute = attributes.Where(attribute => attribute.Id == null || attribute.Id == monitoredTypeMetadataKey.Id)
                        .OrderByDescending(attribute => attribute, MonitorChangesAttributePriorityComparer.Instance).FirstOrDefault();
                }

                var monitorProperty = !monitoredTypeMetadataKey.MonitorOnlyMarkedProperties;
                var monitorSublevels = !monitoredTypeMetadataKey.MonitorOnlyMarkedProperties;
                bool? monitorOnlyMarkedProperties = null;

                // set tracking settings from MonitorChangesAttribute
                if (actualMonitoringAttribute != null)
                {
                    monitorProperty = actualMonitoringAttribute.MonitorProperty;
                    monitorSublevels = actualMonitoringAttribute.MonitorSublevels;
                    monitorOnlyMarkedProperties = actualMonitoringAttribute.IsMonitorOnlyMarkedPropertiesInitialized
                        ? actualMonitoringAttribute.MonitorOnlyMarkedProperties
                        : (bool?)null;
                }

                return new MonitoredPropertyMetadata(monitorProperty, monitorSublevels, monitorOnlyMarkedProperties);
            }

            /// <summary>
            /// Initializes monitored property metadata.
            /// </summary>
            /// <param name="isPropertyMonitored">Specifies if property itself is monitored.</param>
            /// <param name="isSublevelsMonitored">Specifies if property sublevels are monitored.</param>
            /// <param name="monitorOnlyMarkedProperties">Specifies if only marked properties should be monitored on sublevels.</param>
            private MonitoredPropertyMetadata(bool isPropertyMonitored, bool isSublevelsMonitored, bool? monitorOnlyMarkedProperties)
            {
                IsPropertyMonitored = isPropertyMonitored;
                IsSublevelsMonitored = isSublevelsMonitored;
                MonitorOnlyMarkedProperties = monitorOnlyMarkedProperties;
            }

            /// <summary>
            /// Specifies if property is monitored itself or on sublevels.
            /// </summary>
            public bool IsMonitored => IsPropertyMonitored || IsSublevelsMonitored;

            /// <summary>
            /// Specifies if property itself should be monitored for changes.
            /// </summary>
            public bool IsPropertyMonitored { get; }

            /// <summary>
            /// Specifies if sublevel properties of property value should be monitored for changes.
            /// </summary>
            public bool IsSublevelsMonitored { get; }

            /// <summary>
            /// Specifies if only marked properties should be monitored on sublevels.
            /// </summary>
            /// <remarks>null means "not specified": the flag value will be acquired from parent changes monitor.</remarks>
            public bool? MonitorOnlyMarkedProperties { get; }
        }

        /// <summary>
        /// Cache of monitored types metadata.
        /// </summary>
        private static readonly Dictionary<MonitoredTypeMetadataKey, WeakReference> MonitoredTypesMetadataCache = new Dictionary<MonitoredTypeMetadataKey, WeakReference>();

        /// <summary>
        /// Gets metadata for monitored type.
        /// </summary>
        /// <param name="monitoredType">Type of monitored object.</param>
        /// <param name="id">Identifier of changes monitor used.</param>
        /// <param name="monitorOnlyMarkedProperties">Specifies if only properties marked with MonitorChanges attribute should be considered.</param>
        /// <returns>Metadata for monitored object type.</returns>
        internal static MonitoredTypeMetadata GetMonitoredTypeMetadata(Type monitoredType, string id, bool monitorOnlyMarkedProperties)
        {
            lock (MonitoredTypesMetadataCache)
            {
                var monitorMetadataKey = new MonitoredTypeMetadataKey(monitoredType, id, monitorOnlyMarkedProperties);

                // try to get metadata from cache
                MonitoredTypeMetadata monitoredTypeMetadata = null;
                if (MonitoredTypesMetadataCache.TryGetValue(monitorMetadataKey, out var cachedMetadataReference))
                    monitoredTypeMetadata = (MonitoredTypeMetadata)cachedMetadataReference.Target;

                // load metadata and put to cache if needed
                if (monitoredTypeMetadata == null)
                {
                    monitoredTypeMetadata = MonitoredTypeMetadata.Load(monitorMetadataKey);
                    MonitoredTypesMetadataCache[monitorMetadataKey] = new WeakReference(monitoredTypeMetadata);
                }

                return monitoredTypeMetadata;
            }
        }

        #endregion

        #region Root Attaching and Detaching

        /// <summary>
        /// Root as notifying object.
        /// </summary>
        private readonly INotifyPropertyChanged _Root;

        /// <summary>
        /// Metadata settings of monitored Root object type.
        /// </summary>
        private readonly MonitoredTypeMetadata _RootTypeMetadata;

        /// <summary>
        /// Weak event subscription handler.
        /// </summary>
        private PropertyChangedWeakEventSubscription _RootPropertyChangedWeakEventSubscription;

        /// <summary>
        /// Attaches changes monitoring.
        /// </summary>
        private void Attach()
        {
            // subscribe to root properties changes
            if (UseWeakEvents)
                _RootPropertyChangedWeakEventSubscription = new PropertyChangedWeakEventSubscription(_Root, Root_PropertyChanged);
            else
                _Root.PropertyChanged += Root_PropertyChanged;

            // set up properties monitoring
            SetupPropertiesMonitoring();
        }

        /// <summary>
        /// Detaches changing monitoring.
        /// </summary>
        private void Detach()
        {
            // unsubscribe from root properties changes
            if (UseWeakEvents)
                _RootPropertyChangedWeakEventSubscription.Dispose();
            else
                _Root.PropertyChanged -= Root_PropertyChanged;

            // dispose all property monitors
            ClearPropertiesMonitoring();
        }

        #endregion

        #region Properties Attaching and Detaching

        /// <summary>
        /// Monitors for values of root object properties.
        /// </summary>
        private readonly Dictionary<PropertyInfo, ChangesMonitor> _PropertyMonitors = new Dictionary<PropertyInfo, ChangesMonitor>();

        /// <summary>
        /// Properties descriptors indexed by corresponding created changes monitors.
        /// </summary>
        private readonly Dictionary<ChangesMonitor, PropertyInfo> _MonitoredProperties = new Dictionary<ChangesMonitor, PropertyInfo>();

        /// <summary>
        /// Provides tracked properties info for property name.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        /// <returns>Collection of property descriptors.</returns>
        private IEnumerable<PropertyInfo> GetTrackedProperties(string propertyName)
        {
            // extract proper property name from indexer change notification
            var indexerParameterStartIndex = propertyName.IndexOf("[", StringComparison.Ordinal);
            if (indexerParameterStartIndex > 0)
                propertyName = propertyName.Substring(0, indexerParameterStartIndex);

            // get property descriptors
            return _RootTypeMetadata.TrackedProperties[propertyName];
        }

        /// <summary>
        /// Provides property metadata.
        /// </summary>
        /// <param name="property">Property descriptor.</param>
        /// <returns>Metadata for property if any.</returns>
        private MonitoredPropertyMetadata GetPropertyMetadata(PropertyInfo property)
        {
            _RootTypeMetadata.PropertiesMetadata.TryGetValue(property, out var metadata);
            return metadata;
        }

        /// <summary>
        /// Checks if property is monitored for changes.
        /// </summary>
        /// <param name="property">Property descriptor.</param>
        /// <returns>true if property is monitored for changes; false otherwise.</returns>
        private bool IsPropertyMonitored(PropertyInfo property)
        {
            var metadata = GetPropertyMetadata(property);
            return metadata != null && metadata.IsPropertyMonitored;
        }

        /// <summary>
        /// Sets up monitoring for all properties of the root.
        /// </summary>
        private void SetupPropertiesMonitoring()
        {
            // get all public instance properties of the root
            var properties = Root.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // create monitors for values of properties
            foreach (var propertyInfo in properties)
            {
                // attach monitoring to property
                AttachToProperty(propertyInfo);
            }
        }

        /// <summary>
        /// Clears all properties monitors.
        /// </summary>
        private void ClearPropertiesMonitoring()
        {
            // dispose all property monitors
            foreach (var propertyMonitor in _PropertyMonitors)
            {
                propertyMonitor.Value.Changed -= PropertyMonitor_Changed;
                propertyMonitor.Value.Dispose();
            }

            // clear monitors collection
            _PropertyMonitors.Clear();
            _MonitoredProperties.Clear();
        }

        /// <summary>
        /// Attaches changes monitor to property value.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void AttachToProperty(string propertyName)
        {
            foreach (var property in GetTrackedProperties(propertyName))
                AttachToProperty(property);
        }

        /// <summary>
        /// Attaches changes monitor to property value.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        private void AttachToProperty(PropertyInfo propertyInfo)
        {
            // indexed properties are not supported on sublevels
            if (propertyInfo.GetIndexParameters().Length > 0)
                return;

            // no metadata means not monitored property
            var propertyMetadata = GetPropertyMetadata(propertyInfo);
            if (propertyMetadata == null)
                return;

            // if property and its sublevels should not be monitored, don't create monitor for the property
            if (!propertyMetadata.IsPropertyMonitored && !propertyMetadata.IsSublevelsMonitored)
                return;

            // get property value
            object propertyValue = null;
            try
            {
                propertyValue = propertyInfo.GetValue(Root, null);
            }
            catch
            {
                // consider all exceptions on property access as no property value available
            }

            // if value is not available, don't attach
            if (propertyValue == null)
                return;

            // if value is not INotifyCollectionChanged and sublevels should not be monitored, don't create monitor for property
            // in other words: monitor only if property value is notifying collection (to track changes in collection) or if we need to monitor sublevels
            if (!(propertyValue is INotifyCollectionChanged) && !propertyMetadata.IsSublevelsMonitored)
                return;

            // create changes monitor for property value
            var propertyMonitor = Create(propertyValue, this, propertyInfo.Name,
                propertyMetadata.MonitorOnlyMarkedProperties, propertyMetadata.IsSublevelsMonitored);

            // if no monitor is created
            if (propertyMonitor == null)
                return;

            // subscribe on property monitor changes notification
            propertyMonitor.Changed += PropertyMonitor_Changed;

            // add monitor to the registry
            _PropertyMonitors.Add(propertyInfo, propertyMonitor);
            _MonitoredProperties.Add(propertyMonitor, propertyInfo);
        }

        /// <summary>
        /// Detaches monitoring from property if attached.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        private void DetachFromProperty(string propertyName)
        {
            foreach (var property in GetTrackedProperties(propertyName))
                DetachFromProperty(property);
        }

        /// <summary>
        /// Detaches monitoring from property if attached.
        /// </summary>
        /// <param name="propertyInfo">Property info.</param>
        private void DetachFromProperty(PropertyInfo propertyInfo)
        {
            // indexed properties are not supported on sublevels
            if (propertyInfo.GetIndexParameters().Length > 0)
                return;

            // detach from property
            if (_PropertyMonitors.TryGetValue(propertyInfo, out var propertyMonitor))
            {
                propertyMonitor.Changed -= PropertyMonitor_Changed;
                propertyMonitor.Dispose();
                _PropertyMonitors.Remove(propertyInfo);
                _MonitoredProperties.Remove(propertyMonitor);
            }
        }

        #endregion

        #region Properties Changes Handling

        /// <summary>
        /// Maintains property changes monitors on property changing.
        /// </summary>
        private void Root_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            // if changed specific property, re-attach and notify
            if (!String.IsNullOrEmpty(args.PropertyName))
            {
                // re-attach to new sublevel if needed
                DetachFromProperty(args.PropertyName);
                AttachToProperty(args.PropertyName);

                // notify on property change if it should be monitored or if this is an unknown property name in greedy monitoring mode
                var trackedProperties = GetTrackedProperties(args.PropertyName);
                if (trackedProperties.Any(IsPropertyMonitored) || (!MonitorOnlyMarkedProperties && !trackedProperties.Any()))
                    RaiseChanged(new MonitoredObjectChangedEventArgs(this, sender, args));
            }
            else
            {
                // if changed all properties of the root object

                // re-attach to all properties
                ClearPropertiesMonitoring();
                SetupPropertiesMonitoring();

                // notify about all properties change only if we monitor all properties in greedy mode or have properties to monitor itself
                if (!MonitorOnlyMarkedProperties || _RootTypeMetadata.PropertiesMetadata.Values.Any(metadata => metadata.IsPropertyMonitored))
                    RaiseChanged(new MonitoredObjectChangedEventArgs(this, sender, args));
            }
        }

        /// <summary>
        /// Redirects change notification from properties monitors to subscribers.
        /// </summary>
        private void PropertyMonitor_Changed(object sender, MonitoredObjectChangedEventArgs args)
        {
            // if notification is from locally created changes monitor for collection property
            if (_MonitoredProperties.TryGetValue(args.Monitor, out var propertyInfo)
                && args.Monitor is CollectionChangesMonitor)
            {
                // notify on collection change only if property should be monitored
                if (IsPropertyMonitored(propertyInfo))
                    RaiseChanged(args);
            }
            else
            {
                // just redirect notification for all non-collection properties
                RaiseChanged(args);
            }
        }

        #endregion
    }
}