using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MyLab.Task.Runtime;
using Xunit;

namespace UnitTests
{
    public class ConfigurationBuilderExtensionsBehavior
    {
        [Fact(DisplayName = "Should add subsection")]
        public void ShouldAdd()
        {
            //Arrange
            var configOrigin = new ConfigurationBuilder()
                .AddInMemoryCollection
                (
                    new Dictionary<string, string>
                    {
                        { "test:foo:bar", "baz" }
                    }
                )
                .Build();
        
            var testSection = configOrigin.GetSection("test");
    
            //Act       
            var configRes = new ConfigurationBuilder()
                .AddSubconfig(testSection, "test")
                .AddConfiguration(testSection)
                .Build();
        
            //Assert
            Assert.Equal("baz", configRes["test:foo:bar"]);
        }
    }
}