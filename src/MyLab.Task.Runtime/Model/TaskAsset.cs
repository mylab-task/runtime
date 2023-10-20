namespace MyLab.Task.Runtime;

class TaskAsset
{
    public string Name { get; }

    public string LibPath { get; }

    public TaskAsset(string name, string libPath) => 
        (Name, LibPath) = (name, libPath);

}
