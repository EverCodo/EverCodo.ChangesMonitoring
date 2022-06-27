using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Tests for ChangesMonitor.Create("Id2", root, true, false).
/// </summary>
[TestClass]
public class ChangesMonitor_Id2_MonitorOnlyMarkedPropertiesTrue_UseWeakEventsFalse_Tests
{
    public ChangesMonitor_Id2_MonitorOnlyMarkedPropertiesTrue_UseWeakEventsFalse_Tests()
    {
        _Root = NotifyingClass.CreateTreeWith3Layers();

        _ChangesMonitor = ChangesMonitor.Create("Id2", _Root, true, false);
        _ChangesMonitor.Changed += ChangesMonitor_Changed;
    }

    /// <summary>
    /// Object hierarchy root.
    /// </summary>
    private readonly NotifyingClass _Root;

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
        _Root.NotMonitoredObjectProperty.ObjectProperty.ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredObjectProperty.ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.ObjectProperty.NotMonitoredObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.NotMonitoredObjectProperty.CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.NotMonitoredObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for not monitored collection change.
    /// </summary>
    [TestMethod]
    public void NotMonitoredCollectionProperty_Change()
    {
        _Root.NotMonitoredCollectionProperty[0].CollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.NotMonitoredCollectionProperty[0].CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.CollectionProperty[0].NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.CollectionProperty[0].NotMonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.NotMonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.NotMonitoredCollectionProperty[0].ObjectProperty = new NotifyingClass();
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
        _Root.MonitoredObjectProperty.MonitoredObjectProperty.MonitoredObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredCollectionProperty_Change()
    {
        _Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredCollectionProperty[0].MonitoredCollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id1")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1ObjectProperty_Change()
    {
        _Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredId1ObjectProperty.MonitoredId1ObjectProperty.ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1CollectionProperty_Change()
    {
        _Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredId1CollectionProperty[0].MonitoredId1CollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    #endregion

    #region Monitored properties (monitor identifier is "Id2")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId2ObjectProperty_Change()
    {
        _Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty = new NotifyingClass();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty.ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId2CollectionProperty_Change()
    {
        _Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty, _Args.ChangedObject);
    }

    #endregion

    #region Monitored properties (monitor identifiers are "1" and "2")

    /// <summary>
    /// Test for monitored object property change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1AndId2ObjectProperty_Change()
    {
        _Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty.ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty = new NotifyingClass();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2ObjectProperty.MonitoredId1AndId2ObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId1AndId2CollectionProperty_Change()
    {
        _Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty[0].ObjectProperty = new NotifyingClass();
        Assert.IsNull(_Args);

        _Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty.Clear();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId1AndId2CollectionProperty[0].MonitoredId1AndId2CollectionProperty, _Args.ChangedObject);
    }

    #endregion
}