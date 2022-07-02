[![NuGet Status](https://img.shields.io/nuget/v/EverCodo.ChangesMonitoring.svg)](https://www.nuget.org/packages/EverCodo.ChangesMonitoring/)

# EverCodo.ChangesMonitoring
A powerful framework to handle PropertyChanged and CollectionChanged events on arbitrary hierarchy of nested objects and collections.

## Basic usage example

### Sample data model
```csharp
// INotifyPropertyChanged implementation is omitted to keep the example simple

public class Entity : INotifyPropertyChanged
{
    public string Name { get; set; }
    public DateTime CreationTime { get; set; }
    public Metadata Metadata { get; set; }    
}

public class Metadata : INotifyPropertyChanged
{
    public string Description { get; set; }
    public ObservableCollection<string> Tags { get; set; }
}

public class File : Entity
{
    public int Size { get; set; }
}

public class Folder : Entity
{
    public ObservableCollection<Entity> Children { get; set; }
}
```

### Subscription for changes
```csharp
public class Tree
{
    public Tree()
    {
        // ... loading tree to the Root is omitted ...

        _ChangesMonitor = ChangesMonitor.Create(Root);
        _ChangesMonitor.Changed += ChangesMonitor_Changed;
    }

    public Folder Root { get; }
       
    public string IsDirty { get; private set; }
    
    private readonly ChangesMonitor _ChangesMonitor;
    
    private void ChangesMonitor_Changed(object sender, MonitoredObjectChangedEventArgs args)
    {
        // any change of any property or collection on any level inside the Root makes the tree dirty
        IsDirty = true;
    }
    
    public void Save()
    {
        // ... saving tree from the Root is omitted ...
        
        IsDirty = false;
    }
}
```

### Changes monitoring
```csharp
[TestClass]
public class TreeChangeTests
{
    [TestMethod]
    public void AddMetadataTag()
    {
        var tree = new Tree();
        Assert.IsFalse(tree.IsDirty);
        
        tree.Root.Children[5].Children[3].Children[1].Metadata.Tags.Add("Some tag");
        Assert.IsTrue(tree.IsDirty);
        
        tree.Save();
        Assert.IsFalse(tree.IsDirty);
    }
    
    [TestMethod]
    public void ReplaceMetadata()
    {
        var tree = new Tree();
        Assert.IsFalse(tree.IsDirty);
        
        tree.Root.Children[5].Children[3].Metadata = new Metadata();
        Assert.IsTrue(tree.IsDirty);
        
        tree.Save();
        Assert.IsFalse(tree.IsDirty);
    }
    
    [TestMethod]
    public void ChangeMetadataDescription()
    {
        var tree = new Tree();
        Assert.IsFalse(tree.IsDirty);
        
        tree.Root.Children[5].Children[3].Metadata.Description = "Some description";
        Assert.IsTrue(tree.IsDirty);
        
        tree.Save();
        Assert.IsFalse(tree.IsDirty);
    }
    
    [TestMethod]
    public void ChangeEntityName()
    {
        var tree = new Tree();
        Assert.IsFalse(tree.IsDirty);
        
        tree.Root.Children[5].Name = "Some name";
        Assert.IsTrue(tree.IsDirty);
        
        tree.Save();
        Assert.IsFalse(tree.IsDirty);
    }
    
    [TestMethod]
    public void ReplaceChildren()
    {
        var tree = new Tree();
        Assert.IsFalse(tree.IsDirty);
        
        tree.Root.Children[5].Children = new ObservableCollection<Entity>();
        Assert.IsTrue(tree.IsDirty);
        
        tree.Save();
        Assert.IsFalse(tree.IsDirty);
    }
}
```

## Advanced features

### Monitor creation parameters
Use one of the `ChangesMonitor.Create()` methods on the root object to create changes monitor for the whole object tree including all nested sublevels. To be monitorable the root object and every nested object should implement `INotifyPropertyChanged` or `INotifyCollectionChanged`. If an object implements both interfaces it will be monitored as a collection.

```csharp
public class ChangesMonitor
{
    public static ChangesMonitor Create(object root) { /*...*/ }
    public static ChangesMonitor Create(object root, string id, bool monitorOnlyMarkedProperties, bool useWeakEvents) { /*...*/ }
}
```

#### Parameters
|Parameter|Description|
|---|---|
|`root`|Root object to attach monitoring to.|
|`id`|Identifier of the changes monitor to associate it with `[MonitorChanges]` attributes.|
|`monitorOnlyMarkedProperties`|Specifies if only properties marked with `[MonitorChanges]` attribute with specific `Id` should be monitored.|
|`useWeakEvents`|Specifies if changes monitor will use weak events when subscribes to monitored objects.|

### \[MonitorChanges\] attribute
Use `[MonitorChanges]` attribute on object properties to control how changes of those properties should be monitored.

#### Parameters
|Parameter|Description|
|---|---|
|`Id`|Identifier of the associated changes monitor. Default is `null`.|
|`DoNotMonitor`|Specifies if changes should not be monitored either for the property itself or for nested objects. Default is `false`.|
|`MonitorProperty`|Specifies if changes of the property iteself should be monitored. Default is `true`.|
|`MonitorSublevels`|Specifies if changes of nested objects should be monitored. Default is `true`.|
|`MonitorOnlyMarkedProperties`|Specifies if only properties marked with `[MonitorChanges]` attribue should be monitored on sublevels. This parameter can be used to change strictness of monitoring behavior for specific property sublevels.|

### Property monitoring behavior variety
Several changes monitors to illustrate the idea:
```csharp
// changes monitor without id specified, configured to subscribe for changes on all properties (greedy mode)
var idNull_Greedy = ChangesMonitor.Create(id: null, monitorOnlyMarkedProperties: false);

// changes monitor with Id1, configured to subscribe for changes on all properties (greedy mode)
var id1_Greedy = ChangesMonitor.Create(id: "Id1", monitorOnlyMarkedProperties: false);

// changes monitor with Id2, configured to subscribe for changes on all properties (greedy mode)
var id2_Greedy = ChangesMonitor.Create(id: "Id2", monitorOnlyMarkedProperties: false);

// changes monitor without id specified, configured to subscribe for changes on properties marked with [MonitorChanges] attribute (strict mode)
var idNull_Strict = ChangesMonitor.Create(id: null, monitorOnlyMarkedProperties: true);

// changes monitor with Id1, configured to subscribe for changes on properties marked with [MonitorChanges] attribute (strict mode)
var id1_Strict = ChangesMonitor.Create(id: "Id1", monitorOnlyMarkedProperties: true);

// changes monitor with Id2, configured to subscribe for changes on properties marked with [MonitorChanges] attribute (strict mode)
var id2_Strict = ChangesMonitor.Create(id: "Id2", monitorOnlyMarkedProperties: true);
```

Monitored object with differently marked properties:
```csharp
// INotifyPropertyChanged implementation is omitted to keep the example simple
public class NotifyingObject : INotifyPropertyChanges
{
    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself and its value's properties
    // idNull_Strict, id1_Strict, id2_Strict : don't monitor because there is no [MonitorChanges] attribute on the property
    public NotifyingObject ObjectProperty { get; set; }   
    
    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself, collection and any collection item's properties
    // idNull_Strict, id1_Strict, id2_Strict : don't monitor because there is no [MonitorChanges] attribute on the property
    public ObservableCollection<NotifyingObject> CollectionProperty { get; set; }
    
    // idNull_Greedy, id1_Greedy, id2_Greedy : don't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // idNull_Strict, id1_Strict, id2_Strict : don't monitor because MonitorChangesAttribute.DoNotMonitor == true
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingObject NotMonitoredObjectProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : don't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // idNull_Strict, id1_Strict, id2_Strict : don't monitor because MonitorChangesAttribute.DoNotMonitor == true
    [MonitorChanges(DoNotMonitor = true)]
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingObject> NotMonitoredCollectionProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself and its value's properties
    // idNull_Strict : monitors property itself and its value's properties marked with suitable [MonitorChanges] attribute
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges]
    public NotifyingObject MonitoredObjectProperty { get; set; }   
    
    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself, collection and any collection item's properties
    // idNull_Strict : monitors property itself, collection and any collection item's properties marked with suitable [MonitorChanges] attribute
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges]
    public ObservableCollection<NotifyingObject> MonitoredCollectionProperty { get; set; }
    
    // idNull_Greedy : monitors property itself and its value's properties
    // id1_Greedy : monitors property itself and its value's properties
    // id2_Greedy : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict : monitors property itself and its value's properties regardless of [MonitorChanges] attribute
    // id2_Strict : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public NotifyingObject MonitoredId1ObjectProperty { get; set; }

    // idNull_Greedy : monitors property itself, collection and any collection item's properties
    // id1_Greedy : monitors property itself, collection and any collection item's properties
    // id2_Greedy : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict : monitors property itself, collection and any collection item's properties regardless of [MonitorChanges] attribute
    // id2_Strict : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    [MonitorChanges(Id = "Id1", MonitorOnlyMarkedProperties = false)]
    [MonitorChanges(Id = "Id2", DoNotMonitor = true)]
    public ObservableCollection<NotifyingObject> MonitoredId1CollectionProperty { get; set; }
    
    // idNull_Greedy : monitors property itself and its value's properties
    // id1_Greedy : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // id2_Greedy : monitors property itself and its value's properties marked with suitable [MonitorChanges] attribute
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // id2_Strict : monitors property itself and its value's properties marked with suitable [MonitorChanges] attribute
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public NotifyingObject MonitoredId2ObjectProperty { get; set; }

    // idNull_Greedy : monitors property itself, collection and any collection item's properties
    // id1_Greedy : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // id2_Greedy : monitors property itself, collection and any collection item's properties marked with suitable [MonitorChanges] attribute
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict : doesn't monitor because MonitorChangesAttribute.DoNotMonitor == true
    // id2_Strict : monitors property itself, collection and any collection item's propertiess marked with suitable [MonitorChanges] attribute
    [MonitorChanges(Id = "Id1", DoNotMonitor = true)]
    [MonitorChanges(Id = "Id2", MonitorOnlyMarkedProperties = true)]
    public ObservableCollection<NotifyingObject> MonitoredId2CollectionProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself and its value's properties
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict, id2_Strict : monitor property itself and its value's properties marked with suitable [MonitorChanges] attribute
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public NotifyingObject MonitoredId1AndId2ObjectProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself, collection and any collection item's properties
    // idNull_Strict : doesn't monitor because no suitable [MonitorChanges] attribute with Id == null
    // id1_Strict, id2_Strict : monitor property itself, collection and any collection item's properties marked with suitable [MonitorChanges] attribute
    [MonitorChanges(Id = "Id1")]
    [MonitorChanges(Id = "Id2")]
    public ObservableCollection<NotifyingObject> MonitoredId1AndId2CollectionProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself, do NOT monitor its value's properties
    // idNull_Strict : monitors property itself, does NOT monitor its value's properties
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges(MonitorSublevels = false)]
    public NotifyingObject MonitoredWithoutSublevelsObjectProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : monitor property itself and collection, do NOT monitor collection item's properties
    // idNull_Strict : monitors property itself and collection, does NOT monitor collection item's properties
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges(MonitorSublevels = false)]
    public ObservableCollection<NotifyingObject> MonitoredWithoutSublevelsCollectionProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : do NOT monitor property itself, do monitor its value's properties
    // idNull_Strict : does NOT monitor property itself, does monitor its value's properties marked with suitable [MonitorChanges] attribute
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public NotifyingObject MonitoredSublevelsOnlyObjectProperty { get; set; }

    // idNull_Greedy, id1_Greedy, id2_Greedy : do NOT monitor property itself and collection, does monitor collection item's properties
    // idNull_Strict : does NOT monitor property itself and collection, does monitor collection item's properties marked with suitable [MonitorChanges] attribute
    // id1_Strict, id2_Strict : don't monitor because no suitable [MonitorChanges] attribute with Id == "Id1" or "Id2"
    [MonitorChanges(MonitorProperty = false, MonitorSublevels = true)]
    public ObservableCollection<NotifyingObject> MonitoredSublevelsOnlyCollectionProperty { get; set; }
}
```

### Complex changes handler example
```csharp
public class SomeClass
{
    // ... 
    
    private void ChangesMonitor_Changed(object sender, MonitoredObjectChangedEventArgs args)
    {
        var changedObjectPath = args.Monitor.PropertyPath;

        // handle only changes inside subtree specified with the following property path and skip everything else
        if (!changedObjectPath.StartsWith(".Level1PropertyName.Item[].Level2PropertyName"))
            return;

        switch (args.ChangedEventArgs)
        {
            // changed property of the object
            case PropertyChangedEventArgs propertyChangedArgs:
                var changedPropertyPath = $"{changedObjectPath}.{propertyChangedArgs.PropertyName}";
                
                switch (args.ChangedObject)
                {
                    // handles changes in object of NotifyingObjectA type
                    case NotifyingObjectA notifyingObjectA:
                        switch (propertyChangedArgs.PropertyName)
                        {
                            case "Property1":
                                // handle notifyingObjectA.Property1 changes at path changedPropertyPath
                                break;

                            case "Property2":
                                // handle notifyingObjectA.Property2 changes at path changedPropertyPath
                                break;
                        }
                        break;

                    // handles changes in object of NotifyingObjectB type
                    case NotifyingObjectB notifyingObjectB:
                        switch (propertyChangedArgs.PropertyName)
                        {
                            case "PropertyA":
                                // handle notifyingObjectB.PropertyA changes at path changedPropertyPath
                                break;

                            case "PropertyB":
                                // handle notifyingObjectB.PropertyB changes at path changedPropertyPath
                                break;
                        }
                        break;
                }

                break;

            // changed collection
            case NotifyCollectionChangedEventArgs collectionChangedArgs:
                switch (args.ChangedObject)
                {
                    // handles changes in collection of NotifyingObjectA items
                    case ObservableCollection<NotifyingObjectA> notifyingCollectionA:
                        switch (collectionChangedArgs.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                // handle addition to notifyingCollectionA at path changedObjectPropertyPath
                                break;
                            
                            case NotifyCollectionChangedAction.Remove:
                                // handle removing from notifyingCollectionA at path changedObjectPropertyPath
                                break;
                            
                            case NotifyCollectionChangedAction.Replace:
                                // handle replacing in notifyingCollectionA at path changedObjectPropertyPath
                                break;
                            
                            case NotifyCollectionChangedAction.Move:
                                // handle moving in notifyingCollectionA at path changedObjectPropertyPath
                                break;
                            
                            case NotifyCollectionChangedAction.Reset:
                                // handle resetting of notifyingCollectionA at path changedObjectPropertyPath
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;

                    // handles changes in collection of NotifyingObjectB items
                    case ObservableCollection<NotifyingObjectB> notifyingCollectionB:
                        switch (collectionChangedArgs.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                // handle addition to notifyingCollectionB at path changedObjectPropertyPath
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                // handle removing from notifyingCollectionB at path changedObjectPropertyPath
                                break;

                            case NotifyCollectionChangedAction.Replace:
                                // handle replacing in notifyingCollectionB at path changedObjectPropertyPath
                                break;

                            case NotifyCollectionChangedAction.Move:
                                // handle moving in notifyingCollectionB at path changedObjectPropertyPath
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                // handle resetting of notifyingCollectionB at path changedObjectPropertyPath
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                }
                break;

            // unexpected
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    // ... 
}
```
