namespace MyLab.Task.Runtime;

interface ITaskAssetProvider
{
    IEnumerable<TaskAssetSource> Provide();
}

