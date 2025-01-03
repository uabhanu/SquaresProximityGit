namespace Interfaces
{
    using UnityEngine;
    
    public interface IColourAdjuster
    {
        public Color GetColor();
        public Color GetContrastingColor(Color color);
    }
}