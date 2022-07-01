using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EverCodo.ChangesMonitoring.Tests;

/// <summary>
/// Tests for ChangesMonitor.Create(root) which is equivalent of ChangesMonitor.Create(null, root, false, false).
/// </summary>
[TestClass]
public class ChangesMonitor_IdNull_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsFalse_Tests
{
    public ChangesMonitor_IdNull_MonitorOnlyMarkedPropertiesFalse_UseWeakEventsFalse_Tests()
    {
        _Root = NotifyingObject.CreateTreeWith3Layers();

        _ChangesMonitor = ChangesMonitor.Create(_Root);
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

    #region Monitoring suppression

    /// <summary>
    /// Test for changes monitoring suppression.
    /// </summary>
    [TestMethod]
    public void SuppressChangesMonitoring()
    {
        _ChangesMonitor.SuppressNotification();
        Assert.IsTrue(_ChangesMonitor.IsNotificationSuppressed);

        _Root.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _ChangesMonitor.SuppressNotification();
        Assert.IsTrue(_ChangesMonitor.IsNotificationSuppressed);

        _Root.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _ChangesMonitor.ResumeNotification();
        Assert.IsTrue(_ChangesMonitor.IsNotificationSuppressed);

        _Root.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _ChangesMonitor.ResumeNotification();
        Assert.IsFalse(_ChangesMonitor.IsNotificationSuppressed);

        _Root.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root, _Args.ChangedObject);
    }

    #endregion

    #region Ordinary properties

