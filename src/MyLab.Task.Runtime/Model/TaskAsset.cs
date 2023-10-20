namespace MyLab.Task.Runtime;

class TaskAsset
{
    public string Name { get; }

    public IAssemblyLoader Loader { get; }

    public TaskAsset(string name, IAssemblyLoader loader)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));
        }

        if (loader is null)
        {
            throw new ArgumentNullException(nameof(loader));
        }

        (Name, Loader) = (name, loader);
    } 
}
