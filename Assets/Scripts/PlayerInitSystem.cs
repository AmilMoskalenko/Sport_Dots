using System.Collections.Generic;
using System.Linq;
using System.IO;
using Leopotam.Ecs;
using Photon.Pun;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class PlayerInitSystem : IEcsInitSystem
{
    private SceneData _sceneData;
    private Config _config;
    
    public void Init()
    {
        _config.PlayerName = string.Empty;
        _config.ScreenWidth = Screen.width;
        _config.ScreenHeight = Screen.height;
        _config.SurroundSystem = new SurroundSystem(_sceneData, _config);
        _config.TreeSystem = new TreeSystem(_sceneData, _config);
        _config.SgfSystem = new SgfSystem(_sceneData, _config);
        _config.ChatSystem = new ChatSystem(_sceneData, _config);
        _config.InitSystem = new InitSystem(_sceneData, _config);
        Initialize();
        _config.BanList = new List<string>();
        _sceneData.RoomButton.GetComponent<Button>().onClick.AddListener(RoomPanel);
        _sceneData.ToolsButton.GetComponent<Button>().onClick.AddListener(ToolsPanel);
        _sceneData.SettingsButton.GetComponent<Button>().onClick.AddListener(SettingsPanel);
        _sceneData.LeftFast.GetComponent<Button>().onClick.AddListener(_config.TreeSystem.LeftFast);
        _sceneData.LeftButton.GetComponent<Button>().onClick.AddListener(_config.TreeSystem.Left);
        _sceneData.RightButton.GetComponent<Button>().onClick.AddListener(_config.TreeSystem.Right);
        _sceneData.RightFast.GetComponent<Button>().onClick.AddListener(_config.TreeSystem.RightFast);
        _sceneData.RightAuto.GetComponent<Button>().onClick.AddListener(_config.TreeSystem.RightAuto);
        _sceneData.SendButton.GetComponent<Button>().onClick.AddListener(_config.ChatSystem.SendMessage);
        _sceneData.DarkPanel.GetComponent<Button>().onClick.AddListener(ClosePanel);
        var roomPrefab = Object.Instantiate(_sceneData.RoomPrefab, _sceneData.ListRooms);
        roomPrefab.GetComponentInChildren<TextMeshProUGUI>().text = "Комната1";
        roomPrefab.GetComponentInChildren<Button>().onClick.AddListener(Room);
        var namePrefab = Object.Instantiate(_sceneData.NamePrefab, _sceneData.ListNames);
        namePrefab.GetComponentInChildren<TextMeshProUGUI>().text = "Игрок " + Random.Range(1, 100);
        _sceneData.Field.GetComponent<Button>().onClick.AddListener(Field);
        var toggles = new List<GameObject> { _sceneData.BigDotToggle,
            _sceneData.MediumDotToggle, _sceneData.SmallDotToggle };
        toggles[0].GetComponent<Toggle>().onValueChanged.AddListener(t => { DotToggle(t, 0); });
        toggles[1].GetComponent<Toggle>().onValueChanged.AddListener(t => { DotToggle(t, 1); });
        toggles[2].GetComponent<Toggle>().onValueChanged.AddListener(t => { DotToggle(t, 2); });
        foreach (var toggle in toggles)
            toggle.GetComponent<Toggle>().isOn = toggles.IndexOf(toggle) == _config.DotSize;
        _sceneData.Dots.GetComponent<Button>().onClick.AddListener(Dots);
        _sceneData.BlueRedDots.GetComponent<Button>().onClick.AddListener(BlueRedDots);
        _sceneData.BlueDot.GetComponent<Button>().onClick.AddListener(BlueDot);
        _sceneData.RedDot.GetComponent<Button>().onClick.AddListener(RedDot);
        _sceneData.Num.GetComponent<Button>().onClick.AddListener(Num);
        _sceneData.Cross.GetComponent<Button>().onClick.AddListener(Cross);
        _sceneData.Line.GetComponent<Button>().onClick.AddListener(Line);
        _sceneData.Arrow.GetComponent<Button>().onClick.AddListener(Arrow);
        _sceneData.Brush.GetComponent<Button>().onClick.AddListener(Brush);
        _sceneData.Eraser.GetComponent<Button>().onClick.AddListener(Eraser);
        _sceneData.NewParty.GetComponent<Button>().onClick.AddListener(NewParty);
        var togglesCross = new List<GameObject> { _sceneData.NoCrossToggle,
            _sceneData.Cross1Toggle, _sceneData.Cross4Toggle };
        togglesCross[0].GetComponent<Toggle>().onValueChanged.AddListener(t => { CrossToggle(t, 0); });
        togglesCross[1].GetComponent<Toggle>().onValueChanged.AddListener(t => { CrossToggle(t, 1); });
        togglesCross[2].GetComponent<Toggle>().onValueChanged.AddListener(t => { CrossToggle(t, 2); });
        foreach (var toggle in togglesCross)
            toggle.GetComponent<Toggle>().isOn = togglesCross.IndexOf(toggle) == _config.CrossSize;
        _sceneData.PointerToggle.GetComponent<Toggle>().onValueChanged.AddListener(Pointer);
        _sceneData.PointerToggle.GetComponent<Toggle>().isOn = _config.IsPointer;
        _sceneData.GameButton.GetComponent<Button>().onClick.AddListener(PlayGame);
        _sceneData.CoachGroup.GetComponent<Button>().onClick.AddListener(() => CoachGroup(true));
        _sceneData.UploadButton.GetComponent<Button>().onClick.AddListener(/*_sceneData.Player.*/UploadSgf);
        _sceneData.SaveButton.GetComponent<Button>().onClick.AddListener(/*_sceneData.Player.*/SaveSgf);
        _sceneData.ApplyButton.GetComponent<Button>().onClick.AddListener(Field);
        _sceneData.InfoButton.GetComponent<Button>().onClick.AddListener(Info);
        _sceneData.LeaveButton.GetComponent<Button>().onClick.AddListener(Leave);
        _sceneData.CloseSettings.GetComponent<Button>().onClick.AddListener(ClosePanel);
        _sceneData.CloseInfo.GetComponent<Button>().onClick.AddListener(ClosePanel);
        if (PhotonNetwork.NickName != string.Empty)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                _config.IsNoDots = true;
                _config.IsNoBlueDot = true;
                _config.IsNoRedDot = true;
                _config.IsNoTools = true;
                _config.IsNoChat = true;
                _sceneData.LeftFast.SetActive(false);
                _sceneData.LeftButton.SetActive(false);
                _sceneData.RightButton.SetActive(false);
                _sceneData.RightFast.SetActive(false);
                _sceneData.RightAuto.SetActive(false);
                _sceneData.BlueRedDots.SetActive(false);
                _sceneData.BlueDot.SetActive(false);
                _sceneData.RedDot.SetActive(false);
                _sceneData.Num.SetActive(false);
                _sceneData.Cross.SetActive(false);
                _sceneData.Line.SetActive(false);
                _sceneData.Arrow.SetActive(false);
                _sceneData.Brush.SetActive(false);
                _sceneData.Eraser.SetActive(false);
                _sceneData.NewParty.SetActive(false);
                _sceneData.RoomsList.SetActive(true);
                _sceneData.RoomsList.GetComponent<Button>().onClick.AddListener(() => CoachGroup(false));
                _sceneData.UploadButton.SetActive(false);
                _sceneData.SaveButton.SetActive(false);
                _sceneData.SettingsButton.SetActive(false);
            }
            _sceneData.RoomButton.GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
            _sceneData.ChatButton.SetActive(true);
            _sceneData.ChatButton.GetComponent<Button>().onClick.AddListener(_config.ChatSystem.ChatButton);
            _sceneData.CommentButton.SetActive(true);
            _sceneData.CommentButton.GetComponent<Button>().onClick.AddListener(_config.ChatSystem.CommentButton);
            _sceneData.PlayerButton.SetActive(true);
            _sceneData.PlayerButton.GetComponent<Button>().onClick.AddListener(_config.ChatSystem.PlayerButton);
            _config.ChatSystem.ChatButton();
            foreach (Transform r in _sceneData.ListRooms)
                Object.Destroy(r.gameObject);
            var rPrefab = Object.Instantiate(_sceneData.RoomPrefab, _sceneData.ListRooms);
            rPrefab.GetComponentInChildren<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
            foreach (Transform n in _sceneData.ListNames)
                Object.Destroy(n.gameObject);
            string playerList = "";
            foreach (var player in PhotonNetwork.PlayerList)
            {
                var nPrefab = Object.Instantiate(_sceneData.NamePrefab, _sceneData.ListNames);
                nPrefab.GetComponentInChildren<TextMeshProUGUI>().text = player.NickName;
                nPrefab.transform.GetChild(1).gameObject.SetActive(false);
                if (PhotonNetwork.IsMasterClient)
                {
                    nPrefab.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => _config.ChatSystem.Lock(nPrefab.transform.GetChild(2)));
                    var roomProps = new Hashtable { ["Width"] = _config.Width, ["Height"] = _config.Height };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
                }
                else
                    nPrefab.transform.GetChild(2).gameObject.SetActive(false);
                playerList += player.NickName + ",";
            }
            var roomProperties = new Hashtable { ["Players"] = playerList };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
            var properties = new Hashtable
            {
                { "IsBlueRedDots", !_config.IsNoDots },
                { "IsBlueDot", !_config.IsNoBlueDot },
                { "IsRedDot", !_config.IsNoRedDot },
                { "IsTools", !_config.IsNoTools },
                { "IsChat", !_config.IsNoChat },
                { "Delete", string.Empty }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            if (!_config.IsCoach)
            {
                _sceneData.Player1Timer.SetActive(true);
                _sceneData.Player2Timer.SetActive(true);
                _sceneData.BlueMarker.SetActive(false);
                _sceneData.RedMarker.SetActive(false);
                _sceneData.DarkPanel.SetActive(true);
                _sceneData.WaitingPanel.SetActive(true);
                _config.IsBluePlayer = PhotonNetwork.CurrentRoom.PlayerCount == 1;
                var player1 = _sceneData.Player1Name.GetComponent<TextMeshProUGUI>();
                if (_config.IsBluePlayer)
                    player1.text = $"{PhotonNetwork.NickName}(Вы)";
                else
                {
                    player1.text = PhotonNetwork.CurrentRoom.Players.Last().Value.NickName;
                    _sceneData.Player2Name.GetComponent<TextMeshProUGUI>().text = $"{PhotonNetwork.NickName}(Вы)";
                    _sceneData.DarkPanel.SetActive(false);
                    _sceneData.WaitingPanel.SetActive(false);
                    _config.PlayerTurnState = 1;
                }
                _config.Width = 39;
                _config.Height = 32;
            }
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

    private void ClosePanel()
    {
        _sceneData.RoomPanel.SetActive(false);
        _sceneData.ToolsPanel.SetActive(false);
        _sceneData.SettingsPanel.SetActive(false);
        _sceneData.InfoPanel.SetActive(false);
        _sceneData.DarkPanel.SetActive(false);
        _config.PlayerTurnState = _config.IsBluePlayer ? 1 : 2;
    }

    private void RoomPanel()
    {
        _sceneData.DarkPanel.SetActive(true);
        _sceneData.RoomPanel.SetActive(true);
        _config.PlayerTurnState = 0;
    }

    private void ToolsPanel()
    {
        _sceneData.DarkPanel.SetActive(true);
        _sceneData.ToolsPanel.SetActive(true);
        _config.PlayerTurnState = 0;
    }

    private void SettingsPanel()
    {
        _sceneData.DarkPanel.SetActive(true);
        _sceneData.SettingsPanel.SetActive(true);
        _config.PlayerTurnState = 0;
    }

    private void Room()
    {
        ClosePanel();
    }

    private void Info()
    {
        _sceneData.DarkPanel.SetActive(true);
        _sceneData.InfoPanel.SetActive(true);
        _config.PlayerTurnState = 0;
    }

    private void Leave()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    private void Field()
    {
        if (int.TryParse(_sceneData.WidthInput.GetComponent<TMP_InputField>().text, out var width) &&
            int.TryParse(_sceneData.HeightInput.GetComponent<TMP_InputField>().text, out var height) &&
            width >= 12 && width <= 40 && height >= 12 && height <= 40)
        {
            _config.Width = width;
            _config.Height = height;
            FieldSize();
            InitUpdate();
        }
        else
        {
            if (_config.Moves.Count == 0)
                InitUpdate();
        }
        _sceneData.WidthInput.GetComponent<TMP_InputField>().text = string.Empty;
        _sceneData.HeightInput.GetComponent<TMP_InputField>().text = string.Empty;
        ClosePanel();
    }

    private void DotToggle(bool isOn, int index)
    {
        if (!isOn) return;
        _config.DotSize = index;
        var dotSize = 0f;
        if (_config.DotSize == 0)
            dotSize = 0.4f;
        if (_config.DotSize == 1)
            dotSize = 0.3f;
        if (_config.DotSize == 2)
            dotSize = 0.2f;
        var newSize = new Vector3(dotSize, dotSize, 1);
        _sceneData.BlueCircle.transform.localScale = newSize;
        _sceneData.RedCircle.transform.localScale = newSize;
        foreach (Transform dot in _sceneData.DotsTransform)
            dot.localScale = newSize;
    }

    private void Dots()
    {
        _config.Tool = Tool.Dot;
        ClosePanel();
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

    private void BlueRedDots()
    {
        if ((PhotonNetwork.NickName != string.Empty && !PhotonNetwork.IsMasterClient) ||
            (PhotonNetwork.IsMasterClient && (_config.IsNoDots || !_config.IsNoBlueDot || !_config.IsNoRedDot)))
            return;
        CloseFrame();
        _sceneData.BlueRedDots.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Dot;
        var blue = _config.Current.CompareTag("Blue");
        _config.IsBluePlayer = !blue;
        _config.PlayerTurnState = blue ? 2 : 1;
        _config.IsLock = false;
    }

    private void BlueDot()
    {
        if ((PhotonNetwork.NickName != string.Empty && !PhotonNetwork.IsMasterClient) ||
            (PhotonNetwork.IsMasterClient && (_config.IsNoDots || !_config.IsNoBlueDot || !_config.IsNoRedDot)))
            return;
        CloseFrame();
        _sceneData.BlueDot.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Dot;
        _config.IsBluePlayer = true;
        _config.PlayerTurnState = 1;
        _config.IsLock = true;
    }

    private void RedDot()
    {
        if ((PhotonNetwork.NickName != string.Empty && !PhotonNetwork.IsMasterClient) ||
            (PhotonNetwork.IsMasterClient && (_config.IsNoDots || !_config.IsNoBlueDot || !_config.IsNoRedDot)))
            return;
        CloseFrame();
        _sceneData.RedDot.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Dot;
        _config.IsBluePlayer = false;
        _config.PlayerTurnState = 2;
        _config.IsLock = true;
    }

    private void Num()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Num.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Num;
        ClosePanel();
    }

    private void Cross()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Cross.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Cross;
        ClosePanel();
    }

    private void Line()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Line.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Line;
        ClosePanel();
    }

    private void Arrow()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Arrow.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Arrow;
        ClosePanel();
    }

    private void Brush()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Brush.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Brush;
        ClosePanel();
    }

    private void Eraser()
    {
        if (_config.Moves.Count == 0) return;
        CloseFrame();
        _sceneData.Eraser.transform.GetChild(0).gameObject.SetActive(true);
        _config.Tool = Tool.Eraser;
        ClosePanel();
    }

    private void NewParty()
    {
        if (PhotonNetwork.IsConnected)
        {
            InitUpdate();
            _config.IsNoDots = true;
            _config.IsNoBlueDot = false;
            _config.IsNoRedDot = false;
            _config.IsNoTools = true;
            _config.IsNoChat = true;
            var properties = PhotonNetwork.LocalPlayer.CustomProperties;
            properties["IsBlueRedDots"] = true;
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            properties["IsBlueDot"] = false;
            properties["IsRedDot"] = false;
            properties["IsTools"] = true;
            properties["IsChat"] = true;
            properties["Delete"] = string.Empty;
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        }
        else
            SceneManager.LoadScene("Game");
    }

    private void CrossToggle(bool isOn, int index)
    {
        if (!isOn) return;
        _config.CrossSize = index;
    }

    private void Pointer(bool isOn)
    {
        _config.IsPointer = isOn;
        if (isOn) return;
        foreach (Transform num in _sceneData.FieldTransform)
            if (num.CompareTag("Num"))
                num.GetChild(0).gameObject.SetActive(false);
        _sceneData.BlueCirclePhantom.position = new Vector3(-100, -100, 0);
        _sceneData.RedCirclePhantom.position = new Vector3(-100, -100, 0);
    }

    private void PlayGame()
    {
        _config.IsCoach = false;
        _config.PlayerTurnState = 0;
        _config.SecondsPlayer1 = 600;
        _config.SecondsPlayer2 = 600;
        SceneManager.LoadScene("Loading");
    }

    private void CoachGroup(bool isGroup)
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
            _config.PlayerName = PhotonNetwork.NickName;
            PhotonNetwork.NickName = string.Empty;
            PhotonNetwork.Disconnect();
        }
        _config.IsCoach = true;
        _config.IsGroup = isGroup;
        SceneManager.LoadScene("Loading");
    }

    private void UploadSgf()
    {
        var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", "sgf", false);
        if (path.Count() == 0) return;
        var text = File.ReadAllText(path[0]);
        var sgfTree = SgfParser.ParseTree(text);
        _config.SgfSystem.GetSgfSize(sgfTree);
        FieldSize();
        _config.CrossSize = 0;
        InitUpdate();
        _config.SgfSystem.UploadSgfTree(sgfTree);
        ClosePanel();
    }

    private void SaveSgf()
    {
        var path = StandaloneFileBrowser.SaveFilePanel("Save File", "", "", "sgf");
        if (path == string.Empty) return;
        File.WriteAllText(path, _config.SgfSystem.DownloadSgfTree());
        ClosePanel();
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
}
