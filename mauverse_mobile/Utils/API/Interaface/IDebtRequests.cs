using mau.Models;

namespace mau.Utils.API.Interaface;

public interface IDebtRequests
{
    Task<List<StudySemester>> GetSemester(
        string creditBook,
        CancellationToken cancellationToken = default);

    Task<List<Debts>> GetDebtsBySemester(
        string creditBook,
        int semester,
        CancellationToken cancellationToken = default);

    Task<List<StudentDebt>> GetGroupDebts(
        string group,
        CancellationToken cancellationToken = default);

    Task<List<StudySemester>> GetSemesterByStudentGroup(
        string group,
        string firstName,
        string secondName,
        string lastName,
        CancellationToken cancellationToken = default);

    Task<List<Debts>> GetDebtByStudentGroup(
        int semesterNumber,
        string group,
        string firstName,
        string secondName,
        string lastName,
        CancellationToken cancellationToken = default);
}
