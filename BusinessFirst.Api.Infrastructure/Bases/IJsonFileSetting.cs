namespace RenameMe.Api.Infrastructure.Bases
{
    public interface IJsonFileSetting : ISetting
    {
        string JsonFilePath { get; }
    }
}
