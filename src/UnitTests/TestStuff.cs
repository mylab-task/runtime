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

    public static string GetOldRefTestAssemblyPath()
    {
        return Path.GetFullPath
            (
                Path.Combine
                (
                    Directory.GetCurrentDirectory(), 
                    "../../../../TestOldTask/bin/Debug/net6.0/TestOldTask.dll"
                )
            );
    }
}
