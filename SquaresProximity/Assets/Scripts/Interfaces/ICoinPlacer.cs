namespace Interfaces
{
    using UnityEngine;
    
    public interface ICoinPlacer
    {
        public void PlaceCoin(Vector2Int cellIndexAtMousePosition);
    }
}