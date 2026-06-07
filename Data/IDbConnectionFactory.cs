using System.Data;

namespace WebLibraryApi.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection Create();
    }
}
