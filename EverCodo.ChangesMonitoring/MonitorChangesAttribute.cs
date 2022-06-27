using System;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Attribute to specify changes monitoring options for property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class MonitorChangesAttribute : Attribute
    {
        public MonitorChangesAttribute()
        {
            MonitorProperty = true;
            MonitorSublevels = true;
        }

        /// <summary>
        /// Id of monitor this attribute is related to.
        /// null means default and applies to all monitors without specified id and to all monitors with id if no specific attribute for that id is present.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Switches off all monitoring if value is true.
        /// </summary>
        public bool DoNotMonitor
        {
            get => !MonitorProperty && !MonitorSublevels;
            set
            {
                MonitorProperty = !value;
                MonitorSublevels = !value;
            }
        }

        /// <summary>
        /// If false target property changes will not be monitored by ChangesMonitor (but sublevels may be monitored if MonitorSublevels == true).
        /// Default is true.
        /// </summary>
        public bool MonitorProperty { get; set; }

        /// <summary>
        /// If false properties of target property object value will not be monitored.
        /// Default is true.
        /// </summary>
        public bool MonitorSublevels { get; set; }

        /// <summary>
        /// Specifies if MonitorOnlyMarkedProperties is initialized.
        /// </summary>
        /// <remarks>false means "not specified": the flag value will be acquired from parent changes monitor.</remarks>
        public bool IsMonitorOnlyMarkedPropertiesInitialized { get; private set; }

        /// <summary>
        /// Specifies if only marked properties should be monitored on sublevels.
        /// </summary>
        private bool _MonitorOnlyMarkedProperties;

        /// <summary>
        /// Specifies if only marked properties should be monitored on sublevels.
        /// </summary>
        public bool MonitorOnlyMarkedProperties
        {
            get => _MonitorOnlyMarkedProperties;
            set
            {
                _MonitorOnlyMarkedProperties = value;
                IsMonitorOnlyMarkedPropertiesInitialized = true;
            }
        }
    }
}