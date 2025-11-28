using System;
using Xunit;

namespace UnitTests
{
    public class TimeSpanBehavior
    {
        [Fact]
        public void ShouldCreateZeroDefault()
        {
            //Arrange
            TimeSpan ts = TimeSpan.FromSeconds(0);
        
            //Act && Assert
            Assert.Equal(default, ts);
        }
    }
}