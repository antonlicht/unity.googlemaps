using UnityEngine;

public class SmartGrid<TCell, TContent>
    where TCell : ICell<TContent>
    where TContent : RefCounted
{
    private Grid<TCell> _grid;

    public TContent this[int x, int y]
    {
        get
        {
            TCell cell = _grid[x, y];
            if (Equals(cell, default(TCell)))
                return default(TContent);
            return cell.GetContent();
        }
        set
        {
            TCell cell = _grid[x, y];
            if (Equals(cell, default(TCell)))
                return;
            cell.SetContent(value);
        }
    }

    public SmartGrid(int width, int height)
    {
        _grid = new Grid<TCell>(width, height);
    }

    public SmartGrid(TCell[] cells, int width, int height)
    {
        _grid = new Grid<TCell>(cells, width, height);
    }

    public void Shift(int deltaX, int deltaY)
    {
        Grid<TContent> contentGrid = Grid<TContent>.CreateEmpty(_grid);
        contentGrid.SetIDOffset(_grid.IDOffsetX, _grid.IDOffsetY);

        for (int i = contentGrid.IDOffsetY; i < contentGrid.IDOffsetY + contentGrid.Height; i++)
        {

            for (int j = contentGrid.IDOffsetX; j < contentGrid.IDOffsetX + contentGrid.Width; j++)
            {
                TContent temp = _grid[j, i].GetContent();
                if (temp != null)
                    temp.AddRef();
                contentGrid[j, i] = temp;
            }
        }
        _grid.ShiftIDs(-deltaX, -deltaY);
        for (int i = _grid.IDOffsetY; i < _grid.IDOffsetY + _grid.Height; i++)
        {

            for (int j = _grid.IDOffsetX; j < _grid.IDOffsetX + _grid.Width; j++)
            {
                if (_grid[j, i] is Object)
                    (_grid[j, i] as Object).name = "cell_" + j + "_" + i;
                _grid[j, i].SetContent(contentGrid[j, i]);
            }
        }

        for (int i = contentGrid.IDOffsetY; i < contentGrid.IDOffsetY + contentGrid.Height; i++)
        {

            for (int j = contentGrid.IDOffsetX; j < contentGrid.IDOffsetX + contentGrid.Width; j++)
            {
                TContent temp = contentGrid[j, i];
                if (temp != null)
                    temp.Release();
            }
        }

    }

    public int Width { get { return _grid.Width; } }
    public int Height { get { return _grid.Height; } }

}
