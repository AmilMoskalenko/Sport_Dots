using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InitSystem
{
    private readonly SceneData _sceneData;
    private readonly Config _config;

    public InitSystem(SceneData sceneData, Config config)
    {
        _sceneData = sceneData;
        _config = config;
    }

    public void Init()
    {
        FieldCorrect();
        _config.BiggestRight = 0;
        _config.BiggestDown = -1;
        _config.Board = new Board(new BoardSize(_config.Width, _config.Height));
        _config.Moves = new Dictionary<string, GameObject>();
        _config.MovesTree = new Dictionary<string, GameObject>();
        _config.Lines = new Dictionary<GameObject, List<GameObject>>();
        _config.Surrounds = new Dictionary<GameObject, List<GameObject>>();
        _config.Points = new Dictionary<GameObject, PlayerScore>();
        _config.FriendPoints = new Dictionary<GameObject, int>();
        _config.SurDots = new Dictionary<GameObject, List<Point>>();
        _config.Symbols = new Dictionary<GameObject, List<GameObject>>();
        _config.SecTrees = new Dictionary<GameObject, List<GameObject>>();
        _config.PlayerScore = new Dictionary<PointColor, int> { { PointColor.Blue, 0 }, { PointColor.Red, 0 } };
        _config.StartIndex = new List<int>();
        _config.CountIndex = -1;
        _config.NumberIndex = new List<int>();
        _config.Tool = Tool.Dot;
        _config.NumTool = 0;
        foreach (Transform f in _sceneData.FieldTransform)
            Object.Destroy(f.gameObject);
        foreach (Transform dot in _sceneData.DotsTransform)
            if (PhotonNetwork.IsConnected)
            {
                if (dot.gameObject.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                    PhotonNetwork.Destroy(dot.gameObject);
            }
            else
                Object.Destroy(dot.gameObject);
        foreach (Transform dotTree in _sceneData.MainTree)
            if (PhotonNetwork.IsConnected)
            {
                if (dotTree.gameObject.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                    PhotonNetwork.Destroy(dotTree.gameObject);
            }
            else
                Object.Destroy(dotTree.gameObject);
        foreach (Transform sym in _sceneData.Symbols)
            if (PhotonNetwork.IsConnected)
            {
                if (sym.gameObject.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                    PhotonNetwork.Destroy(sym.gameObject);
            }
            else
                Object.Destroy(sym.gameObject);
        foreach (Transform com in _sceneData.Comments.GetComponentInChildren<VerticalLayoutGroup>().transform)
            if (PhotonNetwork.IsConnected)
            {
                if (com.gameObject.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                    PhotonNetwork.Destroy(com.gameObject);
            }
            else
                Object.Destroy(com.gameObject);
        foreach (Transform line in _sceneData.Lines)
            Object.Destroy(line.gameObject);
        foreach (Transform surround in _sceneData.Surrounds)
            Object.Destroy(surround.gameObject);
        foreach (Transform sec in _sceneData.SecTrees)
            Object.Destroy(sec.gameObject);
        _sceneData.Player1Name.GetComponent<TextMeshProUGUI>().text = "Синий";
        _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text = "Красный";
        _sceneData.Player1Points.GetComponent<TextMeshProUGUI>().text = "0";
        _sceneData.Player2Points.GetComponent<TextMeshProUGUI>().text = "0";
        _sceneData.Pointer.position = new Vector3(-100, -100, 0);
        _sceneData.BlueCirclePhantom.position = new Vector3(-100, -100, 0);
        _sceneData.RedCirclePhantom.position = new Vector3(-100, -100, 0);
        var rt = _sceneData.MainTree.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(0, 430);
        rt.offsetMax = new Vector2(-700, 0);
        for (int i = 0; i < _config.Width; i++)
        {
            for (int j = 0; j < _config.Height; j++)
            {
                if (i != _config.Width - 1 && j != _config.Height - 1)
                    Object.Instantiate(_sceneData.CellPrefab, new Vector3(i, j, 0),
                        Quaternion.identity, _sceneData.FieldTransform);
                if (i == 0)
                {
                    var text = Object.Instantiate(Resources.Load((j + 1).ToString()),
                        new Vector3(i - 1, j - 0.5f, 0), Quaternion.identity, _sceneData.FieldTransform);
                    text.GetComponent<Collider2D>().enabled = false;
                }
                if (j == 0)
                {
                    var text = Object.Instantiate(Resources.Load((i + 1).ToString()),
                        new Vector3(i - 0.5f, j - 1, 0), Quaternion.identity, _sceneData.FieldTransform);
                    text.GetComponent<Collider2D>().enabled = false;
                }
            }
        }
    }

    private void FieldCorrect()
    {
        var cameraPosition = Camera.main.transform.position;
        if (_config.Width > _config.Height)
            cameraPosition.x = _config.Width * 0.82f;
        if (_config.Width == _config.Height)
            cameraPosition.x = _config.Height - 2f;
        if (_config.Width < _config.Height)
            cameraPosition.x = _config.Width * 0.82f;
        if (_config.Width > _config.Height)
            cameraPosition.y = _config.Height * 0.49f;
        if (_config.Width == _config.Height)
            cameraPosition.y = _config.Height * 0.44f;
        if (_config.Width < _config.Height)
            cameraPosition.y = _config.Height * 0.5f;
        var cameraSize = _config.Width > _config.Height ? _config.Width * 0.59f : _config.Height * 0.64f;
        if (_config.Width == 39 && _config.Height == 32)
        {
            cameraSize *= 0.86f;
            cameraPosition.x *= 1.016f;
            cameraPosition.y *= 0.956f;
        }
        else
            _config.CrossSize = 0;
        var dif = 2.777f - (_config.ScreenWidth / _config.ScreenHeight);
        if (_config.ScreenWidth / _config.ScreenHeight > 1.8)
            dif = 1f;
        cameraSize = dif * cameraSize;
        if (_config.Width == 39 && _config.Height == 32)
            cameraSize -= (dif - 1) * 8;
        Camera.main.orthographicSize = cameraSize;
        Camera.main.transform.position = cameraPosition;
    }
}
