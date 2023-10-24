namespace UnitTests;

static class TestStuff
{
    public static  string GetTestAssemblyPath()
    {
        return Path.GetFullPath
            (
                Path.Combine
                (
                    Directory.GetCurrentDirectory(), 
                    "../../../../TestTask/bin/Debug/net6.0/TestTask.dll"
                )
            );
    }
}
