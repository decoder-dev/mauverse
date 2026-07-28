namespace mau.DTOModels;

public sealed class RssDTO : BaseDTO
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Publish { get; set; } = string.Empty;
}
