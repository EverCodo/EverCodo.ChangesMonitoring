using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Sample notifying class to test changes monitoring.
/// </summary>
internal class NotifyingClass : INotifyPropertyChanged
{
    /// <summary>
    /// Creates objects subtree with 3 layers of NotifyingClass objects.
    /// </summary>
    /// <returns>NotifyingClass objects subtree.</returns>
    public static NotifyingClass CreateTreeWith3Layers()
    {
        var notifyingClass = new NotifyingClass
        {
            ObjectProperty = CreateTreeWith2Layers(),
            CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            NotMonitoredObjectProperty = CreateTreeWith2Layers(),
            NotMonitoredCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredObjectProperty = CreateTreeWith2Layers(),
            MonitoredCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId1ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId1CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId2ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId2CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredId1AndId2ObjectProperty = CreateTreeWith2Layers(),
            MonitoredId1AndId2CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredWithoutSublevelsObjectProperty = CreateTreeWith2Layers(),
            MonitoredWithoutSublevelsCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },

            MonitoredSublevelsOnlyObjectProperty = CreateTreeWith2Layers(),
            MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
                CreateTreeWith2Layers(),
            },
        };

        return notifyingClass;
    }

    /// <summary>
    /// Creates objects subtree with 2 layers of NotifyingClass objects.
    /// </summary>
    /// <returns>NotifyingClass objects subtree.</returns>
    public static NotifyingClass CreateTreeWith2Layers()
    {
        var notifyingClass = new NotifyingClass
        {
            ObjectProperty = new NotifyingClass(),
            CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },
            
            NotMonitoredObjectProperty = new NotifyingClass(),
            NotMonitoredCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredObjectProperty = new NotifyingClass(),
            MonitoredCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredId1ObjectProperty = new NotifyingClass(),
            MonitoredId1CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredId2ObjectProperty = new NotifyingClass(),
            MonitoredId2CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredId1AndId2ObjectProperty = new NotifyingClass(),
            MonitoredId1AndId2CollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredWithoutSublevelsObjectProperty = new NotifyingClass(),
            MonitoredWithoutSublevelsCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
            },

            MonitoredSublevelsOnlyObjectProperty = new NotifyingClass(),
            MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingClass>
            {
                new NotifyingClass(),
                new NotifyingClass(),
                new NotifyingClass(),
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
    private NotifyingClass _ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    public NotifyingClass ObjectProperty
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
    private ObservableCollection<NotifyingClass> _CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    public ObservableCollection<NotifyingClass> CollectionProperty
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
    private NotifyingClass _NotMonitoredObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingClass NotMonitoredObjectProperty
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
    private ObservableCollection<NotifyingClass> _NotMonitoredCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingClass> NotMonitoredCollectionProperty
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
    private NotifyingClass _MonitoredObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges]
    public NotifyingClass MonitoredObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges]
    public ObservableCollection<NotifyingClass> MonitoredCollectionProperty
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
    private NotifyingClass _MonitoredId1ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingClass MonitoredId1ObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredId1CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingClass> MonitoredId1CollectionProperty
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
    private NotifyingClass _MonitoredId2ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public NotifyingClass MonitoredId2ObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredId2CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public ObservableCollection<NotifyingClass> MonitoredId2CollectionProperty
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
    private NotifyingClass _MonitoredId1AndId2ObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public NotifyingClass MonitoredId1AndId2ObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredId1AndId2CollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public ObservableCollection<NotifyingClass> MonitoredId1AndId2CollectionProperty
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
    private NotifyingClass _MonitoredWithoutSublevelsObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(MonitorSublevels = false)]
    public NotifyingClass MonitoredWithoutSublevelsObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredWithoutSublevelsCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(MonitorSublevels = false)]
    public ObservableCollection<NotifyingClass> MonitoredWithoutSublevelsCollectionProperty
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
    private NotifyingClass _MonitoredSublevelsOnlyObjectProperty;

    /// <summary>
    /// Object property.
    /// </summary>
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public NotifyingClass MonitoredSublevelsOnlyObjectProperty
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
    private ObservableCollection<NotifyingClass> _MonitoredSublevelsOnlyCollectionProperty;

    /// <summary>
    /// Collection property.
    /// </summary>
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public ObservableCollection<NotifyingClass> MonitoredSublevelsOnlyCollectionProperty
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