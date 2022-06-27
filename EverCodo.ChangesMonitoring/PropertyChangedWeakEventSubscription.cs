using System;
using System.ComponentModel;
using System.Reflection;

namespace EverCodo.ChangesMonitoring
{
    /// <summary>
    /// Weak subscription handler for INotifyPropertyChanged.PropertyChanged event.
    /// </summary>
    internal class PropertyChangedWeakEventSubscription : IDisposable
    {
        public PropertyChangedWeakEventSubscription(INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _Target = handler.Target != null ? new WeakReference(handler.Target) : null;
            _Method = handler.Method;
            _Source = source;
            _Source.PropertyChanged += Source_PropertyChanged;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _Source.PropertyChanged -= Source_PropertyChanged;
        }

        /// <summary>
        /// Weak reference to event subscriber.
        /// </summary>
        private readonly WeakReference _Target;

        /// <summary>
        /// Descriptor of event handler method.
        /// </summary>
        private readonly MethodInfo _Method;

        /// <summary>
        /// Event source to subscribe to.
        /// </summary>
        private readonly INotifyPropertyChanged _Source;

        /// <summary>
        /// Redirects event from source to target if target is alive.
        /// </summary>
        private void Source_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            object target = null;
            if (_Target != null)
            {
                target = _Target.Target;
                if (target == null)
                {
                    Dispose();
                    return;
                }
            }

            _Method.Invoke(target, new[] { sender, args });
        }
    }
}