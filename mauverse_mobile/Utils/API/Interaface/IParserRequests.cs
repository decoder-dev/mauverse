using mau.DTOModels;
using mau.Models;

namespace mau.Utils.API.Interaface;

public interface IParserRequests
{
    Task<IEnumerable<DeptInfoDTO>> GetDeptsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TelephoneInfoDTO>> GetTelephonesAsync(
        DeptInfoDTO department,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<RssDTO>> GetNewsAsync(
        RssData type,
        CancellationToken cancellationToken = default);
    Task<UniversityInfo> GetTeacherInfoAsync(
        string teacher,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Room>> GetRoomsAsync(
        string room,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Room>> GetRoomsAsync(
        bool isAll,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetTeachersAsync(
        string teacher,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetTeachersAsync(
        bool isAll,
        CancellationToken cancellationToken = default);
}
