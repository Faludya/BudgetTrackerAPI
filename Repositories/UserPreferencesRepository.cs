using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class UserPreferencesRepository : RepositoryBase<UserPreferences>, IUserPreferencesRepository
    {
        public UserPreferencesRepository(Models.AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public async Task<UserPreferences> GetUserPreferences(string userId)
        {
            return await _appDbContext.UserPreferences.Where(t => t.UserId == userId).FirstAsync(); ;
        }
    }
}