namespace CreativeCoders.HomeMatic.Tools.ControlCenter.Backend.Repositories.LiteDbRepository;

public interface IObjectKey<T>
{
    T Id { get; set; }
}