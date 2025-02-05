using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreeSystem
{
    private readonly SceneData _sceneData;
    private readonly Config _config;

    private bool _isPlaying;
    private int _indexSecTree;

    public TreeSystem(SceneData sceneData, Config config)
    {
        _sceneData = sceneData;
        _config = config;
    }

    public void Left()
    {
        if (_config.CountIndex == -1) return;
        if (_config.CountIndex == 0) return;
        if (_config.StartIndex.Count > 0)
            if (_config.StartIndex.Last() == _config.CountIndex - 1)
                _config.StartIndex.RemoveAt(_config.StartIndex.Count - 1);
        string start = null;
        foreach (var s in _config.StartIndex)
            start += $"{s},";
        _config.CountIndex--;
        if (_config.NumberIndex.Count > 0)
            _config.NumberIndex.RemoveAt(_config.NumberIndex.Count - 1);
        string number = null;
        foreach (var n in _config.NumberIndex)
            number += $"{n},";
        var key = $"{start}.{_config.CountIndex}.{number}";
        ShowMoves(key);
    }

    public void Right()
    {
        if (_config.CountIndex == -1) return;
        var last = false;
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
        if (last) return;
        string start = null;
        foreach (var s in _config.StartIndex)
            start += $"{s},";
        _config.CountIndex++;
        if (_config.NumberIndex.Count > 0)
            _config.NumberIndex.Add(0);
        string number = null;
        foreach (var n in _config.NumberIndex)
            number += $"{n},";
        var key = $"{start}.{_config.CountIndex}.{number}";
        ShowMoves(key);
    }

    public void LeftFast()
    {
        while (_config.CountIndex != 0)
            _config.TreeSystem.Left();
    }

    public void RightFast()
    {
        while (_sceneData.RightButton.GetComponent<Button>().interactable == true)
            _config.TreeSystem.Right();
    }

    public void RightAuto()
    {
        _sceneData.Player.Stop();
        _isPlaying = !_isPlaying;
        if (_isPlaying)
            _sceneData.Player.RightAuto();
        else
            _sceneData.Player.Stop();
    }

    public void NumSecTree(int index, bool isUpDown = false)
    {
        if (index < 0) return;
        List<int> startIndex = new(_config.StartIndex);
        int countIndex = _config.CountIndex;
        List<int> numberIndex = new(_config.NumberIndex);
        if (isUpDown)
        {
            if (startIndex.Count > 0)
                if (startIndex.Last() == countIndex - 1)
                    startIndex.RemoveAt(startIndex.Count - 1);
            countIndex--;
            if (numberIndex.Count > 0)
                numberIndex.RemoveAt(numberIndex.Count - 1);
        }
        if (index > 0)
            startIndex.Add(countIndex);
        string start = null;
        foreach (var s in startIndex)
            start += $"{s},";
        countIndex++;
        if (index > 0)
            numberIndex.Add(index - 1);
        if (index == 0 && startIndex.Count > 0)
            numberIndex.Add(0);
        string number = null;
        foreach (var n in numberIndex)
            number += $"{n},";
        var key = $"{start}.{countIndex}.{number}";
        if (_config.Moves.ContainsKey(key))
        {   
            _indexSecTree = index;
            _config.StartIndex = startIndex;
            _config.CountIndex = countIndex;
            _config.NumberIndex = numberIndex;
            ShowMoves(key);
        }
    }

    public void DownSecTree()
    {
        NumSecTree(_indexSecTree + 1, true);
    }

    public void UpSecTree()
    {
        NumSecTree(_indexSecTree - 1, true);
    }

    public void ShowMoves(string key, bool isMainTree = false)
    {
        ShowMovesTree(key, isMainTree);
        if (PhotonNetwork.NickName != string.Empty)
            PhotonNetwork.RaiseEvent(4, key, RaiseEventOptions.Default, SendOptions.SendReliable);
    }

    public void ShowMovesTree(string key, bool isMainTree = false)
    {
        _sceneData.BlueCirclePhantom.position = new Vector3(-100, -100, 0);
        _sceneData.RedCirclePhantom.position = new Vector3(-100, -100, 0);
        _sceneData.LeftButton.GetComponent<Button>().interactable = true;
        _sceneData.RightButton.GetComponent<Button>().interactable = true;
        _sceneData.LeftFast.GetComponent<Button>().interactable = true;
        _sceneData.RightFast.GetComponent<Button>().interactable = true;
        _sceneData.RightAuto.GetComponent<Button>().interactable = true;
        var keyA = key.Split('.');
        var start = new List<int>();
        if (keyA[0] != string.Empty)
            start = keyA[0].Split(",").Where(c => c != string.Empty).
                Select(c => int.Parse(c.ToString())).ToList();
        var count = int.Parse(keyA[1]);
        var number = new List<int>();
        if (keyA[2] != string.Empty)
            number = keyA[2].Split(",").Where(c => c != string.Empty).
                Select(c => int.Parse(c.ToString())).ToList();
        var last = false;
        var curTreePosY = 0f;
        foreach (var (k, v) in _config.MovesTree)
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
            if (n.Count > number.Count)
                n.RemoveAt(number.Count);
            if (s.SequenceEqual(start) && n.SequenceEqual(number))
                last = c <= count;
            v.transform.GetChild(0).gameObject.SetActive(false);
            if (s.SequenceEqual(start) && n.SequenceEqual(number) && c == count)
            {
                v.transform.GetChild(0).gameObject.SetActive(true);
                curTreePosY = v.GetComponent<RectTransform>().anchoredPosition.y;
            }
        }
        var rt = _sceneData.MainTree.GetComponent<RectTransform>();
        if (_config.BiggestRight >= 8)
        {
            var max = -20f - 80f * (_config.BiggestRight - 8);
            var left = 0f;
            var right = 0f;
            if (count <= 3)
            {
                left = 0;
                right = max;
            }
            if (count >= 4 && count <= _config.BiggestRight - 5)
            {
                left = -20f - 80f * (count - 4);
                right = max - left;
            }
            if (count >= _config.BiggestRight - 4)
            {
                left = max;
                right = 0;
            }
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
        if (_config.BiggestDown >= 5)
        {
            var max = -50f - 80f * (_config.BiggestDown - 5);
            var top = 0f;
            var bottom = 0f;
            var posY = (int)(curTreePosY / -80f) - 1;
            if (posY <= 2)
            {
                top = 0;
                bottom = max;
            }
            if (posY >= 3 && posY <= _config.BiggestDown - 3)
            {
                top = -50f - 80f * (posY - 3);
                bottom = max - top;
            }
            if (posY >= _config.BiggestDown - 2)
            {
                top = max;
                bottom = 0;
            }
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }
        if (count == 0)
        {
            _sceneData.LeftButton.GetComponent<Button>().interactable = false;
            _sceneData.LeftFast.GetComponent<Button>().interactable = false;
        }
        if (last)
        {
            _sceneData.RightButton.GetComponent<Button>().interactable = false;
            _sceneData.RightFast.GetComponent<Button>().interactable = false;
            _sceneData.RightAuto.GetComponent<Button>().interactable = false;
            _isPlaying = false;
        }
        _config.PlayerScore[PointColor.Blue] = 0;
        _config.PlayerScore[PointColor.Red] = 0;
        var show = false;
        foreach (var (k, v) in _config.Moves)
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
            if (s.Count == start.Count)
            {
                if (s.SequenceEqual(start))
                {
                    var numberFirst = number.ToList();
                    if (number.Count > n.Count)
                        numberFirst.RemoveRange(n.Count, number.Count - n.Count);
                    if (n.SequenceEqual(numberFirst))
                        show = c <= count;
                    else
                        show = false;
                }
                else
                    show = false;
            }
            if (s.Count < start.Count)
            {
                var startFirst = start.ToList();
                startFirst.RemoveRange(s.Count, start.Count - s.Count);
                if (s.SequenceEqual(startFirst))
                {
                    var numberFirst = number.ToList();
                    if (number.Count > n.Count)
                        numberFirst.RemoveRange(n.Count, number.Count - n.Count);
                    if (n.SequenceEqual(numberFirst))
                        show = c <= start[s.Count];
                    else
                        show = false;
                }
                else
                    show = false;
            }
            if (s.Count > start.Count)
                show = false;
            v.SetActive(show);
            var pos = v.transform.position;
            var x = (int)(pos.x + 0.5f);
            var y = (int)(pos.y + 0.5f);
            var point = _config.Board.At(new Coordinate(x, y));
            var color1 = v.CompareTag("Blue") ? PointColor.Blue : PointColor.Red;
            var color2 = v.CompareTag("Blue") ? PointColor.Red : PointColor.Blue;
            if (show)
            {
                if (v.layer == 3)
                    point.PutStone(_config.Points[v].Color, color1, k);
                else
                    point.PutStone(color1, color1, k);
            }
            else
            {
                if (point.Stone != PointColor.None && (k == point.Key || isMainTree))
                {
                    if (c >= count)
                        point.PutStone(PointColor.None, PointColor.None);
                    if (c < count)
                        point.PutStone(color1, PointColor.None);
                }
            }
            if (s.SequenceEqual(start) && c == count && n.SequenceEqual(number))
                _config.Current = v;
            if (_config.Symbols.ContainsKey(v))
                foreach (var sym in _config.Symbols[v])
                    sym.SetActive(false);
            if (v.layer == 3)
            {
                var lines = _config.Lines.FirstOrDefault(pair => pair.Key == v).Value;
                foreach (var line in lines)
                    line.SetActive(show);
                var surrounds = _config.Surrounds.FirstOrDefault(pair => pair.Key == v).Value;
                foreach (var surround in surrounds)
                    surround.SetActive(show);
                if (show)
                {
                    _config.PlayerScore[point.Owner] += _config.Points[v].Score;
                    _config.PlayerScore[point.Owner == PointColor.Blue ? PointColor.Red : PointColor.Blue] -= _config.FriendPoints[v];
                }
                var surDots = _config.SurDots.FirstOrDefault(pair => pair.Key == v).Value;
                if (surDots != null)
                {
                    foreach (var surDot in surDots)
                    {
                        if (surDot.Stone != PointColor.None)
                        {
                            if (show)
                                surDot.PutStone(color1, color2);
                            else
                                surDot.PutStone(color2, color2);
                        }
                        else
                            surDot.PutStone(PointColor.None, PointColor.None);
                    }
                }
            }
        }
        foreach (Transform s in _sceneData.SecTrees)
            Object.Destroy(s.gameObject);
        if (_config.SecTrees.ContainsKey(_config.Current) && _config.SecTrees[_config.Current].Count > 1)
        {
            foreach (var obj in _config.SecTrees[_config.Current])
            {
                var secTree = (GameObject)Object.Instantiate(Resources.Load($"{_config.SecTrees[_config.Current].IndexOf(obj) + 1}"),
                    obj.transform.position, Quaternion.identity, _sceneData.SecTrees);
                var child = secTree.transform.GetChild(0).gameObject;
                child.SetActive(true);
                child.GetComponent<SpriteRenderer>().sprite = obj.CompareTag("Blue") ? _config.BlueNumber : _config.RedNumber;
            }
        }
        foreach (var l in _config.SecTrees.Values)
        {
            var sec = l.FirstOrDefault(s => s == _config.Current);
            if (sec)
                _indexSecTree = l.IndexOf(sec);
        }
        if (_config.Symbols.ContainsKey(_config.Current))
            foreach (var sym in _config.Symbols[_config.Current])
                sym.SetActive(true);
        _sceneData.Player1Points.GetComponent<TextMeshProUGUI>().text = _config.PlayerScore[PointColor.Blue].ToString();
        _sceneData.Player2Points.GetComponent<TextMeshProUGUI>().text = _config.PlayerScore[PointColor.Red].ToString();
        _config.StartIndex = start;
        _config.CountIndex = count;
        _config.NumberIndex = number;
        _sceneData.Pointer.position = _config.Current.transform.position;
        if (_config.IsCoach && !_config.IsLock && !_config.IsNoDots && _config.IsNoBlueDot && _config.IsNoRedDot)
        {
            var blue = _config.Current.CompareTag("Blue");
            _sceneData.BlueMarker.transform.localScale = blue ? _config.Small : _config.Big;
            _sceneData.RedMarker.transform.localScale = blue ? _config.Big : _config.Small;
            _config.IsBluePlayer = !blue;
            _config.PlayerTurnState = blue ? 2 : 1;
            _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(true);
            _sceneData.BlueDot.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.RedDot.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Line.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Cross.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Arrow.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Brush.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Num.transform.GetChild(0).gameObject.SetActive(false);
            _sceneData.Eraser.transform.GetChild(0).gameObject.SetActive(false);
        }
        if (_config.Symbols.ContainsKey(_config.Current))
            _config.NumTool = _config.Symbols[_config.Current].Count(n => n.CompareTag("Num"));
        else
            _config.NumTool = 0;
        _config.Tool = Tool.Dot;
    }
}