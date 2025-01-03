using Data;

public class GridData<T>
{
    private T[,] _grid;

    public GridData(GridInfo gridInfo , T defaultValue = default)
    {
        _grid = new T[gridInfo.Cols , gridInfo.Rows];
        
        for(int x = 0; x < gridInfo.Cols; x++)
        {
            for(int y = 0; y < gridInfo.Rows; y++)
            {
                _grid[x , y] = defaultValue;
            }
        }
    }

    public T GetValue(int columnIndex , int rowIndex)
    {
        if(IsValidIndex(columnIndex , rowIndex))
        {
            return _grid[columnIndex , rowIndex];
        }
        
        return default;
    }

    public void SetValue(int columnIndex , int rowIndex , T value)
    {
        if(IsValidIndex(columnIndex , rowIndex))
        {
            _grid[columnIndex , rowIndex] = value;
        }
    }

    public bool IsValidIndex(int columnIndex , int rowIndex)
    {
        return columnIndex >= 0 && columnIndex < _grid.GetLength(0) && rowIndex >= 0 && rowIndex < _grid.GetLength(1);
    }
}