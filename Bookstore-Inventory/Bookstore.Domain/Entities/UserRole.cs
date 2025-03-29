using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Bookstore.Domain.Entities
{
    public enum UserRole
    {
        ADMINISTRATOR,
        WAREHOUSE_KEEPER
    }
}
