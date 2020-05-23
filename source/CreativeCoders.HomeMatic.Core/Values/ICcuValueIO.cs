using System.Threading.Tasks;
using JetBrains.Annotations;

namespace CreativeCoders.HomeMatic.Core.Values
{
    [PublicAPI]
    public interface ICcuValueIo
    {
        Task WriteAsync(object value);

        Task<object> ReadAsync();
        
        Task WriteAsync<T>(T value);

        Task<T> ReadAsync<T>();
        
        ICcuValueAddress ValueAddress { get; }
    }
    
    [PublicAPI]
    public interface ICcuValueIo<T>
    {
        Task WriteAsync(T value);

        Task<T> ReadAsync();
        
        ICcuValueAddress ValueAddress { get; }
    }
}