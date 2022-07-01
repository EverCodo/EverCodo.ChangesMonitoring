using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Tests for ChangesMonitor.Create(null, root, false, true).
/// </summary>
[TestClass]
public class ChangesMonitor_IdNull_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsTrue_Tests
{
    public ChangesMonitor_IdNull_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsTrue_Tests()
    {
        _Root = NotifyingObject.CreateTreeWith3Layers();

        _ChangesMonitor = ChangesMonitor.Create(_Root, null, false, true);
        _ChangesMonitor.Changed += ChangesMonitor_Changed;
    }

    /// <summary>
    /// Object hierarchy root.
    /// </summary>
    private readonly NotifyingObject _Root;

    /// <summary>
    /// Changes monitor attached to root.
    /// </summary>
    private ChangesMonitor _ChangesMonitor;

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

    #region Weak events

    /// <summary>
    /// Test for automatic changes monitor weak events detach after garbage collection.
    /// </summary>
    [TestMethod]
    public void NoMonitoringAfterGarbageCollection()
    {
        WeakReference GetChangesMonitorWeakReference()
        {
            return new WeakReference(_ChangesMonitor);
        }

        var changesMonitorReference = GetChangesMonitorWeakReference();
        _ChangesMonitor = null;

        Assert.IsTrue(changesMonitorReference.IsAlive);

        _Root.ObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.ObjectProperty.ObjectProperty, _Args.ChangedObject);

        _Args = null;
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        Assert.IsFalse(changesMonitorReference.IsAlive);

        _Root.ObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    #endregion
}