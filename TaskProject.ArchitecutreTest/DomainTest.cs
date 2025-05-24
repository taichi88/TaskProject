using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using NetArchTest.Rules;

namespace TaskProject.ArchitectureTest
{
    public class DomainTest
    {
        [Fact]
       public void ShouldPass_whenDomainDoesNotReferenceApplication()
        {
            //arrange
            var types = Types.InAssembly(Assembly.Load("Domain"));

            // Act

            var result = types
                .ShouldNot()
                .HaveDependencyOn("Application")
                .GetResult();

            // Assert
            Assert.True(result.IsSuccessful);
        }
    }
}
