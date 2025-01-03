namespace Interfaces
{
    public interface IGooglePlayGamesManager
    {
        bool IsAuthenticated { get; }
        void Authenticate();
    }
}
