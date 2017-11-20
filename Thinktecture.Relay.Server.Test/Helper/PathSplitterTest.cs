using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Thinktecture.Relay.Server.Helper
{
	// ReSharper disable JoinDeclarationAndInitializer
	[TestClass]
	public class PathSplitterTest
	{
		[TestMethod]
		public void Split_returns_a_PathInformation_DTO_with_correctly_filled_path_properties()
		{
			var sut = new PathSplitter();
			var result = sut.Split("userName/targetKey/services/index.html");

			result.CompletePath.Should().Be("userName/targetKey/services/index.html");
			result.OnPremiseTargetKey.Should().Be("targetKey");
			result.LocalUrl.Should().Be("/services/index.html");
			result.UserName.Should().Be("userName");
			result.PathWithoutUserName.Should().Be("targetKey/services/index.html");
			result.BasePath.Should().Be("/relay/userName/targetKey");
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Split_throws_an_exception_when_given_path_is_null()
		{
			IPathSplitter sut = new PathSplitter();

			sut.Split(null);
		}

		[TestMethod]
		public void SplitPath_splits_a_given_path_into_its_parts()
		{
			var sut = new PathSplitter();
			var result = sut.SplitPath("this/is/a/test").ToList();

			result.Count.Should().Be(4);
			result[0].Should().Be("this");
			result[1].Should().Be("is");
			result[2].Should().Be("a");
			result[3].Should().Be("test");
		}

		[TestMethod]
		public void GetTargetKey_extracts_the_local_target_key_from_a_given_path()
		{
			var sut = new PathSplitter();
			var result = sut.GetTargetKey(new List<string> { "userName", "targetKey" });

			result.Should().Be("targetKey");
		}

		[TestMethod]
		public void GetTargetKey_returns_null_when_path_doesnt_have_at_least_two_parts()
		{
			var sut = new PathSplitter();
			var result = sut.GetTargetKey(new List<string> { "onlyonepart" });

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetLocalUrl_extracts_the_local_URL_from_a_given_path()
		{
			var sut = new PathSplitter();
			var result = sut.GetLocalUrl(new List<string> { "userName", "targetKey", "url" });

			result.Should().Be("/url");
		}

		[TestMethod]
		public void GetLocalUrl_extracts_an_empty_local_URL_from_a_given_path()
		{
			var sut = new PathSplitter();
			var result = sut.GetLocalUrl(new List<string> { "userName", "targetKey", "" });

			result.Should().Be("/");
		}

		[TestMethod]
		public void GetLocalUrl_returns_null_when_path_doesnt_have_at_least_three_parts()
		{
			var sut = new PathSplitter();
			var result = sut.GetLocalUrl(new List<string> { "userName", "targetKey" });

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetUserName_returns_the_userName_extracted_from_a_given_path()
		{
			var sut = new PathSplitter();
			var result = sut.GetUserName(new List<string> { "userName", "targetKey" });

			result.Should().Be("userName");
		}

		[TestMethod]
		public void GetUserName_returns_the_userName_extracted_from_a_given_path_that_only_contains_a_userName()
		{
			var sut = new PathSplitter();
			var result = sut.GetUserName(new List<string> { "userName" });

			result.Should().Be("userName");
		}

		[TestMethod]
		public void GetPathWithoutUserName_returns_path_without_user_name_based_on_given_path()
		{
			var sut = new PathSplitter();
			var result = sut.GetPathWithoutUserName(new List<string> { "userName", "targetKey", "file.html" });

			result.Should().Be("targetKey/file.html");
		}

		[TestMethod]
		public void GetPathWithoutUserName_returns_null_if_given_path_is_empty()
		{
			var sut = new PathSplitter();
			var result = sut.GetPathWithoutUserName(new List<string>());

			result.Should().BeNull();
		}
	}
}
