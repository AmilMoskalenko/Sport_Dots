using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

public class SurroundSystem
{
    private readonly SceneData _sceneData;
    private readonly Config _config;

    public SurroundSystem(SceneData sceneData, Config config)
    {
        _sceneData = sceneData;
        _config = config;
    }
    
    public void Run(bool isBluePlayer)
    {
        var lastColor = isBluePlayer ? PointColor.Blue : PointColor.Red;
        var pos = _config.Current.transform.position;
        var x = (int)(pos.x + 0.5f);
        var y = (int)(pos.y + 0.5f);
        var coordinate = new Coordinate(x, y);
        var opponent = isBluePlayer ? PointColor.Red : PointColor.Blue;
        _config.Board.At(coordinate).PutStone(lastColor, lastColor);
        var offset = new InternalCoord(1, 0);
        var captured = false;
        var coord = InternalCoord.FromCoordinate(coordinate);
        for (var i = 0; i < 4; i++)
        {
            captured |= HandleCaptureAt(coord.Plus(offset), lastColor);
            offset.RotateClockwise();
        }
        if (captured)
            RecalculateScore();
        else
        {
            if (HandleCaptureAt(coord, opponent))
                RecalculateScore();
        }
    }
    
    private bool HandleCaptureAt(InternalCoord point, PointColor color)
    {
        var mask = new Mask(_config.Board.Size1, _config.Board.Size2);
        bool OwnColorPredicate(InternalCoord p) => _config.Board.At(p).Owner == color;
        if (OwnColorPredicate(point))
            return false;
        Flood(point, mask, OwnColorPredicate);
        if (!DoMaskCaptures(mask, color))
            return false;
        var bugStartPoint = LeftTopMaskBoundary(mask);
        bugStartPoint.X += 1;
        var startDirection = new InternalCoord(-1, 0);
        bool BoundaryPredicate(InternalCoord p) => mask.At(p.X, p.Y) == TopologicalKind.Boundary;
        var contour = NormalizeContour(RunBug(bugStartPoint, startDirection, BoundaryPredicate));
        DrawChain(contour, color);
        var territoryMask = new Mask(mask.Size1, mask.Size2);
        foreach (var contourPoint in contour)
            territoryMask.Set(contourPoint.X, contourPoint.Y, TopologicalKind.Boundary);
        Flood(point, territoryMask, _ => false);
        CaptureByMask(territoryMask, color);
        return true;
    }
    
    private void Flood(InternalCoord startPoint, Mask mask, Func<InternalCoord, bool> wallFunction)
    {
        mask.Set(startPoint.X, startPoint.Y, TopologicalKind.Interior);
        var toProcess = new Stack<InternalCoord>();
        toProcess.Push(startPoint);
        while (toProcess.Count > 0)
        {
            var point = toProcess.Pop();
            var offset = new InternalCoord(1, 0);
            for (int i = 0; i < 4; i++)
            {
                var neighbour = point.Plus(offset);
                if (mask.At(neighbour.X, neighbour.Y) == TopologicalKind.Exterior)
                {
                    if (wallFunction(neighbour))
                    {
                        mask.Set(neighbour.X, neighbour.Y, TopologicalKind.Boundary);
                    }
                    else
                    {
                        mask.Set(neighbour.X, neighbour.Y, TopologicalKind.Interior);
                        toProcess.Push(neighbour);
                    }
                }
                offset.RotateClockwise();
            }
        }
    }
    
    private bool DoMaskCaptures(Mask mask, PointColor color)
    {
        if (mask.At(1, 1) == TopologicalKind.Interior)
            return false;
        for (int x = 2; x < mask.Size1; x++)
        {
            for (int y = 2; y < mask.Size2; y++)
            {
                var coordinate = new InternalCoord(x, y);
                var point = _config.Board.At(coordinate);
                if (mask.At(x, y) == TopologicalKind.Interior &&
                    point.Owner != PointColor.None && point.Owner != color)
                    return true;
            }
        }
        return false;
    }
    
    private InternalCoord LeftTopMaskBoundary(Mask mask)
    {
        for (int x = 0; x < mask.Size1; x++)
        {
            for (int y = 0; y < mask.Size2; y++)
            {
                if (mask.At(x, y) == TopologicalKind.Boundary)
                    return new InternalCoord(x, y);
            }
        }
        throw new InvalidOperationException("No boundary points in mask");
    }

