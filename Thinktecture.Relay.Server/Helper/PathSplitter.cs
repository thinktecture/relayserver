using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Helper
{
	internal class PathSplitter : IPathSplitter
	{
		public PathInformation Split(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path", "Path cannot be null");
			}

			var splitPath = SplitPath(path).ToList();

			return new PathInformation
			{
				CompletePath = path,
				OnPremiseTargetKey = GetTargetKey(splitPath),
				LocalUrl = GetLocalUrl(splitPath),
				PathWithoutUserName = GetPathWithoutUserName(splitPath),
				UserName = GetUserName(splitPath)
			};
		}

		internal IEnumerable<string> SplitPath(string path)
		{
			return path.Split('/');
		}

		internal string GetTargetKey(List<string> splitPath)
		{
			if (splitPath.Count < 2)
			{
				return null;
			}

			return splitPath.Skip(1).First();
		}

		internal string GetLocalUrl(List<string> splitPath)
		{
			if (splitPath.Count < 3)
			{
				return null;
			}

			return "/" + String.Join("/", splitPath.Skip(2));
		}

		internal string GetPathWithoutUserName(List<string> splitPath)
		{
			if (splitPath.Count == 0)
			{
				return null;
			}

			return String.Join("/", splitPath.Skip(1));
		}

		internal string GetUserName(List<string> splitPath)
		{
			return splitPath[0];
		}
	}
}