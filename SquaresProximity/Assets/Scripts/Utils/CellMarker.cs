namespace Utils
{
    using Interfaces;
    using Managers;
    using TMPro;
    using UnityEditor;
    using UnityEngine;
    
    public class CellMarker : MonoBehaviour , IColourAdjuster
    {
        private GridManager _gridManager;
        private Vector2Int _currentIndex;
        
        [SerializeField] private GameObject indexLabelTMPObj;
        [SerializeField] private TextMeshPro indexLabelTextTMP;

        private void Awake()
        {
            _gridManager = FindAnyObjectByType<GridManager>();
        }

        private void Update()
        {
            #if UNITY_EDITOR
            
                _currentIndex = _gridManager.WorldToCell(transform.position);
                
                if(EditorApplication.isPlaying)
                {
                    indexLabelTMPObj.SetActive(true);
                    indexLabelTextTMP.text = _currentIndex.ToString();

                    Color cellColor = GetColor();
                    indexLabelTextTMP.color = GetContrastingColor(cellColor);
                }
            
            #endif
        }
        public Color GetContrastingColor(Color color)
        {
            float luminance = (0.299f * color.r + 0.587f * color.g + 0.114f * color.b);
            return luminance > 0.5f ? Color.black : Color.white;
        }
        
        public Color GetColor()
        {
            Vector2Int cellIndex = _gridManager.WorldToCell(transform.position);
            return _gridManager.CellSpriteRenderersData.GetValue(cellIndex.x , cellIndex.y).color;
        }
    }
}