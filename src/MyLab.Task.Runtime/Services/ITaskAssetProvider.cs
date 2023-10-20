namespace MyLab.Task.Runtime;

interface ITaskAssetProvider
{
    IEnumerable<TaskAsset> Provide();
}

