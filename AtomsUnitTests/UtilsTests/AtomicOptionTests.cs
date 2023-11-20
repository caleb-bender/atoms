using CalebBender.Atoms.Utils;
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
        public void WhenAFunctionReturnsAtomicOptionExists_ThenItIsExistsVariant()
        {
            // Act
            AtomicOption<string> atomicOption = GetExists(true);
            // Assert
            var atomicOptionExists = Assert.IsType<AtomicOption<string>.Exists>(atomicOption);
            Assert.Equal("Hello World!", atomicOptionExists.Value);
        }

        [Fact]
        public void WhenAFunctionReturnsAtomicOptionWithoutValue_ThenItIsEmptyVariant()
        {
            // Act
            AtomicOption<string> atomicOption = GetExists(false);
            // Assert
            Assert.IsType<AtomicOption<string>.Empty>(atomicOption);
        }

        private AtomicOption<string> GetExists(bool shouldBeExists)
        {
            if (!shouldBeExists) return new AtomicOption<string>.Empty();
            return new AtomicOption<string>.Exists("Hello World!");
        }
    }
}
