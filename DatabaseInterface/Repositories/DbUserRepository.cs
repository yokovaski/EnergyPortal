using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface.Repositories;

public class DbUserRepository(ApplicationDbContext dbContext)
{
    private readonly ApplicationDbContext dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    
    public async Task StoreUsers(List<ApplicationUser> users)
    {
        foreach (var user in users)
        {
            // Check if the user already exists
            var exists = await dbContext.Users.AnyAsync(u => u.Email == user.Email);

            if (exists)
            {
                continue;
            }

            dbContext.Users.Add(user);
        }
        
        await dbContext.SaveChangesAsync();
    }
}