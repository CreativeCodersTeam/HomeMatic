using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Api.Core.Values
{
    [PublicAPI]
    public interface ICcuValueIo
    {
        Task WriteAsync(object value);

        Task<object> ReadAsync();
        
        Task WriteAsync<T>(T value);

        Task<T> ReadAsync<T>();
        
        CcuValueAddress ValueAddress { get; set; }
    }
    
    [PublicAPI]
    public interface ICcuValueIo<T>
    {
        Task WriteAsync(T value);

        Task<T> ReadAsync();
        
        CcuValueAddress ValueAddress { get; set; }
    }
}