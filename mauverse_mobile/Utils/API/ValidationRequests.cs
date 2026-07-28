using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.Utils.API;

public sealed class ValidationRequests(IAPIService apiService) : IValidationRequests
{
    public Task<bool> CheckGroupAsync(
        string group,
        CancellationToken cancellationToken = default) =>
        apiService.PostAsync<bool>("/check_group", new { group_name = group }, cancellationToken);

    public Task<bool> CheckTeacherAsync(
        string teacher,
        CancellationToken cancellationToken = default) =>
        apiService.PostAsync<bool>("/check_teacher", new { teacher_name = teacher }, cancellationToken);
}
