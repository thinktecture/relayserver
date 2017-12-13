using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Security
{
	[TestClass]
	public class PasswordComplexityValidatorTests
	{
		[TestMethod]
		public void ValidatePassword_validates_a_good_password()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("username", "12345678aA!", out var errorMessage);

			result.Should().BeTrue();
			errorMessage.Should().BeNull();
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_username_and_password_match()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("username", "username", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Username and password must not be the same.");
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_password_is_too_short()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("UserName", "1234567", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Password needs to be at least 8 characters long.");
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_password_misses_lowercase_chars()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("UserName", "1AAAAAAA!", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Password must contain at least one lower case character.");
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_password_misses_uppercase_chars()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("UserName", "1aaaaaaa!", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Password must contain at least one upper case character.");
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_password_misses_numerical_chars()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("UserName", "aaaaAAAA!", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Password must contain at least one number.");
		}

		[TestMethod]
		public void ValidatePassword_gives_error_when_password_misses_special_chars()
		{
			var sut = new PasswordComplexityValidator();

			var result = sut.ValidatePassword("UserName", "aaaaAAAA1", out var errorMessage);

			result.Should().BeFalse();
			errorMessage.Should().Contain("Password must contain at least one special character.");
		}
	}
}
