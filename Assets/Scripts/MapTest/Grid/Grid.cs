using System;

public class Grid<T>
{

    private T[] _grid;
    private int _width;
    private int _height;
    private int _idOffsetX;
    private int _idOffsetY;

    public int Height { get { return _height; } }
    public int Width { get { return _width; } }
    public int IDOffsetX { get { return _idOffsetX; } }
    public int IDOffsetY { get { return _idOffsetY; } }

    public T this[int x, int y]
    {
        get
        {
            x -= _idOffsetX;
            y -= _idOffsetY;
            return GetValue(x, y);
        }
        set
        {
            x -= _idOffsetX;
            y -= _idOffsetY;
            SetValue(x, y, value);
        }
    }

    public Grid(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new T[width * height];
    }

    public Grid(Grid<T> value)
        : this(value.Width, value.Height)
    {
        _idOffsetX = value._idOffsetX;
        _idOffsetY = value._idOffsetY;
        for (int i = 0; i < value._grid.Length; i++)
        {
            _grid[i] = value._grid[i];
        }
    }

    public Grid(T[] values, int width, int height)
        : this(width, height)
    {
        int end = Math.Min(values.Length, _grid.Length);
        for (int i = 0; i < end; i++)
        {
            _grid[i] = values[i];
        }
    }

    public void Shift(int deltaX, int deltaY)
    {
        Grid<T> grid = new Grid<T>(Width, Height);
        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                grid.SetValue(j + deltaX, i + deltaY, GetValue(j, i));
            }
        }
        _grid = grid._grid;
        _idOffsetX = _idOffsetX - deltaX;
        _idOffsetY = _idOffsetY - deltaY;
    }

    public void ShiftIDs(int deltaX, int deltaY)
    {
        _idOffsetX += deltaX;
        _idOffsetY += deltaY;
    }

    public void Apply(Grid<T> grid)
    {
        _grid = grid._grid;
        _idOffsetX = grid._idOffsetX;
        _idOffsetY = grid._idOffsetY;
    }

    public static Grid<T> CreateEmpty<U>(Grid<U> grid)
    {
        return new Grid<T>(grid.Width, grid.Height);
    }

    private T GetValue(int realX, int realY)
    {
        if (realX >= Width || realY >= Height || realX < 0 || realY < 0)
            return default(T);
        return _grid[realY * Width + realX];

    }

    private void SetValue(int realX, int realY, T value)
    {
        if (realX >= Width || realY >= Height || realX < 0 || realY < 0)
            return;
        _grid[realY * Width + realX] = value;

    }

    public void SetIDOffset(int x, int y)
    {
        _idOffsetX = x;
        _idOffsetY = y;
    }

    public override string ToString()
    {
        string message = "";
        for (int i = 0; i < Height; i++)
        {

            for (int j = 0; j < Width; j++)
            {
                message += GetValue(j, i);
                if (j < Width - 1)
                    message += " ";
            }
            if (i < Height - 1)
                message += Environment.NewLine;
        }
        return message;
    }

    public static implicit operator string(Grid<T> grid)
    {
        return grid.ToString();
    }
}
