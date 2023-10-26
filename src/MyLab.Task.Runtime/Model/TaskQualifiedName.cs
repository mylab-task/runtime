namespace MyLab.Task.Runtime;

public record TaskQualifiedName(string Asset, string? LocalName)
{
    public override string ToString()
    {
        return LocalName != null ? Asset : $"{Asset}:{LocalName}"; 
    }
}
