using mau.DTOModels;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.Utils.API;

public sealed class UserRequests(IAPIService apiService) : IUserRequests
{
    public async Task<IEnumerable<string>> GetGroupAsync(
        string group,
        CancellationToken cancellationToken = default)
    {
        var result = await apiService.PostAsync<GroupDTO>(
            "/get_groups",
            new { group_name = group },
            cancellationToken);
        return result.Groups;
    }

    public Task<UserDTO> Auth(
        string username,
        string password,
        CancellationToken cancellationToken = default) =>
        apiService.PostAsync<UserDTO>("/auth", new { username, password }, cancellationToken);

    public Task<SubGroupDTO> GetSubGroupsAsync(
        string groupName,
        CancellationToken cancellationToken = default) =>
        apiService.PostAsync<SubGroupDTO>(
            "/get_subgroups",
            new { group_name = groupName },
            cancellationToken);

    public Task<List<Message>> GetNotifications(
        string token,
        int userId,
        CancellationToken cancellationToken = default) =>
        apiService.PostAsync<List<Message>>(
            "/get_notifications",
            new { token, user_id = userId },
            cancellationToken);
}
