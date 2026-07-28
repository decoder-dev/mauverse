using mau.Models;

namespace mau.DTOModels;

public sealed class SemesterDTO : BaseDTO
{
    public List<StudySemester> Semesters { get; set; } = [];
}
