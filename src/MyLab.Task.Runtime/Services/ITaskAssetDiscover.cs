namespace MyLab.Task.Runtime;

interface ITaskAssetDiscover
{
    IEnumerable<TaskAsset> Discover();
}

