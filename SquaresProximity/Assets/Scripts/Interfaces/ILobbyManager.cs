namespace Interfaces
{
    using System.Threading.Tasks;

    public interface ILobbyManager
    {
        Task CreateLobby();
        void JoinLobby();
        Task LeaveLobby();
    }
}