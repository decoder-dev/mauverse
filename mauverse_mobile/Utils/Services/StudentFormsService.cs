using mau.DTOModels;
using mau.Utils.Services.Interface;

namespace mau.Utils.Services;

public sealed class StudentFormsService(IAPIService apiService) : IStudentFormsService
{
    public async Task SubmitCertificateAsync(
        string email,
        string fullName,
        IReadOnlyCollection<StudentFormField> fields,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name cannot be empty", nameof(fullName));
        ArgumentNullException.ThrowIfNull(fields);
        if (fields.Count == 0)
            throw new ArgumentException("At least one form field is required", nameof(fields));

        var result = await apiService.PostAsync<StudentFormSubmissionResult>(
            "/send_order",
            new
            {
                from = email,
                username = fullName,
                text = fields
            },
            cancellationToken);

        if (!result.Success)
            throw new InvalidOperationException("Сервер не подтвердил отправку заявки");
    }
}
