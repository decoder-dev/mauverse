namespace mau.Utils.API.Interaface;

public interface IValidationRequests
{
    Task<bool> CheckGroupAsync(string group, CancellationToken cancellationToken = default);
    Task<bool> CheckTeacherAsync(string teacher, CancellationToken cancellationToken = default);
}
