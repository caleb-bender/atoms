using Atoms.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtomsUnitTests.UtilsTests
{
    public class AtomicResultTests
    {

        [Fact]
        public void WhenAFunctionReturnsAnAtomicResultErrorWithException_ItHasThatException()
        {
            // Act
            AtomicResult<InvalidOperationException> atomicResultError = GetResultError();
            // Assert
            var errorVariant = Assert.IsType<AtomicResult<InvalidOperationException>.Error>(atomicResultError);
            Assert.Equal("invalid operation", errorVariant.Except.Message);
        }

        [Fact]
        public void WhenAFunctionReturnsAnAtomicResultWithoutException_ThenOkVariantIsReturned()
        {
            // Act
            AtomicResult<InvalidOperationException> atomicResultOk = GetResultOk();
            // Assert
            Assert.IsType<AtomicResult<InvalidOperationException>.Ok>(atomicResultOk);
        }

        private AtomicResult<InvalidOperationException> GetResultOk()
        {
            return new AtomicResult<InvalidOperationException>.Ok();
        }

        [Fact]
        public void WhenEvenNumberIsPassedToFunction_ThenErrorVariantIsReturned()
        {
            // Act
            AtomicResult<int, ArgumentException> atomicResultError = GetNumberPlusOneIfOddElseError(2);
            // Assert
            var errorVariant = Assert.IsType<AtomicResult<int, ArgumentException>.Error>(atomicResultError);
            Assert.Equal("Number is even", errorVariant.Except.Message);
        }

        [Fact]
        public void WhenOddNumberIsPassedToFunction_ThenOkVariantIsReturned()
        {
            // Act
            var atomicResultOk = GetNumberPlusOneIfOddElseError(3);
            // Assert
            var okVariant = Assert.IsType<AtomicResult<int, ArgumentException>.Ok>(atomicResultOk);
            Assert.Equal(3 + 1, okVariant.Value);
        }

        private AtomicResult<int, ArgumentException> GetNumberPlusOneIfOddElseError(int v)
        {
            var argumentErr = new ArgumentException("Number is even");
            if (v % 2 == 0) return new AtomicResult<int, ArgumentException>.Error(argumentErr);
            return new AtomicResult<int, ArgumentException>.Ok(v + 1);
        }

        private AtomicResult<InvalidOperationException> GetResultError()
        {
            return new AtomicResult<InvalidOperationException>.Error(new InvalidOperationException("invalid operation"));
        }
    }
}
