namespace Interfaces
{
    public interface IPlayerTurnsManager
    {
        public void EndPlayerTurn();
        public void StartPlayerTurn();
        public void UpdateAdjacentCoinText(int x , int y , int newCoinValue);
        public void UpdateCoinColor(int x , int y);
        public void UpdateCoinUIImageColors();
        public void UpdateTrailColor();
    }
}
