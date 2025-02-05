using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.RemoteConfig;
using System.Runtime.InteropServices;

public class PlayerController : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private SceneData _sceneData;
    [SerializeField] private Config _config;

    public struct userAttributes { }
    public struct appAttributes { }

    private string _key;
    private bool _isNew;
    private GameObject _dotTool;
    private GameObject _tool;
    private GameObject _send;

    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void UploadSgf()
    {
        UploadFile(gameObject.name, "OnFileUpload", ".sgf", false);
    }

    public void OnFileUpload(string url)
    {
        StartCoroutine(OutputRoutine(url));
    }

    private IEnumerator OutputRoutine(string url)
    {
        var loader = new WWW(url);
        yield return loader;
        _config.SgfSystem.NewUploadSgfTree(loader.text);
    }

    [DllImport("__Internal")]
    private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

    public void SaveSgf()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(_config.SgfSystem.DownloadSgfTree());
        DownloadFile(gameObject.name, "OnFileDownload", "sample.sgf", bytes, bytes.Length);
    }

    public void OnFileDownload()
    {
        _config.SgfSystem.NewSaveSgfTree();
    }

    private async Task InitializeRemoteConfigAsync()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task Start()
    {
        await InitializeRemoteConfigAsync();
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        await RemoteConfigService.Instance.FetchConfigsAsync(new userAttributes(), new appAttributes());
    }

    private void ApplyRemoteConfig(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                //Debug.Log("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                //Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                //Debug.Log("New settings loaded this session; update values accordingly.");
                break;
        }
        if (RemoteConfigService.Instance.appConfig.GetBool("Setting0"))
        {
            _config.PlayerTurnState = 0;
            _sceneData.WaitingPanel.SetActive(true);
        }
    }

    public void RightAuto()
    {
        StartCoroutine(RightAutoCoroutine());
    }

    private IEnumerator RightAutoCoroutine()
    {
        while (_sceneData.RightButton.GetComponent<Button>().interactable == true)
        {
            _config.TreeSystem.Right();
            yield return new WaitForSeconds(0.75f);
        }
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        StartCoroutine(WaitAndCheck(newPlayer));
    }

    private IEnumerator WaitAndCheck(Player newPlayer)
    {
        yield return new WaitForSeconds(1);
        if (!string.IsNullOrEmpty(newPlayer.NickName))
        {
            if (_config.BanList.Any(p => p == newPlayer.NickName))
            {
                if (PhotonNetwork.IsMasterClient)
                    _config.ChatSystem.Delete(newPlayer);
                yield break;
            }
            var nPrefab = Instantiate(_sceneData.NamePrefab, _sceneData.ListNames);
            nPrefab.GetComponentInChildren<TextMeshProUGUI>().text = newPlayer.NickName;
            nPrefab.transform.GetChild(2).gameObject.SetActive(false);
            if (PhotonNetwork.IsMasterClient)
            {
                var admin = nPrefab.transform.GetChild(1);
                admin.GetChild(0).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.BlueRedDots(newPlayer, admin));
                admin.GetChild(1).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.BlueDot(newPlayer, admin));
                admin.GetChild(2).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.RedDot(newPlayer, admin));
                admin.GetChild(3).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Tools(newPlayer, admin));
                admin.GetChild(4).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Chat(newPlayer, admin));
                admin.GetChild(5).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Delete(newPlayer));
                var raiseEventOptions = RaiseEventOptions.Default;
                var sendOptions = SendOptions.SendReliable;
                foreach (var (k, v) in _config.Moves)
                {
                    PhotonNetwork.RaiseEvent(1, k, raiseEventOptions, sendOptions);
                    PhotonNetwork.RaiseEvent(2, v.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    PhotonNetwork.RaiseEvent(3, _config.MovesTree[k].GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    PhotonNetwork.RaiseEvent(5, v.CompareTag("Blue"), raiseEventOptions, sendOptions);
                }
                foreach (var (k, v) in _config.Symbols)
                {
                    foreach (var t in v)
                    {
                        PhotonNetwork.RaiseEvent(6, k.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                        PhotonNetwork.RaiseEvent(7, t.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                        if (t.GetComponent<LineRenderer>())
                        {
                            for (int i = 0; i < t.GetComponent<LineRenderer>().positionCount; i++)
                                PhotonNetwork.RaiseEvent(9, (Vector2)t.GetComponent<LineRenderer>().GetPosition(i),
                                    raiseEventOptions, sendOptions);
                            PhotonNetwork.RaiseEvent(9, Vector2.zero, raiseEventOptions, sendOptions);
                        }
                        if (t.GetComponent<TextMeshProUGUI>())
                        {
                            PhotonNetwork.RaiseEvent(10, t.GetComponent<TextMeshProUGUI>().text, raiseEventOptions, sendOptions);
                        }
                    }
                }
                PhotonNetwork.RaiseEvent(17, _sceneData.Player1Name.GetComponent<TextMeshProUGUI>().text, raiseEventOptions, sendOptions);
                PhotonNetwork.RaiseEvent(18, _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text, raiseEventOptions, sendOptions);
                foreach (var (k, v) in _config.SecTrees)
                {
                    foreach (var s in v)
                    {
                        PhotonNetwork.RaiseEvent(12, k.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                        PhotonNetwork.RaiseEvent(13, s.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    }
                }
                if (_config.Current)
                {
                    var key = _config.Moves.FirstOrDefault(m => m.Value == _config.Current).Key;
                    PhotonNetwork.RaiseEvent(4, key, raiseEventOptions, sendOptions);
                }
            }
            else
                nPrefab.transform.GetChild(1).gameObject.SetActive(false);
            if (!_config.IsCoach)
            {
                _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Players.Last().Value.NickName;
                _sceneData.DarkPanel.SetActive(false);
                _sceneData.WaitingPanel.SetActive(false);
                _config.PlayerTurnState = 1;
            }
        }
        else
            StartCoroutine(WaitAndCheck(newPlayer));
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (otherPlayer.CustomProperties.TryGetValue("IsBlueRedDots", out bool isDots) && isDots)
        {
            var properties = PhotonNetwork.MasterClient.CustomProperties;
            properties["IsBlueRedDots"] = true;
            PhotonNetwork.MasterClient.SetCustomProperties(properties);
        }
        if (otherPlayer.CustomProperties.TryGetValue("IsBlueDot", out bool isBlueDot) && isBlueDot)
        {
            var properties = PhotonNetwork.MasterClient.CustomProperties;
            properties["IsBlueRedDots"] = true;
            PhotonNetwork.MasterClient.SetCustomProperties(properties);
        }
        if (otherPlayer.CustomProperties.TryGetValue("IsRedDot", out bool isRedDot) && isRedDot)
        {
            var properties = PhotonNetwork.MasterClient.CustomProperties;
            properties["IsBlueRedDots"] = true;
            PhotonNetwork.MasterClient.SetCustomProperties(properties);
        }
        if (otherPlayer.CustomProperties.TryGetValue("IsTools", out bool isTools) && isTools)
        {
            var properties = PhotonNetwork.MasterClient.CustomProperties;
            properties["IsTools"] = true;
            PhotonNetwork.MasterClient.SetCustomProperties(properties);
        }
        foreach (Transform n in _sceneData.ListNames)
            Destroy(n.gameObject);
        var roomProperties = new ExitGames.Client.Photon.Hashtable();
        string playerList = "";
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var nPrefab = Instantiate(_sceneData.NamePrefab, _sceneData.ListNames);
            nPrefab.GetComponentInChildren<TextMeshProUGUI>().text = player.NickName;
            if (PhotonNetwork.IsMasterClient)
            {
                var admin = nPrefab.transform.GetChild(1);
                if (player == PhotonNetwork.LocalPlayer)
                {
                    admin.gameObject.SetActive(false);
                    nPrefab.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Lock(nPrefab.transform.GetChild(2)));
                }
                else
                {
                    nPrefab.transform.GetChild(2).gameObject.SetActive(false);
                    admin.GetChild(0).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.BlueRedDots(player, admin));
                    admin.GetChild(1).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.BlueDot(player, admin));
                    admin.GetChild(2).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.RedDot(player, admin));
                    admin.GetChild(3).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Tools(player, admin));
                    admin.GetChild(4).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Chat(player, admin));
                    admin.GetChild(5).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Delete(player));
                }
            }
            else
            {
                nPrefab.transform.GetChild(1).gameObject.SetActive(false);
                nPrefab.transform.GetChild(2).gameObject.SetActive(false);
            }
            playerList += player.NickName + ",";
        }
        roomProperties["Players"] = playerList;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (targetPlayer != PhotonNetwork.LocalPlayer) return;
        if (changedProps.TryGetValue("IsBlueRedDots", out bool isDots) && _config.IsNoDots == isDots)
        {
            _config.IsNoDots = !isDots;
            if (!_config.IsNoDots)
                CloseFrame();
            if (!PhotonNetwork.IsMasterClient)
                _sceneData.BlueRedDots.SetActive(!_config.IsNoDots);
            _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(!_config.IsNoDots);
            _sceneData.LeftFast.SetActive(!_config.IsNoDots);
            _sceneData.LeftButton.SetActive(!_config.IsNoDots);
            _sceneData.RightButton.SetActive(!_config.IsNoDots);
            _sceneData.RightFast.SetActive(!_config.IsNoDots);
            _sceneData.RightAuto.SetActive(!_config.IsNoDots);
            if (!_config.IsNoDots)
            {
                _config.Tool = Tool.Dot;
                var blue = false;
                if (_config.Current)
                    blue = _config.Current.CompareTag("Blue");
                _config.IsBluePlayer = !blue;
                _config.PlayerTurnState = blue ? 2 : 1;
                _config.IsNoBlueDot = true;
                _config.IsNoRedDot = true;
                _config.IsLock = false;
                return;
            }
        }
        if (changedProps.TryGetValue("IsBlueDot", out bool isBlueDot) && _config.IsNoBlueDot == isBlueDot)
        {
            _config.IsNoBlueDot = !isBlueDot;
            if (!_config.IsNoBlueDot)
                CloseFrame();
            if (!PhotonNetwork.IsMasterClient)
                _sceneData.BlueDot.SetActive(!_config.IsNoBlueDot);
            _sceneData.BlueDot.transform.GetChild(0).gameObject.SetActive(!_config.IsNoBlueDot);
            if (!_config.IsNoBlueDot)
            {
                _config.Tool = Tool.Dot;
                _config.IsBluePlayer = true;
                var blue = false;
                if (_config.Current)
                    blue = _config.Current.CompareTag("Blue");
                _config.PlayerTurnState = blue ? 2 : 1;
            }
        }
        if (changedProps.TryGetValue("IsRedDot", out bool isRedDot) && _config.IsNoRedDot == isRedDot)
        {
            _config.IsNoRedDot = !isRedDot;
            if (!_config.IsNoRedDot)
                CloseFrame();
            if (!PhotonNetwork.IsMasterClient)
                _sceneData.RedDot.SetActive(!_config.IsNoRedDot);
            _sceneData.RedDot.transform.GetChild(0).gameObject.SetActive(!_config.IsNoRedDot);
            if (!_config.IsNoRedDot)
            {
                _config.Tool = Tool.Dot;
                _config.IsBluePlayer = false;
                var blue = false;
                if (_config.Current)
                    blue = _config.Current.CompareTag("Blue");
                _config.PlayerTurnState = blue ? 2 : 1;
            }
        }
        if (changedProps.TryGetValue("IsTools", out bool isTools) && _config.IsNoTools == isTools)
        {
            _config.IsNoTools = !isTools;
            _sceneData.Num.SetActive(!_config.IsNoTools);
            _sceneData.Cross.SetActive(!_config.IsNoTools);
            _sceneData.Line.SetActive(!_config.IsNoTools);
            _sceneData.Arrow.SetActive(!_config.IsNoTools);
            _sceneData.Brush.SetActive(!_config.IsNoTools);
            _sceneData.Eraser.SetActive(!_config.IsNoTools);
            if (!PhotonNetwork.IsMasterClient)
                _config.ChatSystem.CommentButton();
        }
        if (changedProps.TryGetValue("IsChat", out bool isChat) && _config.IsNoChat == isChat)
        {
            _config.IsNoChat = !isChat;
            _config.ChatSystem.ChatButton();
        }
        if (changedProps.TryGetValue("Delete", out var value))
        {
            var player = value.ToString();
            if (!_config.BanList.Contains(player))
                _config.BanList.Add(player);
            if (targetPlayer.NickName == player)
            {
                _config.PlayerName = PhotonNetwork.NickName;
                PhotonNetwork.NickName = string.Empty;
                PhotonNetwork.Disconnect();
                _config.IsCoach = true;
                _config.IsGroup = PhotonNetwork.IsMasterClient;
                SceneManager.LoadScene("Loading");
            }
            if (PhotonNetwork.IsMasterClient)
                _config.ChatSystem.PlayerButton();
        }
    }

    private void CloseFrame()
    {
        _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.BlueDot.transform.GetChild(0).gameObject.SetActive(false);
        _sceneData.RedDot.transform.GetChild(0).gameObject.SetActive(false);
        if (!PhotonNetwork.IsMasterClient)
        {
            _sceneData.BlueRedDots.SetActive(false);
            _sceneData.BlueDot.SetActive(false);
            _sceneData.RedDot.SetActive(false);
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 0)
        {
            _config.PlayerTurnState = (int) photonEvent.CustomData;
        }
        if (photonEvent.Code == 1)
        {
            _key = (string) photonEvent.CustomData;
            if (_config.Moves.ContainsKey(_key))
            {
                _isNew = false;
                return;
            }
            else
                _isNew = true;
            var keyA = _key.Split('.');
            var start = new List<int>();
            if (keyA[0] != string.Empty)
                start = keyA[0].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            var count = int.Parse(keyA[1]);
            var number = new List<int>();
            if (keyA[2] != string.Empty)
                number = keyA[2].Split(",").Where(c => c != string.Empty).
                    Select(c => int.Parse(c.ToString())).ToList();
            _config.StartIndex = start;
            _config.CountIndex = count;
            _config.NumberIndex = number;
        }
        if (photonEvent.Code == 2)
        {
            var dot = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            if (_config.Moves.ContainsValue(dot)) return;
            dot.transform.parent = _sceneData.DotsTransform;
            _config.Current = dot;
            _config.Moves.Add(_key, dot);
        }
        if (photonEvent.Code == 3)
        {
            var dotTree = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            if (_config.MovesTree.ContainsValue(dotTree)) return;
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
                if (c == _config.CountIndex && s.Count > 0)
                    dotPosTreeY++;
            }
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
            var key = new string(_key);
            dotTree.GetComponent<Button>().onClick.AddListener(() => { if (!_config.IsNoDots) _config.TreeSystem.ShowMoves(key); });
            _config.MovesTree.Add(_key, dotTree);
        }
        if (photonEvent.Code == 4)
        {
            _config.TreeSystem.ShowMovesTree((string)photonEvent.CustomData);
        }
        if (photonEvent.Code == 5)
        {
            if (_isNew || _config.Current)
                _config.SurroundSystem.Run((bool)photonEvent.CustomData);
        }
        if (photonEvent.Code == 6)
        {
            _dotTool = PhotonView.Find((int)photonEvent.CustomData).gameObject;
        }
        if (photonEvent.Code == 7)
        {
            _tool = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            _tool.transform.parent = _sceneData.Symbols;
            if (_config.Symbols.ContainsKey(_dotTool))
            {
                if (!_config.Symbols[_dotTool].Contains(_tool))
                    _config.Symbols[_dotTool].Add(_tool);
            }
            else
                _config.Symbols.Add(_dotTool, new List<GameObject> { _tool });
        }
        if (photonEvent.Code == 8)
        {
            var tool = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            _config.Symbols[_config.Current].Remove(tool);
            if (tool.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                PhotonNetwork.Destroy(tool);
        }
        if (photonEvent.Code == 9)
        {
            var point = (Vector2) photonEvent.CustomData;
            if (_config.BrushLine)
            {
                if (_config.BrushLastPos != point && point != Vector2.zero)
                {
                    _config.BrushLine.positionCount++;
                    _config.BrushLine.SetPosition(_config.BrushLine.positionCount - 1, point);
                    _config.BrushLastPos = point;
                }
                if (point == Vector2.zero)
                {
                    var mesh = new Mesh();
                    _config.BrushLine.BakeMesh(mesh, true);
                    if (mesh.vertexCount > 6)
                        _config.BrushCollider.sharedMesh = mesh;
                    _config.BrushLine = null;
                }
            }
            else
            {
                _config.BrushLine = _tool.GetComponent<LineRenderer>();
                _config.BrushCollider = _tool.GetComponent<MeshCollider>();
                _config.BrushLine.SetPosition(0, point);
                _config.BrushLine.SetPosition(1, point);
            }
        }
        if (photonEvent.Code == 10)
        {
            _config.CommentHeight = _sceneData.Comments.GetComponent<ScrollRect>().content.sizeDelta.y;
            _tool.transform.parent = _sceneData.Comments.GetComponentInChildren<VerticalLayoutGroup>().transform;
            _tool.GetComponent<RectTransform>().localScale = Vector3.one;
            _tool.GetComponent<TextMeshProUGUI>().text = (string) photonEvent.CustomData;
            var key = _config.Moves.FirstOrDefault(m => m.Value == _dotTool).Key;
            var moveTree = _config.MovesTree[key];
            moveTree.transform.GetChild(2).gameObject.SetActive(true);
            if (!Mathf.Approximately(_sceneData.Comments.GetComponent<ScrollRect>().content.anchoredPosition.y, -270f))
                _config.IsSentComment = true;
        }
        if (photonEvent.Code == 11)
        {
            var properties = PhotonNetwork.LocalPlayer.CustomProperties;
            properties["IsBlueRedDots"] = false;
            properties["IsBlueDot"] = false;
            properties["IsRedDot"] = false;
            properties["IsTools"] = false;
            properties["IsChat"] = false;
            properties["Delete"] = string.Empty;
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            _config.InitSystem.Init();
        }
        if (photonEvent.Code == 12)
        {
            _config.Current = PhotonView.Find((int)photonEvent.CustomData).gameObject;
        }
        if (photonEvent.Code == 13)
        {
            var dot = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            if (_config.SecTrees.ContainsKey(_config.Current))
            {
                if (!_config.SecTrees[_config.Current].Contains(dot))
                    _config.SecTrees[_config.Current].Add(dot);
            }
            else
                _config.SecTrees.Add(_config.Current, new List<GameObject> { dot });
        }
        if (photonEvent.Code == 14)
        {
            var send = PhotonView.Find((int)photonEvent.CustomData).gameObject;
            if (send.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                PhotonNetwork.Destroy(send);
        }
        if (photonEvent.Code == 15)
        {
            _send = PhotonView.Find((int)photonEvent.CustomData).gameObject;
        }
        if (photonEvent.Code == 16)
        {
            _config.ChatHeight = _sceneData.Chat.GetComponent<ScrollRect>().content.sizeDelta.y;
            var send = _send;
            send.transform.parent = _sceneData.Chat.GetComponentInChildren<VerticalLayoutGroup>().transform;
            send.GetComponent<RectTransform>().localScale = Vector3.one;
            send.GetComponent<TextMeshProUGUI>().text = (string)photonEvent.CustomData;
            if (PhotonNetwork.IsMasterClient)
            {
                send.transform.GetChild(0).gameObject.SetActive(true);
                send.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    var raiseEventOptions = RaiseEventOptions.Default;
                    var sendOptions = SendOptions.SendReliable;
                    PhotonNetwork.RaiseEvent(14, send.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    if (send.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                        PhotonNetwork.Destroy(send);
                });
            }
            if (!Mathf.Approximately(_sceneData.Chat.GetComponent<ScrollRect>().content.anchoredPosition.y, -270f))
                _config.IsSentChat = true;
        }
        if (photonEvent.Code == 17)
        {
            _sceneData.Player1Name.GetComponent<TextMeshProUGUI>().text = (string)photonEvent.CustomData;
        }
        if (photonEvent.Code == 18)
        {
            _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text = (string)photonEvent.CustomData;
        }
        if (photonEvent.Code == 19)
        {
            _config.Width = (int)photonEvent.CustomData;
        }
        if (photonEvent.Code == 20)
        {
            _config.Height = (int)photonEvent.CustomData;
        }
    }

    private void OnApplicationQuit()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var p in PhotonNetwork.PlayerList)
                    _config.ChatSystem.Delete(p);
                PhotonNetwork.CurrentRoom.IsOpen = false;
                return;
            }
            PhotonNetwork.NickName = string.Empty;
            PhotonNetwork.Disconnect();
        }
    }
}
