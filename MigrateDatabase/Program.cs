using System;
using System.IO;
using DatabaseInterface;
using Microsoft.EntityFrameworkCore;

namespace MigrateDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string connectionString;

                using (StreamReader sr = new StreamReader("connectionstring.txt"))
                {
                    connectionString = sr.ReadToEnd();
                }

                Console.WriteLine("Starting migration...");

                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                optionsBuilder.UseMySql(connectionString);

                using (var context = new ApplicationDbContext(optionsBuilder.Options))
                {
                    context.Database.Migrate();
                }

                Console.WriteLine("Finished migration!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}