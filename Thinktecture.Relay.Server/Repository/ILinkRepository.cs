using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	public interface ILinkRepository
	{
		IEnumerable<Link> GetLinks();
		PageResult<Link> GetLinks(PageRequest paging);
		Link GetLink(Guid linkId);
		Link GetLink(string userName);
		CreateLinkResult CreateLink(string symbolicName, string userName);
		bool UpdateLink(Link linkId);
		void DeleteLink(Guid linkId);

		bool Authenticate(string userName, [Optional] string password, out Guid linkId);
		bool IsUserNameAvailable(string userName);
		Task AddOrRenewActiveConnectionAsync(Guid linkId, Guid originId, string connectionId, int connectorVersion);
		Task RenewActiveConnectionAsync(string connectionId);
		Task RemoveActiveConnectionAsync(string connectionId);
		void DeleteAllConnectionsForOrigin(Guid originId);
	}
}
