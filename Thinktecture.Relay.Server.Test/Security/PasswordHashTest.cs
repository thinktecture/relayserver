using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Security
{
    [TestClass]
    public class PasswordHashTest
    {
        [TestMethod]
        public void GeneratePassword_generates_a_random_byte_array_of_length_100()
        {
            var sut = new PasswordHash();
            var result = sut.GeneratePassword(100);

            result.Length.Should().Be(100);
        }

        [TestMethod]
        public void CreatePasswordInformation_creates_a_password_information_which_can_be_used_for_validation()
        {
            var sut = new PasswordHash();
            var password = sut.GeneratePassword(100);
            var passwordInformation = sut.CreatePasswordInformation(password);

            passwordInformation.Should().NotBeNull();
            passwordInformation.Iterations.Should().BeGreaterThan(2500);

            var isValid = sut.ValidatePassword(password, passwordInformation);

            isValid.Should().BeTrue();
        }
    }
}