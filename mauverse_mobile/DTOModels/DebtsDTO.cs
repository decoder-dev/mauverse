using mau.Models;

namespace mau.DTOModels;

public sealed class DebtsDTO : BaseDTO
{
    public List<Debts> Debts { get; set; } = [];
}
