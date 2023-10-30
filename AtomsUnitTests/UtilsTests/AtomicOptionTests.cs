using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsUnitTests.UtilsTests
{
    public class AtomicOptionTests
    {
        [Fact]
        public void WhenAFunctionReturnsAtomicOptionDefined_ThenItIsDefinedVariant()
        {
            // Act
            AtomicOption<string> atomicOption = GetDefined(true);
            // Assert
            var atomicOptionDefined = Assert.IsType<AtomicOption<string>.Defined>(atomicOption);
            Assert.Equal("Hello World!", atomicOptionDefined.Value);
        }

        [Fact]
        public void WhenAFunctionReturnsAtomicOptionWithoutValue_ThenItIsAbsentVariant()
        {
            // Act
            AtomicOption<string> atomicOption = GetDefined(false);
            // Assert
            Assert.IsType<AtomicOption<string>.Empty>(atomicOption);
        }

        private AtomicOption<string> GetDefined(bool shouldBeDefined)
        {
            if (!shouldBeDefined) return new AtomicOption<string>.Empty();
            return new AtomicOption<string>.Defined("Hello World!");
        }
    }
}
