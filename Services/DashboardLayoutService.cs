using Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class DashboardLayoutService : IDashboardLayoutService
    {
        private readonly IRepositoryWrapper _repositoryWrapper;

        public DashboardLayoutService(IRepositoryWrapper repositoryWrapper)
        {
            _repositoryWrapper = repositoryWrapper;
        }

        public async Task<List<string>> GetLayoutAsync(string userId)
        {
            var layout = await _repositoryWrapper.DashboardLayoutRepository.GetByUserIdAsync(userId);
            return layout?.WidgetOrder ?? new List<string>();
        }

        public async Task SaveLayoutAsync(string userId, List<string> widgetOrder)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

            var layout = new DashboardLayout
            {
                UserId = userId,
                WidgetOrder = widgetOrder
            };

            await _repositoryWrapper.DashboardLayoutRepository.SaveOrUpdateAsync(layout);
            await _repositoryWrapper.Save();
        }
    }
}
