using mau.Models;

namespace mau.DTOModels;

public sealed class StudentDebtsDTO : BaseDTO
{
    public List<StudentDebt> Students { get; set; } = [];
}
