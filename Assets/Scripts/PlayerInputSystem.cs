using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Leopotam.Ecs;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputSystem : IEcsRunSystem
{
    private SceneData _sceneData;
    private Config _config;
    
    public void Run()
    {
        var raycastOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(raycastOrigin, Vector2.zero);
        if (hit.collider)
        {
            if (hit.collider.CompareTag("Field") && _config.IsPointer)
            {
                var pos = hit.collider.transform.position + (Vector3)hit.collider.offset;
                if (_config.Tool == Tool.Dot && (!_config.IsNoDots || !_config.IsNoBlueDot || !_config.IsNoRedDot))
                {
                    if (_config.IsBluePlayer)
                        _sceneData.BlueCirclePhantom.position = pos;
                    else
                        _sceneData.RedCirclePhantom.position = pos;
                }
                foreach (Transform num in _sceneData.FieldTransform)
                {
                    if (num.CompareTag("Num"))
                    {
                        num.GetChild(0).gameObject.SetActive(false);
                        if (num.position == new Vector3(pos.x, -1, 0) ||
                            num.position == new Vector3(-1, pos.y, 0))
                            num.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            var hits = Physics2D.RaycastAll(raycastOrigin, Vector2.zero);
            if (Physics.Raycast(raycastOrigin, Vector3.forward, out var hit3D))
            {
                var tool = hit3D.collider.gameObject;
                if (_config.Tool == Tool.Eraser && tool.CompareTag("Symbol") && !_config.IsNoTools)
                {
                    _config.Symbols[_config.Current].Remove(tool);
                    var raiseEventOptions = RaiseEventOptions.Default;
                    var sendOptions = SendOptions.SendReliable;
                    if (PhotonNetwork.NickName != string.Empty)
                        PhotonNetwork.RaiseEvent(8, tool.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    if (PhotonNetwork.NickName == string.Empty)
                        Object.Destroy(tool);
                    else
                    {
                        if (tool.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                            PhotonNetwork.Destroy(tool);
                    }
                }
            }
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider)
                {
                    if (hits[i].collider.CompareTag("Chain")) return;
                    var playerState = _config.IsBluePlayer ? 1 : 2;
                    if (_config.PlayerTurnState != playerState) return;
                    var pos = hits[i].collider.transform.position + (Vector3)hits[i].collider.offset;
                    if (_config.Current)
                    {
                        if (_config.SecTrees.ContainsKey(_config.Current))
                        {
                            var num = _config.SecTrees[_config.Current].FirstOrDefault(n => n.transform.position == pos);
                            if (num)
                            {
                                if (!_config.IsNoDots)
                                    _config.TreeSystem.NumSecTree(_config.SecTrees[_config.Current].IndexOf(num));
                                return;
                            }
                        }
                    }
                    if (_config.Tool == Tool.Dot && (!_config.IsNoDots || !_config.IsNoBlueDot || !_config.IsNoRedDot))
                    {
                        var x = (int)(pos.x + 0.5f);
                        var y = (int)(pos.y + 0.5f);
                        if (_config.Board.At(new Coordinate(x, y)).Stone != PointColor.None) return;   
                        var prefab = _config.IsBluePlayer ? _sceneData.BlueCircle : _sceneData.RedCircle;
                        var prefabTree = _config.IsBluePlayer ? _sceneData.BlueCircleTree : _sceneData.RedCircleTree;
                        var last = true;
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
                            if (n.Count > _config.NumberIndex.Count)
                                n.RemoveAt(_config.NumberIndex.Count);
                            if (s.SequenceEqual(_config.StartIndex) && n.SequenceEqual(_config.NumberIndex))
                                last = c <= _config.CountIndex;
                        }
                        if (!last)
                            _config.StartIndex.Add(_config.CountIndex);
                        string start = null;
                        foreach (var s in _config.StartIndex)
                            start += $"{s},";
                        _config.CountIndex++;
                        if (_config.CountIndex > _config.BiggestRight)
                            _config.BiggestRight = _config.CountIndex;
                        var dotPosTreeY = 0;
                        var sameKey = 0;
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
                                if (s.SequenceEqual(_config.StartIndex))
                                {
                                    var nFirst = n.ToList();
                                    nFirst.RemoveAt(_config.NumberIndex.Count);
                                    if (_config.NumberIndex.SequenceEqual(nFirst))
                                        sameKey++;
                                }
                            }
                        }
                        if (_config.StartIndex.Count > 0)
                            _config.NumberIndex.Add(sameKey);
                        string number = null;
                        foreach (var n in _config.NumberIndex)
                            number += $"{n},";
                        var key = $"{start}.{_config.CountIndex}.{number}";
                        var dot = _config.IsCoach ? PhotonNetwork.NickName == string.Empty ?
                            Object.Instantiate(prefab, pos, Quaternion.identity) :
                            PhotonNetwork.Instantiate(prefab.name, pos, Quaternion.identity) :
                            PhotonNetwork.Instantiate(prefab.name, pos, Quaternion.identity);
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
                        var dotTree = _config.IsCoach ? PhotonNetwork.NickName == string.Empty ?
                            Object.Instantiate(prefabTree) :
                            PhotonNetwork.Instantiate(prefabTree.name, Vector3.zero, Quaternion.identity) :
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
                        _config.SurroundSystem.Run(_config.IsBluePlayer);
                        if (PhotonNetwork.NickName != string.Empty)
                        {
                            PhotonNetwork.RaiseEvent(1, key, raiseEventOptions, sendOptions);
                            PhotonNetwork.RaiseEvent(2, dot.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                            PhotonNetwork.RaiseEvent(3, dotTree.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                            PhotonNetwork.RaiseEvent(5, _config.IsBluePlayer, raiseEventOptions, sendOptions);
                        }
                        _config.TreeSystem.ShowMoves(key);
                        if (!_config.IsCoach || !_config.IsNoBlueDot || !_config.IsNoRedDot)
                        {
                            _config.PlayerTurnState = _config.IsBluePlayer ? 2 : 1;
                            PhotonNetwork.RaiseEvent(0, _config.PlayerTurnState, raiseEventOptions, sendOptions);
                        }
                    }
                    if (!_config.IsNoTools)
                    {
                        if (hits[i].collider.CompareTag("Field") && _config.Tool != Tool.Eraser && _config.Tool != Tool.Dot)
                        {
                            var dot = _config.Current;
                            if (!_config.Symbols.ContainsKey(dot) ||
                                _config.Symbols.ContainsKey(dot) &&
                                !_config.Symbols[dot].Any(s => s.transform.position == pos))
                            {
                                GameObject tool = null;
                                if (_config.Tool == Tool.Num && _config.NumTool < 40)
                                {
                                    _config.NumTool++;
                                    tool = PhotonNetwork.NickName == string.Empty ?
                                        (GameObject)Object.Instantiate(Resources.Load(_config.NumTool.ToString()),
                                        pos, Quaternion.identity) :
                                        PhotonNetwork.Instantiate(_config.NumTool.ToString(), pos, Quaternion.identity);
                                    tool.transform.localScale = new Vector3(2, 2, 1);
                                }
                                if (_config.Tool == Tool.Cross)
                                    tool = PhotonNetwork.NickName == string.Empty ?
                                        (GameObject)Object.Instantiate(Resources.Load("Cross"),
                                        pos, Quaternion.identity) :
                                        PhotonNetwork.Instantiate("Cross", pos, Quaternion.identity);
                                if (_config.Tool == Tool.Line)
                                    tool = PhotonNetwork.NickName == string.Empty ?
                                        (GameObject)Object.Instantiate(Resources.Load("Line"),
                                        pos, Quaternion.identity) :
                                        PhotonNetwork.Instantiate("Line", pos, Quaternion.identity);
                                if (_config.Tool == Tool.Arrow)
                                    tool = PhotonNetwork.NickName == string.Empty ?
                                        (GameObject)Object.Instantiate(Resources.Load("Arrow"),
                                        pos, Quaternion.identity) :
                                        PhotonNetwork.Instantiate("Arrow", pos, Quaternion.identity);
                                if (_config.Tool == Tool.Brush && i == 0)
                                {
                                    tool = PhotonNetwork.NickName == string.Empty ?
                                        (GameObject)Object.Instantiate(Resources.Load("Brush")) :
                                        PhotonNetwork.Instantiate("Brush", Vector3.zero, Quaternion.identity);
                                    _config.BrushLine = tool.GetComponent<LineRenderer>();
                                    _config.BrushCollider = tool.GetComponent<MeshCollider>();
                                    _config.BrushLine.SetPosition(0, hits[i].point);
                                    _config.BrushLine.SetPosition(1, hits[i].point);
                                }
                                if (!tool)
                                    return;
                                tool.transform.parent = _sceneData.Symbols;
                                if (_config.Symbols.ContainsKey(dot))
                                    _config.Symbols[dot].Add(tool);
                                else
                                    _config.Symbols.Add(dot, new List<GameObject> { tool });
                                var raiseEventOptions = RaiseEventOptions.Default;
                                var sendOptions = SendOptions.SendReliable;
                                if (PhotonNetwork.NickName != string.Empty)
                                {
                                    PhotonNetwork.RaiseEvent(6, dot.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                                    PhotonNetwork.RaiseEvent(7, tool.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                                    if (_config.Tool == Tool.Brush)
                                    {
                                        PhotonNetwork.RaiseEvent(9, hits[i].point, raiseEventOptions, sendOptions);
                                    }
                                }
                            }
                        }
                        if (_config.Tool == Tool.Eraser)
                        {
                            if (hits[i].collider.CompareTag("Num") || hits[i].collider.CompareTag("Symbol") ||
                                hits[i].collider.CompareTag("Rotate"))
                            {
                                var tool = hits[i].collider.gameObject;
                                _config.Symbols[_config.Current].Remove(tool);
                                var raiseEventOptions = RaiseEventOptions.Default;
                                var sendOptions = SendOptions.SendReliable;
                                if (PhotonNetwork.NickName != string.Empty)
                                    PhotonNetwork.RaiseEvent(8, tool.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                                if (PhotonNetwork.NickName == string.Empty)
                                    Object.Destroy(tool);
                                else
                                {
                                    if (tool.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                                        PhotonNetwork.Destroy(tool);
                                }
                            }
                        }
                        if (_config.Tool == Tool.Arrow || _config.Tool == Tool.Line)
                        {
                            if (hits[i].collider.CompareTag("Rotate"))
                                hits[i].collider.gameObject.transform.Rotate(Vector3.forward, 45f);
                        }
                    }
                }
            }
        }
        if (!_config.IsNoTools)
        {
            if (Input.GetMouseButton(0))
            {
                if (hit.collider)
                {
                    if (_config.Tool == Tool.Brush && hit.collider.CompareTag("Field") && _config.BrushLine)
                    {
                        if (_config.BrushLastPos != hit.point)
                        {
                            _config.BrushLine.positionCount++;
                            _config.BrushLine.SetPosition(_config.BrushLine.positionCount - 1, hit.point);
                            _config.BrushLastPos = hit.point;
                            var raiseEventOptions = RaiseEventOptions.Default;
                            var sendOptions = SendOptions.SendReliable;
                            if (PhotonNetwork.NickName != string.Empty)
                                PhotonNetwork.RaiseEvent(9, hit.point, raiseEventOptions, sendOptions);
                        }
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (_config.BrushLine)
                {
                    var mesh = new Mesh();
                    _config.BrushLine.BakeMesh(mesh, true);
                    if (mesh.vertexCount > 6)
                        _config.BrushCollider.sharedMesh = mesh;
                    _config.BrushLine = null;
                    var raiseEventOptions = RaiseEventOptions.Default;
                    var sendOptions = SendOptions.SendReliable;
                    if (PhotonNetwork.NickName != string.Empty)
                        PhotonNetwork.RaiseEvent(9, Vector2.zero, raiseEventOptions, sendOptions);
                }
            }
        }
        if (!_config.IsNoDots)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) && !_sceneData.InputField.GetComponent<TMP_InputField>().isFocused)
                _config.TreeSystem.Left();
            if (Input.GetKeyDown(KeyCode.RightArrow) && !_sceneData.InputField.GetComponent<TMP_InputField>().isFocused)
                _config.TreeSystem.Right();
            if (Input.GetKeyDown(KeyCode.Alpha1))
                _config.TreeSystem.NumSecTree(0);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                _config.TreeSystem.NumSecTree(1);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                _config.TreeSystem.NumSecTree(2);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                _config.TreeSystem.NumSecTree(3);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                _config.TreeSystem.NumSecTree(4);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                _config.TreeSystem.NumSecTree(5);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                _config.TreeSystem.NumSecTree(6);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                _config.TreeSystem.NumSecTree(7);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                _config.TreeSystem.NumSecTree(8);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                _config.TreeSystem.DownSecTree();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                _config.TreeSystem.UpSecTree();
        }
        if (Input.GetKeyDown(KeyCode.Return))
            _config.ChatSystem.SendMessage();
        if (_config.IsSentComment && _sceneData.Comments.GetComponent<ScrollRect>().content.sizeDelta.y != _config.CommentHeight)
        {
            _sceneData.Comments.GetComponent<ScrollRect>().content.anchoredPosition =
                new Vector2(_sceneData.Comments.GetComponent<ScrollRect>().content.anchoredPosition.x,
                _sceneData.Comments.GetComponent<ScrollRect>().content.anchoredPosition.y -
                (_sceneData.Comments.GetComponent<ScrollRect>().content.sizeDelta.y - _config.CommentHeight));
            _config.IsSentComment = false;
            _config.CommentHeight = 0;
        }
        if (_config.IsSentChat && _sceneData.Chat.GetComponent<ScrollRect>().content.sizeDelta.y != _config.ChatHeight)
        {
            _sceneData.Chat.GetComponent<ScrollRect>().content.anchoredPosition =
                new Vector2(_sceneData.Chat.GetComponent<ScrollRect>().content.anchoredPosition.x,
                _sceneData.Chat.GetComponent<ScrollRect>().content.anchoredPosition.y -
                (_sceneData.Chat.GetComponent<ScrollRect>().content.sizeDelta.y - _config.ChatHeight));
            _config.IsSentChat = false;
            _config.ChatHeight = 0;
        }
    }
}
