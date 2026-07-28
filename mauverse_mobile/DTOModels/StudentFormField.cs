namespace mau.DTOModels;

public sealed record StudentFormField(string Title, string Value);

public sealed class StudentFormSubmissionResult : BaseDTO
{
    public bool Success { get; set; }
}
