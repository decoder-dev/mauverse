using mau.Models;

namespace mau.DTOModels;

public sealed class RoomDTO : BaseDTO
{
    public IEnumerable<Room> Rooms { get; set; } = [];
}
