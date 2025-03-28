using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Infrastructure.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext(IConfiguration configuration)
        {
            var settings = configuration.GetSection("DatabaseSettings");
            var client = new MongoClient(settings["ConnectionString"]);
            _database = client.GetDatabase(settings["DatabaseName"]);
        }
        public IMongoCollection<T> GetCollection<T>(string name) => _database.GetCollection<T>(name); 
    }
}
