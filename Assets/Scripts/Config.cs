using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Config : ScriptableObject
{
    [HideInInspector]
    public string PlayerName;
    [HideInInspector]
    public float ScreenWidth;
    [HideInInspector]
    public float ScreenHeight;
    [HideInInspector]
    public int Width;
    [HideInInspector]
    public int Height;
    [HideInInspector]
    public Sprite BlueNumber;
    [HideInInspector]
    public Sprite RedNumber;
    [HideInInspector]
    public bool IsBluePlayer;
    [HideInInspector]
    public int PlayerTurnState;
    [HideInInspector]
    public bool IsCoach;
    [HideInInspector]
    public bool IsLock;
    [HideInInspector]
    public LineRenderer BrushLine;
    [HideInInspector]
    public Vector2 BrushLastPos;
    [HideInInspector]
    public MeshCollider BrushCollider;
    [HideInInspector]
    public float SecondsPlayer1;
    [HideInInspector]
    public float SecondsPlayer2;
    [HideInInspector]
    public Vector3 Big;
    [HideInInspector]
    public Vector3 Small;
    [HideInInspector]
    public GameObject Current;
    [HideInInspector]
    public int DotSize;
    [HideInInspector]
    public int CrossSize;
    [HideInInspector]
    public bool IsPointer;
    [HideInInspector]
    public int BiggestRight;
    [HideInInspector]
    public int BiggestDown;
    [HideInInspector]
    public bool IsSentComment;
    [HideInInspector]
    public float CommentHeight;
    [HideInInspector]
    public bool IsSentChat;
    [HideInInspector]
    public float ChatHeight;
    public SurroundSystem SurroundSystem;
    public TreeSystem TreeSystem;
    public SgfSystem SgfSystem;
    public ChatSystem ChatSystem;
    public InitSystem InitSystem;
    public Board Board;
    public Dictionary<string, GameObject> Moves;
    public Dictionary<string, GameObject> MovesTree;
    public Dictionary<GameObject, List<GameObject>> Lines;
    public Dictionary<GameObject, List<GameObject>> Surrounds;
    public Dictionary<GameObject, PlayerScore> Points;
    public Dictionary<GameObject, int> FriendPoints;
    public Dictionary<GameObject, List<Point>> SurDots;
    public Dictionary<GameObject, List<GameObject>> Symbols;
    public Dictionary<GameObject, List<GameObject>> SecTrees;
    public Dictionary<PointColor, int> PlayerScore;
    [HideInInspector]
    public List<int> StartIndex;
    [HideInInspector]
    public int CountIndex;
    [HideInInspector]
    public List<int> NumberIndex;
    [HideInInspector]
    public Tool Tool;
    [HideInInspector]
    public int NumTool;
    [HideInInspector]
    public bool IsGroup;
    [HideInInspector]
    public bool IsNoDots;
    [HideInInspector]
    public bool IsNoBlueDot;
    [HideInInspector]
    public bool IsNoRedDot;
    [HideInInspector]
    public bool IsNoTools;
    [HideInInspector]
    public bool IsNoChat;
    [HideInInspector]
    public List<string> BanList;
}

public class PlayerScore
{
    public PointColor Color;
    public int Score;

    public PlayerScore(PointColor color, int score)
    {
        Color = color;
        Score = score;
    }
}

public enum Tool
{
    Dot,
    Num,
    Cross,
    Line,
    Arrow,
    Brush,
    Eraser
}