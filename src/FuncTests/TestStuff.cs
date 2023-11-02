namespace FuncTests;

static class TestStuff
{
    public static  string GetTestAssemblyPath()
    {
        return Path.GetFullPath
            (
                Path.Combine
                (
                    Directory.GetCurrentDirectory(), 
                    "./TestTask.dll"
                )
            );
    }
}