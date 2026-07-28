using mau.DTOModels;

namespace mau.Utils.Services.Interface;

public interface IStudentFormsService
{
    Task SubmitCertificateAsync(
        string email,
        string fullName,
        IReadOnlyCollection<StudentFormField> fields,
        CancellationToken cancellationToken = default);
}
