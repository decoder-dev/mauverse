using mau.DTOModels;
using mau.Models;

namespace mau.Utils.API.Interaface;

public interface IUserRequests
{
    Task<IEnumerable<string>> GetGroupAsync(string group, CancellationToken cancellationToken = default);
    Task<UserDTO> Auth(string username, string password, CancellationToken cancellationToken = default);
    Task<List<Message>> GetNotifications(string token, int userId, CancellationToken cancellationToken = default);
    Task<SubGroupDTO> GetSubGroupsAsync(string groupName, CancellationToken cancellationToken = default);
}
