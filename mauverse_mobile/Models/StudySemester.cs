namespace mau.Models;

public sealed class StudySemester
{
    public int SemesterNumber { get; set; }
    public string Semester { get; set; } = string.Empty;
    public string SemesterSubtitle { get; set; } = "Нажмите, чтобы увидеть задолженности";
    public List<Debts> Debts { get; set; } = [];
}
