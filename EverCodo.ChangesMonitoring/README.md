[![NuGet Status](https://img.shields.io/nuget/v/EverCodo.ChangesMonitoring.svg)](https://www.nuget.org/packages/EverCodo.ChangesMonitoring/)

# EverCodo.ChangesMonitoring
A powerful framework to handle PropertyChanged and CollectionChanged events on arbitrary hierarchy of nested objects and collections.


## Sample data model
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

public class File : EntityInfo
{
    public int Size { get; set; }
}

public class Folder : EntityInfo
{
    public ObservableCollection<Entity> Children { get; set; }
}
```

## Subscription for changes
```csharp
public class Tree
{
    public Tree()
    {
        // ... loading tree to the Root is omitted ...

        _ChangesMonitor = ChangesMonitor.Create(Root);
        _ChangesMonitor.Changed += ChangesMonitor_Changed;
    }

    public FolderInfo Root { get; }
       
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

## Changes monitoring
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
