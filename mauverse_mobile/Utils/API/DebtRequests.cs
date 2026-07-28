using mau.DTOModels;
using mau.Models;
using mau.Utils.API.Interaface;
using mau.Utils.Services.Interface;

namespace mau.Utils.API;

public sealed class DebtRequests(IAPIService apiService) : IDebtRequests
{
    public async Task<List<Debts>> GetDebtByStudentGroup(
        int semesterNumber,
        string group,
        string firstName,
        string secondName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            first_name = firstName,
            name = secondName,
            last_name = lastName,
            group_name = group,
            semester_number = semesterNumber
        };
        var result = await apiService.PostAsync<DebtsDTO>("/get_debts", request, cancellationToken);
        return result.Debts;
    }

    public async Task<List<Debts>> GetDebtsBySemester(
        string creditBook,
        int semester,
        CancellationToken cancellationToken = default)
    {
        var request = new { credit_book = creditBook, semester_number = semester };
        var result = await apiService.PostAsync<DebtsDTO>("/get_debts", request, cancellationToken);
        return result.Debts;
    }

    public async Task<List<StudentDebt>> GetGroupDebts(
        string group,
        CancellationToken cancellationToken = default)
    {
        var result = await apiService.PostAsync<StudentDebtsDTO>(
            "/get_debts",
            new { group_name = group },
            cancellationToken);
        return result.Students;
    }

    public async Task<List<StudySemester>> GetSemester(
        string creditBook,
        CancellationToken cancellationToken = default)
    {
        var result = await apiService.PostAsync<SemesterDTO>(
            "/get_semesters",
            new { credit_book = creditBook },
            cancellationToken);
        return result.Semesters;
    }

    public async Task<List<StudySemester>> GetSemesterByStudentGroup(
        string group,
        string firstName,
        string secondName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            first_name = firstName,
            name = secondName,
            last_name = lastName,
            group_name = group
        };
        var result = await apiService.PostAsync<SemesterDTO>("/get_semesters", request, cancellationToken);
        return result.Semesters;
    }
}
