using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Tests for ChangesMonitor.Create("Id1", root, false, false).
/// </summary>
[TestClass]
public class ChangesMonitor_Id1_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsFalse_Tests
{
    public ChangesMonitor_Id1_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsFalse_Tests()
    {
        _Root = NotifyingObject.CreateTreeWith3Layers();

        _ChangesMonitor = ChangesMonitor.Create(_Root, "Id1", false, false);
        _ChangesMonitor.Changed += ChangesMonitor_Changed;
    }

    /// <summary>
    /// Object hierarchy root.
    /// </summary>
    private readonly NotifyingObject _Root;

    /// <summary>
    /// Changes monitor attached to root.
    /// </summary>
    private readonly ChangesMonitor _ChangesMonitor;

    /// <summary>
    /// Handles Changed event from changes monitor.
    /// </summary>
    private void ChangesMonitor_Changed(object sender, MonitoredObjectChangedEventArgs args)
    {
        _Args = args;
    }

    /// <summary>
    /// Last changed event args.
    /// </summary>
    private MonitoredObjectChangedEventArgs _Args;

    #region Not monitored properties

    /// <summary>
    /// Test for not monitored property change.
    /// </summary>
    [TestMethod]
    public void NotMonitoredObjectProperty_Change()
    {
        _Root.NotMonitoredObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.ObjectProperty.NotMonitoredObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.NotMonitoredObjectProperty.CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.NotMonitoredObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for not monitored collection change.
    /// </summary>
    [TestMethod]
    public void NotMonitoredCollectionProperty_Change()
    {
        _Root.NotMonitoredCollectionProperty[0].CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.NotMonitoredCollectionProperty[0].CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.CollectionProperty[0].NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.CollectionProperty[0].NotMonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.NotMonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    #endregion

    #region Monitored properties (monitor identifier is not specified)

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredObjectProperty_Change()
    {
        _Root.MonitoredObjectProperty.MonitoredObjectProperty.MonitoredObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredObjectProperty.MonitoredObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredCollectionProperty_Change()
    {
        _Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty[0], _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty, _Args.ChangedObject);
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id1")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1ObjectProperty_Change()
    {
        _Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1CollectionProperty_Change()
    {
        _Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty[0], _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty, _Args.ChangedObject);
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id2")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId2ObjectProperty_Change()
    {
        _Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId2CollectionProperty_Change()
    {
        _Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    #endregion

    #region Monitored properties (monitor identifiers are "1" and "2")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1AndId2ObjectProperty_Change()
    {
        _Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1AndId2CollectionProperty_Change()
    {
        _Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty[0], _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty, _Args.ChangedObject);
    }

    #endregion

    #region Monitored properties (without sublevels)

    /// <summary>
    /// Test for object property change (without sublevels).
    /// </summary>
    [TestMethod]
    public void MonitoredWithoutSublevelsObjectProperty_Change()
    {
        _Root.MonitoredWithoutSublevelsObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredWithoutSublevelsObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredWithoutSublevelsObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for collection change (without sublevels).
    /// </summary>
    [TestMethod]
    public void MonitoredWithoutSublevelsCollectionProperty_Change()
    {
        _Root.MonitoredWithoutSublevelsCollectionProperty[0].CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredWithoutSublevelsCollectionProperty[0].CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.MonitoredWithoutSublevelsCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredWithoutSublevelsCollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredWithoutSublevelsCollectionProperty, _Args.ChangedObject);
    }

    #endregion

    #region Monitored properties (sublevels only)

    /// <summary>
    /// Test for object property change (sublevels only).
    /// </summary>
    [TestMethod]
    public void MonitoredSublevelsOnlyObjectProperty_Change()
    {
        _Root.MonitoredSublevelsOnlyObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredSublevelsOnlyObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredSublevelsOnlyObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for most nested object property change after top level change (sublevels only).
    /// </summary>
    [TestMethod]
    public void MonitoredSublevelsOnlyObjectProperty_NestedChangeAfterTopLevelChange()
    {
        _Root.MonitoredSublevelsOnlyObjectProperty = new NotifyingObject
        {
            MonitoredSublevelsOnlyObjectProperty = new NotifyingObject()
        };

        _Args = null;
        _Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredSublevelsOnlyObjectProperty.MonitoredSublevelsOnlyObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for collection change (sublevels only).
    /// </summary>
    [TestMethod]
    public void MonitoredSublevelsOnlyCollectionProperty_Change()
    {
        _Root.MonitoredSublevelsOnlyCollectionProperty[0].MonitoredSublevelsOnlyCollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.MonitoredSublevelsOnlyCollectionProperty[0].MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingObject>();
        Assert.IsNull(_Args);

        _Root.MonitoredSublevelsOnlyCollectionProperty[0].CollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredSublevelsOnlyCollectionProperty[0].CollectionProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredSublevelsOnlyCollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for nested collection change after top level collection change (sublevels only).
    /// </summary>
    [TestMethod]
    public void MonitoredSublevelsOnlyCollectionProperty_NestedChangeAfterTopLevelChange()
    {
        _Root.MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingObject>
        {
            new NotifyingObject
            {
                MonitoredSublevelsOnlyCollectionProperty = new ObservableCollection<NotifyingObject>
                {
                    new NotifyingObject()
                }
            }
        };

        _Args = null;
        _Root.MonitoredSublevelsOnlyCollectionProperty[0].MonitoredSublevelsOnlyCollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredSublevelsOnlyCollectionProperty[0].MonitoredSublevelsOnlyCollectionProperty[0], _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredSublevelsOnlyCollectionProperty[0].MonitoredSublevelsOnlyCollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    #endregion
}