    /// <summary>
    /// Test for top level object property change.
    /// </summary>
    [TestMethod]
    public void ObjectProperty_TopLevelChange()
    {
        _Root.ObjectProperty = new NotifyingObject();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root, _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(PropertyChangedEventArgs));
        Assert.AreEqual(nameof(NotifyingObject.ObjectProperty), ((PropertyChangedEventArgs)_Args.ChangedEventArgs).PropertyName);
        Assert.AreEqual(String.Empty, _Args.Monitor.PropertyPath);

        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root, hierarchyRoots[0]);
    }

    /// <summary>
    /// Test for most nested object property change.
    /// </summary>
    [TestMethod]
    public void ObjectProperty_NestedChange()
    {
        _Root.ObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.ObjectProperty.ObjectProperty, _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(PropertyChangedEventArgs));
        Assert.AreEqual(nameof(NotifyingObject.ObjectProperty), ((PropertyChangedEventArgs)_Args.ChangedEventArgs).PropertyName);
        Assert.AreEqual($".{nameof(NotifyingObject.ObjectProperty)}.{nameof(NotifyingObject.ObjectProperty)}", _Args.Monitor.PropertyPath);
            
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.ObjectProperty.ObjectProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.ObjectProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root.ObjectProperty.ObjectProperty, hierarchyRoots[0]);
        Assert.AreSame(_Root.ObjectProperty, hierarchyRoots[1]);
        Assert.AreSame(_Root, hierarchyRoots[2]);
    }

    /// <summary>
    /// Test for most nested object property change after top level change.
    /// </summary>
    [TestMethod]
    public void ObjectProperty_NestedChangeAfterTopLevelChange()
    {
        _Root.ObjectProperty = new NotifyingObject
        {
            ObjectProperty = new NotifyingObject()
        };

        _Args = null;
        _Root.ObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.ObjectProperty.ObjectProperty, _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(PropertyChangedEventArgs));
        Assert.AreEqual(nameof(NotifyingObject.ObjectProperty), ((PropertyChangedEventArgs)_Args.ChangedEventArgs).PropertyName);
        Assert.AreEqual($".{nameof(NotifyingObject.ObjectProperty)}.{nameof(NotifyingObject.ObjectProperty)}",
            _Args.Monitor.PropertyPath);

        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.ObjectProperty.ObjectProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.ObjectProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root.ObjectProperty.ObjectProperty, hierarchyRoots[0]);
        Assert.AreSame(_Root.ObjectProperty, hierarchyRoots[1]);
        Assert.AreSame(_Root, hierarchyRoots[2]);
    }

    /// <summary>
    /// Test for no changes monitoring inside old value.
    /// </summary>
    [TestMethod]
    public void ObjectProperty_NoMonitoringInsideOldValue()
    {
        var prevObjectPropertyValue = _Root.ObjectProperty;
        _Root.ObjectProperty = new NotifyingObject();

        _Args = null;
        prevObjectPropertyValue.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        prevObjectPropertyValue.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for no changes monitoring after changes monitor disposing.
    /// </summary>
    [TestMethod]
    public void ObjectProperty_NoMonitoringAfterDispose()
    {
        _ChangesMonitor.Dispose();

        _Root.ObjectProperty.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);

        _Root.ObjectProperty = new NotifyingObject();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for most nested collection reset change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChange_Reset()
    {
        _Root.CollectionProperty[0].CollectionProperty.Clear();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreEqual($".{nameof(NotifyingObject.CollectionProperty)}.Item[].{nameof(NotifyingObject.CollectionProperty)}", _Args.Monitor.PropertyPath);

        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0].CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0]));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(ObservableCollection<NotifyingObject>)));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, hierarchyRoots[0]);
        Assert.AreSame(_Root.CollectionProperty[0], hierarchyRoots[1]);
        Assert.AreSame(_Root.CollectionProperty, hierarchyRoots[2]);
        Assert.AreSame(_Root, hierarchyRoots[3]);
    }

    /// <summary>
    /// Test for most nested collection add item change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChange_Add()
    {
        var newItem = new NotifyingObject();
        _Root.CollectionProperty[0].CollectionProperty.Add(newItem);

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Add, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreSame(newItem, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).NewItems[0]);
    }

    /// <summary>
    /// Test for most nested collection remove item change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChange_Remove()
    {
        var nestedCollection = _Root.CollectionProperty[0].CollectionProperty;
        var oldItem = nestedCollection[1];
        nestedCollection.RemoveAt(1);

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreSame(oldItem, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).OldItems[0]);
    }

    /// <summary>
    /// Test for most nested collection replace item change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChange_Replace()
    {
        var nestedCollection = _Root.CollectionProperty[0].CollectionProperty;
        var oldItem = nestedCollection[1];
        var newItem = new NotifyingObject();
        nestedCollection[1] = newItem;

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Replace, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreSame(oldItem, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).OldItems[0]);
        Assert.AreSame(newItem, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).NewItems[0]);
    }

    /// <summary>
    /// Test for most nested collection move item change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChange_Move()
    {
        var nestedCollection = _Root.CollectionProperty[0].CollectionProperty;
        var item = nestedCollection[1];
        nestedCollection.Move(1, 0);

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Move, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreSame(item, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).OldItems[0]);
        Assert.AreSame(item, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).NewItems[0]);
    }

    /// <summary>
    /// Test for most nested collection change after top level collection change.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NestedChangeAfterTopLevelChange()
    {
        _Root.CollectionProperty = new ObservableCollection<NotifyingObject>
        {
            new NotifyingObject
            {
                CollectionProperty = new ObservableCollection<NotifyingObject>
                {
                    new NotifyingObject()
                }
            }
        };

        _Args = null;
        _Root.CollectionProperty[0].CollectionProperty.Clear();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(NotifyCollectionChangedEventArgs));
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, ((NotifyCollectionChangedEventArgs)_Args.ChangedEventArgs).Action);
        Assert.AreEqual($".{nameof(NotifyingObject.CollectionProperty)}.Item[].{nameof(NotifyingObject.CollectionProperty)}", _Args.Monitor.PropertyPath);

        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0].CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0]));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(ObservableCollection<NotifyingObject>)));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, hierarchyRoots[0]);
        Assert.AreSame(_Root.CollectionProperty[0], hierarchyRoots[1]);
        Assert.AreSame(_Root.CollectionProperty, hierarchyRoots[2]);
        Assert.AreSame(_Root, hierarchyRoots[3]);
    }

    /// <summary>
    /// Test for no changes monitoring inside old value.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NoMonitoringInsideOldValue()
    {
        var prevCollectionPropertyValue = _Root.CollectionProperty;
        _Root.CollectionProperty = new ObservableCollection<NotifyingObject>();

        _Args = null;
        prevCollectionPropertyValue[0].CollectionProperty.Clear();
        Assert.IsNull(_Args);

        prevCollectionPropertyValue[0].CollectionProperty = null;
        Assert.IsNull(_Args);

        prevCollectionPropertyValue.Clear();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for no changes monitoring after changes monitor disposing.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_NoMonitoringAfterDispose()
    {
        _ChangesMonitor.Dispose();

        _Args = null;
        _Root.CollectionProperty[0].CollectionProperty.Clear();
        Assert.IsNull(_Args);

        _Root.CollectionProperty[0].CollectionProperty = null;
        Assert.IsNull(_Args);

        _Root.CollectionProperty.Clear();
        Assert.IsNull(_Args);
    }

    /// <summary>
    /// Test for most nested property change inside collection item.
    /// </summary>
    [TestMethod]
    public void CollectionProperty_ObjectProperty_NestedChange()
    {
        _Root.CollectionProperty[0].CollectionProperty[0].ObjectProperty = new NotifyingObject();

        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty[0], _Args.ChangedObject);
        Assert.AreSame(_Args.ChangedObject, _Args.Monitor.Root);
        Assert.IsInstanceOfType(_Args.ChangedEventArgs, typeof(PropertyChangedEventArgs));
        Assert.AreEqual(nameof(NotifyingObject.ObjectProperty), ((PropertyChangedEventArgs)_Args.ChangedEventArgs).PropertyName);
        Assert.AreEqual($".{nameof(NotifyingObject.CollectionProperty)}.Item[].{nameof(NotifyingObject.CollectionProperty)}.Item[]", _Args.Monitor.PropertyPath);

        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0].CollectionProperty[0]));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0].CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty[0]));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root.CollectionProperty));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObject(_Root));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(ObservableCollection<NotifyingObject>)));
        Assert.IsTrue(_Args.Monitor.IsMonitoringInsideObjectOfType(typeof(NotifyingObject)));

        var hierarchyRoots = _Args.Monitor.GetHierarchyRoots().ToList();
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty[0], hierarchyRoots[0]);
        Assert.AreSame(_Root.CollectionProperty[0].CollectionProperty, hierarchyRoots[1]);
        Assert.AreSame(_Root.CollectionProperty[0], hierarchyRoots[2]);
        Assert.AreSame(_Root.CollectionProperty, hierarchyRoots[3]);
        Assert.AreSame(_Root, hierarchyRoots[4]);
    }

    #endregion

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
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty, _Args.ChangedObject);

        _Args = null;
        _Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty.ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId2ObjectProperty.MonitoredId2ObjectProperty, _Args.ChangedObject);
    }

    /// <summary>
    /// Test for monitored collection change.
    /// </summary>
    [TestMethod]
    public void MonitoredId2CollectionProperty_Change()
    {
        _Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty[0].ObjectProperty = new NotifyingObject();
        Assert.IsNotNull(_Args);
        Assert.AreSame(_Root.MonitoredId2CollectionProperty[0].MonitoredId2CollectionProperty[0], _Args.ChangedObject);

        _Args = null;
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