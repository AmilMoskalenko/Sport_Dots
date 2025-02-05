using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SgfSystem
{
    private readonly SceneData _sceneData;
    private readonly Config _config;

    private readonly char[] alphabet = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i',
        'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M' };

    private string _sgfTree;
    private int _lastMainTree;

    public SgfSystem(SceneData sceneData, Config config)
    {
        _sceneData = sceneData;
        _config = config;
    }

    private int SgfConvert(char value, int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < 39; i++)
                if (value == alphabet[i])
                    return i;
        }
        if (index == 1)
        {
            for (int i = 0; i < 32; i++)
                if (value == alphabet[i])
                    return 31 - i;
        }
        return 0;
    }

    private char SgfConvertInvert(int value, int index)
    {
        if (index == 0)
        {
            for (int i = 0; i < 39; i++)
                if (value == i)
                    return alphabet[i];
        }
        if (index == 1)
        {
            for (int i = 0; i < 32; i++)
                if (value == 31 - i)
                    return alphabet[i];
        }
        return alphabet[0];
    }

    public void GetSgfSize(SgfTree sgfTree)
    {
        foreach (var (key, value) in sgfTree.Data)
        {
            if (key == "SZ")
            {
                var sizes = value[0].Split(":");
                _config.Width = int.Parse(sizes[0]);
                _config.Height = int.Parse(sizes[1]);
            }
        }
    }

    public void NewUploadSgfTree(string text)
    {
        var sgfTree = SgfParser.ParseTree(text);
        GetSgfSize(sgfTree);
        FieldSize();
        _config.CrossSize = 0;
        InitUpdate();
        UploadSgfTree(sgfTree);
        ClosePanel();
    }

    public void NewSaveSgfTree()
    {
        ClosePanel();
    }

    private void ClosePanel()
    {
        _sceneData.RoomPanel.SetActive(false);
        _sceneData.ToolsPanel.SetActive(false);
        _sceneData.SettingsPanel.SetActive(false);
        _sceneData.InfoPanel.SetActive(false);
        _sceneData.DarkPanel.SetActive(false);
        _config.PlayerTurnState = _config.IsBluePlayer ? 1 : 2;
    }

    private void FieldSize()
    {
        if (PhotonNetwork.IsConnected)
        {
            var raiseEventOptions = RaiseEventOptions.Default;
            var sendOptions = SendOptions.SendReliable;
            PhotonNetwork.RaiseEvent(19, _config.Width, raiseEventOptions, sendOptions);
            PhotonNetwork.RaiseEvent(20, _config.Height, raiseEventOptions, sendOptions);
            var roomProps = new Hashtable { ["Width"] = _config.Width, ["Height"] = _config.Height };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
    }

    private void Initialize()
    {
        _config.IsNoDots = false;
        _config.IsNoBlueDot = true;
        _config.IsNoRedDot = true;
        _config.IsNoTools = false;
        _config.IsNoChat = false;
        _config.IsLock = false;
        _sceneData.LeftFast.SetActive(true);
        _sceneData.LeftButton.SetActive(true);
        _sceneData.RightButton.SetActive(true);
        _sceneData.RightFast.SetActive(true);
        _sceneData.RightAuto.SetActive(true);
        _sceneData.Num.SetActive(true);
        _sceneData.Cross.SetActive(true);
        _sceneData.Line.SetActive(true);
        _sceneData.Arrow.SetActive(true);
        _sceneData.Brush.SetActive(true);
        _sceneData.Eraser.SetActive(true);
        _config.InitSystem.Init();
        CloseFrame();
        _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(true);
        _config.IsBluePlayer = true;
        _config.PlayerTurnState = 1;
        _sceneData.LeftButton.GetComponent<Button>().interactable = false;
        _sceneData.RightButton.GetComponent<Button>().interactable = false;
        _sceneData.LeftFast.GetComponent<Button>().interactable = false;
        _sceneData.RightFast.GetComponent<Button>().interactable = false;
        _sceneData.RightAuto.GetComponent<Button>().interactable = false;
        if (_config.CrossSize == 1)
        {
            CrossDot(true, 19, 16);
            CrossDot(true, 20, 15);
            CrossDot(false, 19, 15);
            CrossDot(false, 20, 16);
        }
        if (_config.CrossSize == 2)
        {
            CrossDot(true, 11, 21);
            CrossDot(true, 12, 20);
            CrossDot(true, 26, 21);
            CrossDot(true, 27, 20);
            CrossDot(true, 11, 11);
            CrossDot(true, 12, 10);
            CrossDot(true, 26, 11);
            CrossDot(true, 27, 10);
            CrossDot(false, 12, 21);
            CrossDot(false, 11, 20);
            CrossDot(false, 27, 21);
            CrossDot(false, 26, 20);
            CrossDot(false, 11, 10);
            CrossDot(false, 12, 11);
            CrossDot(false, 26, 10);
            CrossDot(false, 27, 11);
        }
    }

    private void InitUpdate()
    {
        if (PhotonNetwork.IsConnected)
        {
            var raiseEventOptions = RaiseEventOptions.Default;
            var sendOptions = SendOptions.SendReliable;
            PhotonNetwork.RaiseEvent(11, true, raiseEventOptions, sendOptions);
        }
        foreach (Transform p in _sceneData.ListNames)
        {
            var v = p.GetChild(1);
            v.GetChild(0).GetChild(0).gameObject.SetActive(false);
            v.GetChild(1).GetChild(0).gameObject.SetActive(false);
            v.GetChild(2).GetChild(0).gameObject.SetActive(false);
            v.GetChild(3).GetChild(0).gameObject.SetActive(false);
            v.GetChild(4).GetChild(0).gameObject.SetActive(false);
        }
        Initialize();
    }

    private void CloseFrame()
    {
        _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.BlueDot.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.RedDot.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Line.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Cross.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Arrow.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Brush.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Num.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.Eraser.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.BlueCirclePhantom.position = new Vector3(-100, -100, 0);
        _sceneData.RedCirclePhantom.position = new Vector3(-100, -100, 0);
    }

    private void CrossDot(bool isBlue, int x, int y)
    {
        var prefab = isBlue ? _sceneData.BlueCircle : _sceneData.RedCircle;
        var position = new Vector3(x - 0.5f, y - 0.5f, 0);
        var rotation = Quaternion.identity;
        var dot = PhotonNetwork.NickName == string.Empty ?
            Object.Instantiate(prefab, position, rotation) :
            PhotonNetwork.Instantiate(prefab.name, position, rotation);
        dot.transform.parent = _sceneData.DotsTransform;
        _config.Current = dot;
        _config.SurroundSystem.Run(isBlue);
        var raiseEventOptions = RaiseEventOptions.Default;
        var sendOptions = SendOptions.SendReliable;
        PhotonNetwork.RaiseEvent(12, _config.Current.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
        PhotonNetwork.RaiseEvent(5, isBlue, raiseEventOptions, sendOptions);
    }

    public void UploadSgfTree(SgfTree sgfTree)
    {
        foreach (var (key, value) in sgfTree.Data)
        {
            if (key == "AB" || key == "AW")
            {
                var prefab = key == "AB" ? _sceneData.BlueCircle : _sceneData.RedCircle;
                foreach (var s in value)
                {
                    var point = s.ToCharArray();
                    var x = SgfConvert(point[0], 0);
                    var y = SgfConvert(point[1], 1);
                    var position = new Vector3(x - 0.5f, y - 0.5f, 0);
                    var rotation = Quaternion.identity;
                    var dot = PhotonNetwork.NickName == string.Empty ?
                        Object.Instantiate(prefab, position, rotation) :
                        PhotonNetwork.Instantiate(prefab.name, position, rotation);
                    dot.transform.parent = _sceneData.DotsTransform;
                    _config.Current = dot;
                    _config.SurroundSystem.Run(key == "AB");
                }
            }
            if (key == "PB")
            {
                _sceneData.Player1Name.GetComponent<TextMeshProUGUI>().text = value[0];
                if (PhotonNetwork.NickName != string.Empty)
                {
                    var raiseEventOptions = RaiseEventOptions.Default;
                    var sendOptions = SendOptions.SendReliable;
                    PhotonNetwork.RaiseEvent(17, value[0], raiseEventOptions, sendOptions);
                }
            }
            if (key == "PW")
            {
                _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text = value[0];
                if (PhotonNetwork.NickName != string.Empty)
                {
                    var raiseEventOptions = RaiseEventOptions.Default;
                    var sendOptions = SendOptions.SendReliable;
                    PhotonNetwork.RaiseEvent(18, value[0], raiseEventOptions, sendOptions);
                }
            }
        }
        _lastMainTree = 0;
        for (int i = 0; i < sgfTree.Children.Length; i++)
            DecodeSgf(sgfTree.Children[i], _config.StartIndex, i, _config.NumberIndex);
        var k = $".{_lastMainTree}.";
        _config.TreeSystem.ShowMoves(k, true);
        _sceneData.LeftButton.GetComponent<Button>().interactable = true;
        _sceneData.RightButton.GetComponent<Button>().interactable = false;
        _sceneData.LeftFast.GetComponent<Button>().interactable = true;
        _sceneData.RightFast.GetComponent<Button>().interactable = false;
        _sceneData.RightAuto.GetComponent<Button>().interactable = false;
    }

    private void DecodeSgf(SgfTree point, List<int> startI, int countI, List<int> numberI)
    {
        var dataPoint = point.Data.First();
        var value = dataPoint.Value[0].ToCharArray();
        var x = SgfConvert(value[0], 0);
        var y = SgfConvert(value[1], 1);
        var prefab = dataPoint.Key == "B" ? _sceneData.BlueCircle : _sceneData.RedCircle;
        var prefabTree = dataPoint.Key == "B" ? _sceneData.BlueCircleTree : _sceneData.RedCircleTree;
        _config.StartIndex = startI;
        string start = null;
        foreach (var s in _config.StartIndex)
            start += $"{s},";
        _config.CountIndex = countI;
        if (_config.CountIndex > _config.BiggestRight)
            _config.BiggestRight = _config.CountIndex;
        var dotPosTreeY = 0;
        foreach (var (k, _) in _config.MovesTree)
        {
            var kA = k.Split('.');
            var s = new List<int>();
            if (kA[0] != string.Empty)
                s = kA[0].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            var c = int.Parse(kA[1]);
            var n = new List<int>();
            if (kA[2] != string.Empty)
                n = kA[2].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            if (c == _config.CountIndex)
            {
                if (s.Count > 0)
                    dotPosTreeY++;
            }
        }
        _config.NumberIndex = numberI;
        string number = null;
        foreach (var n in _config.NumberIndex)
            number += $"{n},";
        if (start == null && _config.CountIndex > _lastMainTree && number == null)
            _lastMainTree = _config.CountIndex;
        var key = $"{start}.{_config.CountIndex}.{number}";
        var position = new Vector3(x - 0.5f, y - 0.5f, 0);
        var rotation = Quaternion.identity;
        var dot = PhotonNetwork.NickName == string.Empty ?
            Object.Instantiate(prefab, position, rotation) :
            PhotonNetwork.Instantiate(prefab.name, position, rotation);
        dot.transform.parent = _sceneData.DotsTransform;
        var raiseEventOptions = RaiseEventOptions.Default;
        var sendOptions = SendOptions.SendReliable;
        if (_config.Current)
        {
            if (_config.SecTrees.ContainsKey(_config.Current))
                _config.SecTrees[_config.Current].Add(dot);
            else
                _config.SecTrees.Add(_config.Current, new List<GameObject> { dot });
            if (PhotonNetwork.NickName != string.Empty)
            {
                PhotonNetwork.RaiseEvent(12, _config.Current.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                PhotonNetwork.RaiseEvent(13, dot.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
            }
        }
        _config.Current = dot;
        _config.Moves.Add(key, dot);
        var dotTree = PhotonNetwork.NickName == string.Empty ?
            Object.Instantiate(prefabTree) :
            PhotonNetwork.Instantiate(prefabTree.name, Vector3.zero, Quaternion.identity);
        dotTree.transform.parent = _sceneData.MainTree;
        dotTree.GetComponent<RectTransform>().localScale = Vector3.one;
        var posTreeY = _config.StartIndex.Count + _config.NumberIndex.Sum();
        var cCount = 0;
        var sCount = 0;
        if (dotPosTreeY >= posTreeY && _config.StartIndex.Count > 0)
        {
            posTreeY = dotPosTreeY + 1;
            sCount++;
        }
        var posTree = new Vector2(80 * (_config.CountIndex + 1), -80 * (posTreeY + 1));
        dotTree.GetComponent<RectTransform>().anchoredPosition = posTree;
        dotTree.transform.GetChild(0).gameObject.SetActive(true);
        var rt = _sceneData.MainTree.GetComponent<RectTransform>();
        foreach (var (k, _) in _config.Moves)
        {
            var kA = k.Split('.');
            var s = new List<int>();
            if (kA[0] != string.Empty)
                s = kA[0].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            var c = int.Parse(kA[1]);
            var n = new List<int>();
            if (kA[2] != string.Empty)
                n = kA[2].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            if (c >= _config.CountIndex)
                cCount++;
            if (s.Count() + n.Sum() >= posTreeY)
                sCount++;
        }
        if (cCount == 1)
            rt.offsetMax = new Vector2(rt.offsetMax.x + 80, rt.offsetMax.y);
        if (sCount == 1)
        {
            rt.offsetMin = new Vector2(rt.offsetMin.x, rt.offsetMin.y - 80);
            _config.BiggestDown++;
        }
        dotTree.GetComponentInChildren<TextMeshProUGUI>().text = (_config.CountIndex + 1).ToString();
        dotTree.GetComponent<Button>().onClick.AddListener(() => { if (!_config.IsNoDots) _config.TreeSystem.ShowMoves(key); });
        _config.MovesTree.Add(key, dotTree);
        _config.SurroundSystem.Run(dataPoint.Key == "B");
        if (PhotonNetwork.NickName != string.Empty)
        {
            PhotonNetwork.RaiseEvent(1, key, raiseEventOptions, sendOptions);
            PhotonNetwork.RaiseEvent(2, dot.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
            PhotonNetwork.RaiseEvent(3, dotTree.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
            PhotonNetwork.RaiseEvent(5, _config.IsBluePlayer, raiseEventOptions, sendOptions);
        }
        _config.TreeSystem.ShowMoves(key);
        if (point.Data.Count > 1)
        {
            foreach (var data in point.Data)
            {
                if (data.Equals(dataPoint)) continue;
                foreach (var v in data.Value)
                {
                    var valueS = v.ToCharArray();
                    var xS = 0;
                    if (valueS.Length > 0)
                        xS = SgfConvert(valueS[0], 0);
                    var yS = 0;
                    if (valueS.Length > 1)
                        yS = SgfConvert(valueS[1], 1);
                    var posS = new Vector3(xS - 0.5f, yS - 0.5f, 0);
                    var rotS = Quaternion.identity;
                    GameObject tool = null;
                    if (data.Key == "LB")
                    {
                        var num = v.Split(':')[1];
                        if (!int.TryParse(num, out int n)) continue;
                        tool = PhotonNetwork.NickName == string.Empty ?
                            (GameObject)Object.Instantiate(Resources.Load(num), posS, rotS) :
                            PhotonNetwork.Instantiate(num, posS, rotS);
                        tool.transform.localScale = new Vector3(2, 2, 1);
                    }
                    if (data.Key == "MA")
                        tool = PhotonNetwork.NickName == string.Empty ?
                            (GameObject)Object.Instantiate(Resources.Load("Cross"), posS, rotS) :
                            PhotonNetwork.Instantiate("Cross", posS, rotS);
                    if (tool)
                        tool.transform.parent = _sceneData.Symbols;
                    if (data.Key == "C")
                    {
                        tool = PhotonNetwork.NickName == string.Empty ?
                            Object.Instantiate(_sceneData.MessagePrefab) :
                            PhotonNetwork.Instantiate(_sceneData.MessagePrefab.name, Vector3.zero, Quaternion.identity);
                        tool.transform.parent = _sceneData.Comments.GetComponentInChildren<VerticalLayoutGroup>().transform;
                        tool.GetComponent<RectTransform>().localScale = Vector3.one;
                        tool.GetComponent<TextMeshProUGUI>().text = v;
                        dotTree.transform.GetChild(2).gameObject.SetActive(true);
                    }
                    if (!tool) continue;
                    if (_config.Symbols.ContainsKey(dot))
                        _config.Symbols[dot].Add(tool);
                    else
                        _config.Symbols.Add(dot, new List<GameObject> { tool });
                    if (PhotonNetwork.NickName != string.Empty)
                    {
                        PhotonNetwork.RaiseEvent(6, dot.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                        PhotonNetwork.RaiseEvent(7, tool.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                        if (data.Key == "C")
                            PhotonNetwork.RaiseEvent(10, v, raiseEventOptions, sendOptions);
                    }
                }
            }
        }
        if (point.Children.Length > 0)
        {
            for (int i = 0; i < point.Children.Length; i++)
            {
                if (i == 0)
                {
                    _config.CountIndex++;
                    if (_config.StartIndex.Count > 0)
                        _config.NumberIndex.Add(0);
                }
                if (i > 0)
                {
                    if (point.Data.First().Key != point.Children[i].Data.First().Key &&
                        point.Children[i - 1].Data.First().Key == point.Children[i].Data.First().Key)
                    {
                        string st = null;
                        foreach (var s in startI)
                            st += $"{s},";
                        string num = null;
                        foreach (var n in numberI)
                            num += $"{n},";
                        var k = $"{st}.{countI}.{num}";
                        _config.TreeSystem.ShowMoves(k);
                        _config.StartIndex.Add(_config.CountIndex);
                        _config.CountIndex++;
                        _config.NumberIndex.Add(i - 1);
                    }
                    if (point.Data.First().Key == point.Children[i].Data.First().Key ||
                        point.Children[i - 1].Data.First().Key != point.Children[i].Data.First().Key)
                    {
                        _config.CountIndex++;
                        if (_config.StartIndex.Count > 0)
                            _config.NumberIndex.Add(0);
                    }
                }
                DecodeSgf(point.Children[i], _config.StartIndex, _config.CountIndex, _config.NumberIndex);
            }
        }
    }

    public string DownloadSgfTree()
    {
        _sgfTree = $"(;SZ[{_config.Width}:{_config.Height}]";
        EncodeSgf(_config.Moves.First().Value);
        _sgfTree += ")";
        return _sgfTree;
    }

    private void EncodeSgf(GameObject v)
    {
        var name = v.CompareTag("Blue") ? "B" : "W";
        var x = SgfConvertInvert((int)(v.transform.position.x + 0.5f), 0);
        var y = SgfConvertInvert((int)(v.transform.position.y + 0.5f), 1);
        _sgfTree += $";{name}[{x}{y}]";
        if (_config.Symbols.ContainsKey(v))
        {
            if (_config.Symbols[v].Any(s => s.CompareTag("Num")))
            {
                var nameTool = "LB";
                _sgfTree += $"{nameTool}";
                foreach (var sym in _config.Symbols[v])
                {
                    if (sym.CompareTag("Num"))
                    {
                        var xTool = SgfConvertInvert((int)(sym.transform.position.x + 0.5f), 0);
                        var yTool = SgfConvertInvert((int)(sym.transform.position.y + 0.5f), 1);
                        _sgfTree += $"[{xTool}{yTool}:{sym.name.Split('(')[0]}]";
                    }
                }
            }
            if (_config.Symbols[v].Any(s => s.name == "Cross(Clone)"))
            {
                var nameTool = "MA";
                _sgfTree += $"{nameTool}";
                foreach (var sym in _config.Symbols[v])
                {
                    if (sym.name == "Cross(Clone)")
                    {
                        var xTool = SgfConvertInvert((int)(sym.transform.position.x + 0.5f), 0);
                        var yTool = SgfConvertInvert((int)(sym.transform.position.y + 0.5f), 1);
                        _sgfTree += $"[{xTool}{yTool}]";
                    }
                }
            }
            if (_config.Symbols[v].Any(s => s.CompareTag("Untagged")))
            {
                var nameTool = "C";
                _sgfTree += $"{nameTool}";
                foreach (var sym in _config.Symbols[v])
                {
                    if (sym.CompareTag("Untagged"))
                    {
                        _sgfTree += $"[{sym.GetComponent<TextMeshProUGUI>().text}]";
                    }
                }
            }
        }
        if (_config.SecTrees.ContainsKey(v))
        {
            foreach (var secTree in _config.SecTrees[v])
            {
                if (_config.SecTrees[v].Count > 1)
                    _sgfTree += "(";
                EncodeSgf(secTree);
                if (_config.SecTrees[v].Count > 1)
                    _sgfTree += ")";
            }
        }
    }
}
