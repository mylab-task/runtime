namespace MyLab.Task.Runtime;

public interface ITaskAssetProvider
{
    IEnumerable<TaskAssetSource> Provide();
}

