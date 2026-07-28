using System.Collections.ObjectModel;

namespace mau.Models;

public sealed class CampusBuildingGroup(
    string name,
    string stops,
    IEnumerable<CampusBuilding> buildings) : ObservableCollection<CampusBuilding>(buildings)
{
    public string Name { get; } = name;
    public string Stops { get; } = stops;
}
