namespace Utils
{
    using Interfaces;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;
    
    public class ColourAdjuster : MonoBehaviour , IColourAdjuster
    {
        private Color _tmpTextOriginalColour;
        
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text textMeshProText;

        private void Awake()
        {
            _tmpTextOriginalColour = textMeshProText.color;
        }

        public Color GetContrastingColor(Color color)
        {
            float luminance = (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
            return luminance > 0.5f ? Color.black : Color.white;
        }

        public Color GetColor()
        {
            return button.image.color;
        }
        
        public void OnPointerEnter()
        {
            Color backgroundColour = GetColor();
            textMeshProText.color = GetContrastingColor(backgroundColour);
        }

        public void OnPointerExit()
        {
            textMeshProText.color = _tmpTextOriginalColour;
        }
    }
}
