namespace Services.Interfaces
{
    public interface IDashboardLayoutService
    {
        Task<List<string>> GetLayoutAsync(string userId);
        Task SaveLayoutAsync(string userId, List<string> widgetOrder);
    }
}
