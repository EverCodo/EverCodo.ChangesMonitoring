using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Sample notifying class to test changes monitoring.
/// </summary>
internal class NotifyingObject : INotifyPropertyChanged
{
    /// <summary>
    /// Creates objects subtree with 3 layers of NotifyingObject objects.
    /// </summary>
    /// <returns>NotifyingObject objects subtree.</returns>
    public static NotifyingObject CreateTreeWith3Layers()
    {
        var notifyingClass = new NotifyingObject
        {
            ObjectProperty = CreateTreeWith2Layers(),
            CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            NotMonitoredObjectProperty = CreateTreeWith2Layers(),
            NotMonitoredCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredObjectProperty = CreateTreeWith2Layers(),
            MonitoredCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId1ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId1CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId2ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId2CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId1AndId2ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId1AndId2CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredWithoutSublevelsObjectProperty = CreateTreeWith2Layers(),
            MonitoredWithoutSublevelsCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredSublevelsOnlyObjectProperty = CreateTreeWith2Layers(),
            MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },
        };

        return notifyingClass;
    }

    /// <summary>
    /// Creates objects subtree with 2 layers of NotifyingObject objects.
    /// </summary>
    /// <returns>NotifyingObject objects subtree.</returns>
    public static NotifyingObject CreateTreeWith2Layers()
    {
        var notifyingClass = new NotifyingObject
        {
            ObjectProperty = new NotifyingObject(),
            CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },
            
            NotMonitoredObjectProperty = new NotifyingObject(),
            NotMonitoredCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredObjectProperty = new NotifyingObject(),
            MonitoredCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredId1ObjectProperty = new NotifyingObject(),
            MonitoredId1CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredId2ObjectProperty = new NotifyingObject(),
            MonitoredId2CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredId1AndId2ObjectProperty = new NotifyingObject(),
            MonitoredId1AndId2CollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredWithoutSublevelsObjectProperty = new NotifyingObject(),
            MonitoredWithoutSublevelsCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },

            MonitoredSublevelsOnlyObjectProperty = new NotifyingObject(),
            MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingObject>
            {
                new NotifyingObject(),
                new NotifyingObject(),
                new NotifyingObject(),
            },
        };

        return notifyingClass;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">Name of changed property.</param>
    protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region Ordinary properties

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    public NotifyingObject ObjectProperty
    {
        get => _ObjectProperty;
        set
        {
            if (_ObjectProperty == value)
                return;

            _ObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    public ObservableCollection<NotifyingObject> CollectionProperty
    {
        get => _CollectionProperty;
        set
        {
            if (_CollectionProperty == value)
                return;

            _CollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Not monitored properties

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _NotMonitoredObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingObject NotMonitoredObjectProperty
    {
        get => _NotMonitoredObjectProperty;
        set
        {
            if (_NotMonitoredObjectProperty == value)
                return;

            _NotMonitoredObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _NotMonitoredCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingObject> NotMonitoredCollectionProperty
    {
        get => _NotMonitoredCollectionProperty;
        set
        {
            if (_NotMonitoredCollectionProperty == value)
                return;

            _NotMonitoredCollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (monitor identifier is not specified)

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges]
    public NotifyingObject MonitoredObjectProperty
    {
        get => _MonitoredObjectProperty;
        set
        {
            if (_MonitoredObjectProperty == value)
                return;

            _MonitoredObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges]
    public ObservableCollection<NotifyingObject> MonitoredCollectionProperty
    {
        get => _MonitoredCollectionProperty;
        set
        {
            if (_MonitoredCollectionProperty == value)
                return;

            _MonitoredCollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id1")

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredId1ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingObject MonitoredId1ObjectProperty
    {
        get => _MonitoredId1ObjectProperty;
        set
        {
            if (_MonitoredId1ObjectProperty == value)
                return;

            _MonitoredId1ObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredId1CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingObject> MonitoredId1CollectionProperty
    {
        get => _MonitoredId1CollectionProperty;
        set
        {
            if (_MonitoredId1CollectionProperty == value)
                return;

            _MonitoredId1CollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id2")

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredId2ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public NotifyingObject MonitoredId2ObjectProperty
    {
        get => _MonitoredId2ObjectProperty;
        set
        {
            if (_MonitoredId2ObjectProperty == value)
                return;

            _MonitoredId2ObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredId2CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public ObservableCollection<NotifyingObject> MonitoredId2CollectionProperty
    {
        get => _MonitoredId2CollectionProperty;
        set
        {
            if (_MonitoredId2CollectionProperty == value)
                return;

            _MonitoredId2CollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (monitor identifiers are "1" and "2")

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredId1AndId2ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public NotifyingObject MonitoredId1AndId2ObjectProperty
    {
        get => _MonitoredId1AndId2ObjectProperty;
        set
        {
            if (_MonitoredId1AndId2ObjectProperty == value)
                return;

            _MonitoredId1AndId2ObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredId1AndId2CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public ObservableCollection<NotifyingObject> MonitoredId1AndId2CollectionProperty
    {
        get => _MonitoredId1AndId2CollectionProperty;
        set
        {
            if (_MonitoredId1AndId2CollectionProperty == value)
                return;

            _MonitoredId1AndId2CollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (without sublevels)

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredWithoutSublevelsObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(MonitorSublevels = false)]
    public NotifyingObject MonitoredWithoutSublevelsObjectProperty
    {
        get => _MonitoredWithoutSublevelsObjectProperty;
        set
        {
            if (_MonitoredWithoutSublevelsObjectProperty == value)
                return;

            _MonitoredWithoutSublevelsObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredWithoutSublevelsCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(MonitorSublevels = false)]
    public ObservableCollection<NotifyingObject> MonitoredWithoutSublevelsCollectionProperty
    {
        get => _MonitoredWithoutSublevelsCollectionProperty;
        set
        {
            if (_MonitoredWithoutSublevelsCollectionProperty == value)
                return;

            _MonitoredWithoutSublevelsCollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion

    #region Monitored properties (sublevels only)

    /// <summary>
    /// Object property backing field.
    /// </summary>
    private NotifyingObject _MonitoredSublevelsOnlyObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public NotifyingObject MonitoredSublevelsOnlyObjectProperty
    {
        get => _MonitoredSublevelsOnlyObjectProperty;
        set
        {
            if (_MonitoredSublevelsOnlyObjectProperty == value)
                return;

            _MonitoredSublevelsOnlyObjectProperty = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Collection property backing field.
    /// </summary>
    private ObservableCollection<NotifyingObject> _MonitoredSublevelsOnlyCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public ObservableCollection<NotifyingObject> MonitoredSublevelsOnlyCollectionProperty
    {
        get => _MonitoredSublevelsOnlyCollectionProperty;
        set
        {
            if (_MonitoredSublevelsOnlyCollectionProperty == value)
                return;

            _MonitoredSublevelsOnlyCollectionProperty = value;
            RaisePropertyChanged();
        }
    }

    #endregion
}