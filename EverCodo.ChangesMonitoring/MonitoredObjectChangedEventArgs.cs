using System;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Event args to notify about monitored object changes.
    /// </summary>
    public class MonitoredObjectChangedEventArgs : EventArgs
    {
        public MonitoredObjectChangedEventArgs(ChangesMonitor monitor, object changedObject, EventArgs changedEventArgs)
        {
            Monitor = monitor;
            ChangedObject = changedObject;
            ChangedEventArgs = changedEventArgs;
        }

        /// <summary>
        /// Direct monitor of changed object.
        /// </summary>
        public ChangesMonitor Monitor { get; }

        /// <summary>
        /// Changed object.
        /// </summary>
        public object ChangedObject { get; }

        /// <summary>
        /// Changed event args (PropertyChangedEventArgs or NotifyCollectionChangedEventArgs).
        /// </summary>
        public EventArgs ChangedEventArgs { get; }
    }
}