    private List<InternalCoord> NormalizeContour(List<InternalCoord> contour)
    {
        var normalizedContour = new List<InternalCoord>();
        var contourPoints = new HashSet<string>();
        string Key(InternalCoord point) => point.X + ";" + point.Y;
        foreach (var point in contour)
        {
            var pointKey = Key(point);
            while (contourPoints.Contains(pointKey))
            {
                var redundantPoint = normalizedContour[^1];
                if (!contourPoints.Contains(Key(redundantPoint)))
                    throw new InvalidOperationException("Assertion failed: redundantPoint is not in contourPoints");
                normalizedContour.RemoveAt(normalizedContour.Count - 1);
                contourPoints.Remove(Key(redundantPoint));
            }
            normalizedContour.Add(point);
            contourPoints.Add(pointKey);
        }
        return normalizedContour;
    }
    
    private List<InternalCoord> RunBug(InternalCoord startPoint, InternalCoord startDirection, Func<InternalCoord, bool> wallFunction)
    {
        var point = startPoint.Clone();
        var direction = startDirection.Clone();
        var contour = new List<InternalCoord>();
        do
        {
            var pointInFront = point.Plus(direction);
            if (wallFunction(pointInFront))
            {
                contour.Add(pointInFront);
                direction.RotateClockwise();
            }
            else
            {
                point = pointInFront;
                direction.RotateCounterClockwise();
            }
        } 
        while (!point.Equals(startPoint) || !direction.Equals(startDirection));
        return contour;
    }
    
    private void CaptureByMask(Mask mask, PointColor color)
    {
        var points = 0;
        var dot = _config.Current;
        var friendPoints = 0;
        for (int x = 2; x < mask.Size1 - 2; x++)
        {
            for (int y = 2; y < mask.Size2 - 2; y++)
            {
                if (mask.At(x, y) == TopologicalKind.Interior)
                {
                    var point = _config.Board.At(new InternalCoord(x, y));
                    if (point.Owner != point.Stone && point.Stone == color)
                        friendPoints++;
                    point.Owner = color;
                    if (point.Owner != PointColor.None && point.Owner != point.Stone)
                    {
                        if (point.Stone != PointColor.None)
                            points++;
                        if (dot.CompareTag("Blue") && point.Stone == PointColor.Red ||
                            dot.CompareTag("Red") && point.Stone == PointColor.Blue)
                        {
                            if (_config.SurDots.ContainsKey(dot))
                                _config.SurDots[dot].Add(point);
                            else
                                _config.SurDots.Add(dot, new List<Point> { point });
                        }
                    }
                }
            }
        }
        if (_config.Moves.Count > 0)
        {
            if (_config.Points.ContainsKey(dot))
                _config.Points[dot] = new PlayerScore(color, points);
            else
                _config.Points.Add(dot, new PlayerScore(color, points));
            if (_config.FriendPoints.ContainsKey(dot))
                _config.FriendPoints[dot] = friendPoints;
            else
                _config.FriendPoints.Add(dot, friendPoints);
        }
    }
    
    private void RecalculateScore()
    {
        _config.PlayerScore = new Dictionary<PointColor, int> {{PointColor.Blue, 0}, {PointColor.Red, 0}};
        for (int x = 0; x < _config.Board.Width; x++)
        {
            for (int y = 0; y < _config.Board.Height; y++)
            {
                var point = _config.Board.At(new Coordinate(x, y));
                var owner = point.Owner;
                var stone = point.Stone;
                if (owner != PointColor.None && stone != PointColor.None && owner != stone)
                    _config.PlayerScore[owner]++;
            }
        }
        _sceneData.Player1Points.GetComponent<TextMeshProUGUI>().text = _config.PlayerScore[PointColor.Blue].ToString();
        _sceneData.Player2Points.GetComponent<TextMeshProUGUI>().text = _config.PlayerScore[PointColor.Red].ToString();
    }

