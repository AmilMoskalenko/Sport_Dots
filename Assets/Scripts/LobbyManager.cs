using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private Config _config;
    [SerializeField] private Button _back;
    [SerializeField] private GameObject _create;
    [SerializeField] private TMP_InputField _nameText;
    [SerializeField] private Button _createRoom;
    [SerializeField] private Button _enterRooms;
    [SerializeField] private GameObject _enter;
    [SerializeField] private Button _group;
    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private Transform _listRooms;
    [SerializeField] private Button _join;
    [SerializeField] private GameObject _namePrefab;
    [SerializeField] private Transform _listNames;

    private string _room;
    private bool _isLock;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        _back.onClick.AddListener(Back);
        _createRoom.onClick.AddListener(CreateRoom);
        _enterRooms.onClick.AddListener(EnterRooms);
        _group.onClick.AddListener(Group);
        _join.onClick.AddListener(Join);
        if (_config.IsGroup)
            Group();
        else
            EnterRooms();
        _nameText.text = _config.PlayerName;
    }

    private void Back()
    {
        PhotonNetwork.NickName = string.Empty;
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene("Game");
    }

    private void CreateRoom()
    {
        var roomOptions = new RoomOptions
        {
            CustomRoomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "Players", 1 },
                { "Lock", false },
                { "Width", 0 },
                { "Height", 0 }
            },
            CustomRoomPropertiesForLobby = new string[] { "Players", "Lock", "Width", "Height" },
            CleanupCacheOnLeave = false
        };
        PhotonNetwork.CreateRoom($"Комната {PhotonNetwork.CountOfRooms + 1}", roomOptions);
    }

    private void Group()
    {
        _create.SetActive(true);
        _enter.SetActive(false);
    }

    private void EnterRooms()
    {
        _create.SetActive(false);
        _enter.SetActive(true);
    }

    private void Join()
    {
        if (_room != string.Empty && !_isLock)
            PhotonNetwork.JoinRoom(_room);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.NickName = _nameText.text != string.Empty ? _nameText.text : "Игрок " + Random.Range(1, 100);
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform r in _listRooms)
            Destroy(r.gameObject);
        roomList.Reverse();
        foreach (var room in roomList)
        {
            var roomPrefab = Instantiate(_roomPrefab, _listRooms);
            roomPrefab.GetComponentInChildren<TextMeshProUGUI>().text = room.Name;
            roomPrefab.transform.GetChild(0).gameObject.SetActive(room.Name == _room);
            roomPrefab.GetComponent<Button>().onClick.AddListener(() => {PlayersList(room);});
            roomPrefab.transform.GetChild(1).gameObject.SetActive((bool)room.CustomProperties["Lock"]);
        }
    }

    private void PlayersList(RoomInfo room)
    {
        foreach (Transform r in _listRooms)
            r.GetChild(0).gameObject.SetActive(r.GetComponentInChildren<TextMeshProUGUI>().text == room.Name);
        _room = room.Name;
        foreach (Transform p in _listNames)
            Destroy(p.gameObject);
        var players = room.CustomProperties["Players"].ToString().Split(',');
        foreach (var player in players)
        {
            var namePrefab = Instantiate(_namePrefab, _listNames);
            namePrefab.GetComponentInChildren<TextMeshProUGUI>().text = player;
        }
        _isLock = (bool)room.CustomProperties["Lock"];
        _config.Width = (int)room.CustomProperties["Width"];
        _config.Height = (int)room.CustomProperties["Height"];
    }
}
