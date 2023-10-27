using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;

namespace UnitTests;

public class OptionsBehavior
{
    [Theory(DisplayName = "Should deserizalize TimeSpan")]
    [MemberData(nameof(GetTimeSpanCases))]
    public void ShouldDeserializeTimeSpan(string value, TimeSpan expected)
    {
        //Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new Dictionary<string, string>
                {
                    { nameof(TestOptions.TimsSpan), value } 
                }
            )
            .Build();
        
        var services = new ServiceCollection()
            .Configure<TestOptions>(config)
            .BuildServiceProvider();

        //Act
        var opts = services.GetService<IOptions<TestOptions>>()?.Value;

        //Assert
        Assert.NotNull(opts);
        Assert.Equal(expected, opts.TimsSpan);
    }

    [Fact(DisplayName = "Should deserialize section")]
    public void ShouldDeserializeSection()
    {
        //Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new Dictionary<string, string>
                {
                    { nameof(TestOptions.Section) + ":foo", "bar" } 
                }
            )
            .Build();
        
        var services = new ServiceCollection()
            .Configure<TestOptions>(config)
            .BuildServiceProvider();

        //Act
        var opts = services.GetService<IOptions<TestOptions>>()?.Value;
        
        //Assert
        Assert.NotNull(opts?.Section);
        Assert.Equal(nameof(TestOptions.Section), opts.Section.Key);
        Assert.Equal(nameof(TestOptions.Section), opts.Section.Path);
        Assert.Equal("bar", opts.Section["foo"]);
    }

    [Fact(DisplayName = "Should merge config")]
    public void ShouldMergeConfig()
    {
        //Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection
            (
                new Dictionary<string, string>
                {
                    { "Test:" + nameof(TestOptions.Section) + ":foo", "bar" } 
                }
            )
            .Build();
        
        var services = new ServiceCollection()
            .Configure<TestOptions>(config.GetSection("Test"))
            .BuildServiceProvider();

        var opts = services.GetService<IOptions<TestOptions>>()!.Value;

        //Act
        var configNew = new ConfigurationBuilder()
            .AddConfiguration(opts.Section)
            .Build();
        
        //Assert
        Assert.Equal("Test:Section", opts.Section!.Path);        
        Assert.Equal("bar", configNew["foo"]);
    }

    public static object[][] GetTimeSpanCases()
    {
        return new object[][]
        {
            new object[] { "00:00:02", TimeSpan.FromSeconds(2) },
            new object[] { "00:02:00", TimeSpan.FromMinutes(2) },
            new object[] { "02:00:00", TimeSpan.FromHours(2) },
            new object[] { "2.00:00:00", TimeSpan.FromDays(2) },
            new object[] { "2", TimeSpan.FromDays(2) },
        };
    }

    class TestOptions
    {
        public TimeSpan TimsSpan{get;set;}

        public IConfigurationSection? Section { get; set; }
    }

    class TestOptionsRoot
    {
        public TestOptions? Test{ get; set; }
    }
}