    private void DrawChain(List<InternalCoord> contour, PointColor color)
    {
        var chain = contour.Select(coord => new Vector2(coord.X - 2.5f, coord.Y - 2.5f)).ToList();
        var line = Object.Instantiate(color == PointColor.Blue ? _sceneData.BlueLine : _sceneData.RedLine, _sceneData.Lines);
        var lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.positionCount = chain.Count;
        for (int i = 0; i < chain.Count; i++)
            lineRenderer.SetPosition(i, chain[i]);
        var surround = MakeCollider2D.Create(chain.ToArray(), _sceneData.Surround);
        surround.transform.parent = _sceneData.Surrounds;
        surround.GetComponent<Renderer>().material = color == PointColor.Blue ? _sceneData.BlueMaterial : _sceneData.RedMaterial;
        if (_config.Moves.Count > 0)
        {
            var dot = _config.Current;
            dot.layer = 3;
            if (_config.Lines.ContainsKey(dot))
                _config.Lines[dot].Add(line);
            else
                _config.Lines.Add(dot, new List<GameObject>{line});
            if (_config.Surrounds.ContainsKey(dot))
                _config.Surrounds[dot].Add(surround);
            else
                _config.Surrounds.Add(dot, new List<GameObject>{surround});
        }
    }
}

public class Coordinate : LibPoint
{
    public Coordinate(int x, int y) : base(x, y) { }
}

public class InternalCoord : LibPoint
{
    public InternalCoord(int x, int y) : base(x, y) { }

    public static InternalCoord FromCoordinate(Coordinate coordinate) => new(2 + coordinate.X, 2 + coordinate.Y);
}

public class LibPoint
{
    public int X;
    public int Y;

    protected LibPoint(int x, int y) { X = x; Y = y; }

    public void RotateClockwise()
    {
        var tempX = Y;
        var tempY = -X;
        X = tempX;
        Y = tempY;
    }

    public void RotateCounterClockwise()
    {
        var tempX = -Y;
        var tempY = X;
        X = tempX;
        Y = tempY;
    }

    public InternalCoord Clone() => new(X, Y);

    public bool Equals(LibPoint other) => X == other.X && Y == other.Y;

    public InternalCoord Plus(LibPoint other) => new(X + other.X, Y + other.Y);
}

public enum PointColor
{
    None = 0,
    Blue = 1,
    Red = 2
}

public class Mask : Array2D<int>
{
    public Mask(int size1, int size2) : base(size1, size2, (_, _) => TopologicalKind.Exterior)
    {
        for (int x = 0; x < size1; x++)
        {
            Set(x, 0, TopologicalKind.Interior);
            Set(x, size2 - 1, TopologicalKind.Interior);
        }
        for (int y = 0; y < size2; y++)
        {
            Set(0, y, TopologicalKind.Interior);
            Set(size1 - 1, y, TopologicalKind.Interior);
        }
    }
}

public static class TopologicalKind
{
    public const int Exterior = 1;
    public const int Boundary = 2;
    public const int Interior = 3;
}

public class Array2D<T>
{
    private readonly T[] _data;

    protected Array2D(int size1, int size2, Func<int, int, T> valueFunction)
    {
        _data = new T[size1 * size2];
        Size1 = size1;
        Size2 = size2;
        for (int i = 0; i < size1; i++)
        {
            for (int j = 0; j < size2; j++)
                _data[_index(i, j)] = valueFunction(i, j);
        }
    }

    private int _index(int i, int j) => i * Size2 + j;

    public int Size1 { get; }

    public int Size2 { get; }

    public T At(int i, int j) => _data[_index(i, j)];

    public void Set(int i, int j, T v) => _data[_index(i, j)] = v;
}

public class Board : Array2D<Point>
{
    private readonly BoardSize _size;

    public Board(BoardSize size) : base(2 + size.Width + 2, 2 + size.Height + 2, 
        (_, _) => new Point()) => _size = size;

    private InternalCoord ConvertCoordinate(LibPoint anyCoordinate)
    {
        if (anyCoordinate is Coordinate coordinate)
            return InternalCoord.FromCoordinate(coordinate);
        return (InternalCoord)anyCoordinate;
    }

    public Point At(LibPoint anyCoordinate)
    {
        var internalCoord = ConvertCoordinate(anyCoordinate);
        return base.At(internalCoord.X, internalCoord.Y);
    }

    public int Width => _size.Width;

    public int Height => _size.Height;
}

public class BoardSize
{
    public int Width { get; }
    public int Height { get; }

    public BoardSize(int width, int height)
    {
        Width = width;
        Height = height;
    }
}

public class Point
{
    public PointColor Stone { get; private set; }
    public PointColor Owner { get; set; }
    public string Key { get; set; }

    public Point()
    {
        Stone = PointColor.None;
        Owner = PointColor.None;
    }

    public void PutStone(PointColor colorO, PointColor colorS, string key = null)
    {
        Owner = colorO;
        Stone = colorS;
        if (key != null)
            Key = key;
    }
}