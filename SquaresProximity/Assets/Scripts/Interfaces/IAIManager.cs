namespace Interfaces
{
    using System.Collections;
    using UnityEngine;
    
    public interface IAIManager
    {
        public IEnumerator AnimateCoinEffect(SpriteRenderer coinRenderer , Color? capturingColor = null);
        public IEnumerator AIPlaceCoinCoroutine();
        public Vector2Int FindCellToPlaceCoinOn();
    }
}