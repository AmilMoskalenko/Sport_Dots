using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatSystem
{
    private readonly SceneData _sceneData;
    private readonly Config _config;

    private bool _isComment;
    private bool _isChat;

    public ChatSystem(SceneData sceneData, Config config)
    {
        _sceneData = sceneData;
        _config = config;
    }

    public void SendMessage()
    {
        if (PhotonNetwork.NickName == string.Empty || _isComment && !_config.IsNoTools)
        {
            if (_config.Moves.Count > 0)
            {
                var tool = PhotonNetwork.NickName == string.Empty ?
                    Object.Instantiate(_sceneData.MessagePrefab) :
                    PhotonNetwork.Instantiate(_sceneData.MessagePrefab.name, Vector3.zero, Quaternion.identity);
                _config.CommentHeight = _sceneData.Comments.GetComponent<ScrollRect>().content.sizeDelta.y;
                tool.transform.parent = _sceneData.Comments.GetComponentInChildren<VerticalLayoutGroup>().transform;
                tool.GetComponent<RectTransform>().localScale = Vector3.one;
                tool.GetComponent<TextMeshProUGUI>().text = _sceneData.InputField.GetComponent<TMP_InputField>().text;
                var key = _config.Moves.FirstOrDefault(m => m.Value == _config.Current).Key;
                var moveTree = _config.MovesTree[key];
                moveTree.transform.GetChild(2).gameObject.SetActive(true);
                if (!Mathf.Approximately(_sceneData.Comments.GetComponent<ScrollRect>().content.anchoredPosition.y, -270f))
                    _config.IsSentComment = true;
                if (_config.Symbols.ContainsKey(_config.Current))
                    _config.Symbols[_config.Current].Add(tool);
                else
                    _config.Symbols.Add(_config.Current, new List<GameObject> { tool });
                var raiseEventOptions = RaiseEventOptions.Default;
                var sendOptions = SendOptions.SendReliable;
                if (PhotonNetwork.NickName != string.Empty)
                {
                    PhotonNetwork.RaiseEvent(6, _config.Current.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    PhotonNetwork.RaiseEvent(7, tool.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    PhotonNetwork.RaiseEvent(10, _sceneData.InputField.GetComponent<TMP_InputField>().text, raiseEventOptions, sendOptions);
                }
            }
        }
        if (_isChat && !_config.IsNoChat)
        {
            var send = PhotonNetwork.Instantiate(_sceneData.MessagePrefab.name, Vector3.zero, Quaternion.identity);
            _config.ChatHeight = _sceneData.Chat.GetComponent<ScrollRect>().content.sizeDelta.y;
            send.transform.parent = _sceneData.Chat.GetComponentInChildren<VerticalLayoutGroup>().transform;
            send.GetComponent<RectTransform>().localScale = Vector3.one;
            send.GetComponent<TextMeshProUGUI>().text = $"{PhotonNetwork.NickName}: {_sceneData.InputField.GetComponent<TMP_InputField>().text}";
            var raiseEventOptions = RaiseEventOptions.Default;
            var sendOptions = SendOptions.SendReliable;
            if (PhotonNetwork.IsMasterClient)
            {
                send.transform.GetChild(0).gameObject.SetActive(true);
                send.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    PhotonNetwork.RaiseEvent(14, send.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
                    if (send.GetComponent<PhotonView>().Owner == PhotonNetwork.LocalPlayer)
                        PhotonNetwork.Destroy(send);
                });
            }
            if (!Mathf.Approximately(_sceneData.Chat.GetComponent<ScrollRect>().content.anchoredPosition.y, -270f))
                _config.IsSentChat = true;
            PhotonNetwork.RaiseEvent(15, send.GetPhotonView().ViewID, raiseEventOptions, sendOptions);
            PhotonNetwork.RaiseEvent(16, $"{PhotonNetwork.NickName}: {_sceneData.InputField.GetComponent<TMP_InputField>().text}", raiseEventOptions, sendOptions);
        }
        _sceneData.InputField.GetComponent<TMP_InputField>().text = string.Empty;
        _sceneData.InputField.GetComponent<TMP_InputField>().Select();
        _sceneData.InputField.GetComponent<TMP_InputField>().ActivateInputField();
    }

    public void ChatButton()
    {
        _sceneData.SendButton.GetComponent<Button>().interactable = !_config.IsNoChat;
        _isComment = false;
        _isChat = true;
        var colorChat = _sceneData.ChatButton.GetComponent<Image>().color;
        colorChat.a = 1;
        _sceneData.ChatButton.GetComponent<Image>().color = colorChat;
        _sceneData.ChatButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.31f, 0.31f, 0.31f);
        _sceneData.Chat.SetActive(true);
        var colorComment = _sceneData.CommentButton.GetComponent<Image>().color;
        colorComment.a = 0;
        _sceneData.CommentButton.GetComponent<Image>().color = colorComment;
        _sceneData.CommentButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Comments.SetActive(false);
        var colorPlayer = _sceneData.PlayerButton.GetComponent<Image>().color;
        colorPlayer.a = 0;
        _sceneData.PlayerButton.GetComponent<Image>().color = colorPlayer;
        _sceneData.PlayerButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Players.SetActive(false);
    }

    public void CommentButton()
    {
        _sceneData.SendButton.GetComponent<Button>().interactable = !_config.IsNoTools;
        _isComment = true;
        _isChat = false;
        var colorChat = _sceneData.ChatButton.GetComponent<Image>().color;
        colorChat.a = 0;
        _sceneData.ChatButton.GetComponent<Image>().color = colorChat;
        _sceneData.ChatButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Chat.SetActive(false);
        var colorComment = _sceneData.CommentButton.GetComponent<Image>().color;
        colorComment.a = 1;
        _sceneData.CommentButton.GetComponent<Image>().color = colorComment;
        _sceneData.CommentButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.31f, 0.31f, 0.31f);
        _sceneData.Comments.SetActive(true);
        var colorPlayer = _sceneData.PlayerButton.GetComponent<Image>().color;
        colorPlayer.a = 0;
        _sceneData.PlayerButton.GetComponent<Image>().color = colorPlayer;
        _sceneData.PlayerButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Players.SetActive(false);
    }

    public void PlayerButton()
    {
        _sceneData.SendButton.GetComponent<Button>().interactable = false;
        _isComment = false;
        _isChat = false;
        var colorChat = _sceneData.ChatButton.GetComponent<Image>().color;
        colorChat.a = 0;
        _sceneData.ChatButton.GetComponent<Image>().color = colorChat;
        _sceneData.ChatButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Chat.SetActive(false);
        var colorComment = _sceneData.CommentButton.GetComponent<Image>().color;
        colorComment.a = 0;
        _sceneData.CommentButton.GetComponent<Image>().color = colorComment;
        _sceneData.CommentButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.83f, 0.83f, 0.83f);
        _sceneData.Comments.SetActive(false);
        var colorPlayer = _sceneData.PlayerButton.GetComponent<Image>().color;
        colorPlayer.a = 1;
        _sceneData.PlayerButton.GetComponent<Image>().color = colorPlayer;
        _sceneData.PlayerButton.GetComponentInChildren<TextMeshProUGUI>().color = new Color(0.31f, 0.31f, 0.31f);
        _sceneData.Players.SetActive(true);
    }

    public void BlueRedDots(Player player, Transform admin)
    {
        var frame = admin.GetChild(0).GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        if (frame.activeSelf)
        {
            admin.GetChild(1).GetChild(0).gameObject.SetActive(false);
            admin.GetChild(2).GetChild(0).gameObject.SetActive(false);
            foreach (Transform p in _sceneData.ListNames)
            {
                var v = p.GetChild(1);
                if (v != admin)
                {
                    v.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    v.GetChild(1).GetChild(0).gameObject.SetActive(false);
                    v.GetChild(2).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            properties["IsBlueRedDots"] = p == player ? frame.activeSelf : frame.activeSelf ? false : p.IsMasterClient;
            properties["IsBlueDot"] = false;
            properties["IsRedDot"] = false;
            p.SetCustomProperties(properties);
        }
    }

    public void BlueDot(Player player, Transform admin)
    {
        var frame = admin.GetChild(1).GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        if (frame.activeSelf)
        {
            admin.GetChild(0).GetChild(0).gameObject.SetActive(false);
            admin.GetChild(2).GetChild(0).gameObject.SetActive(false);
            foreach (Transform p in _sceneData.ListNames)
            {
                var v = p.GetChild(1);
                if (v != admin)
                {
                    v.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    v.GetChild(1).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        var prop = player.CustomProperties;
        if (prop.TryGetValue("IsRedDot", out var isR) && (bool)isR)
        {
            prop["IsRedDot"] = false;
            player.SetCustomProperties(prop);
        }
        var isRedDot = false;
        var isRedDotM = false;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            if (properties.TryGetValue("IsRedDot", out var isRed) && (bool)isRed)
            {
                isRedDot = true;
                if (p.IsMasterClient)
                    isRedDotM = true;
            }
        }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            properties["IsBlueDot"] = p == player ? frame.activeSelf : frame.activeSelf ? false : p.IsMasterClient;
            if (p.IsMasterClient && !isRedDot && frame.activeSelf)
                properties["IsRedDot"] = true;
            p.SetCustomProperties(properties);
        }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            if (!frame.activeSelf && isRedDotM)
                properties["IsBlueRedDots"] = p.IsMasterClient;
            if (frame.activeSelf)
                properties["IsBlueRedDots"] = false;
            p.SetCustomProperties(properties);
        }
    }

    public void RedDot(Player player, Transform admin)
    {
        var frame = admin.GetChild(2).GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        if (frame.activeSelf)
        {
            admin.GetChild(0).GetChild(0).gameObject.SetActive(false);
            admin.GetChild(1).GetChild(0).gameObject.SetActive(false);
            foreach (Transform p in _sceneData.ListNames)
            {
                var v = p.GetChild(1);
                if (v != admin)
                {
                    v.GetChild(0).GetChild(0).gameObject.SetActive(false);
                    v.GetChild(2).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
        var prop = player.CustomProperties;
        if (prop.TryGetValue("IsBlueDot", out var isB) && (bool)isB)
        {
            prop["IsBlueDot"] = false;
            player.SetCustomProperties(prop);
        }
        var isBlueDot = false;
        var isBlueDotM = false;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            if (properties.TryGetValue("IsBlueDot", out var isBlue) && (bool)isBlue)
            {
                isBlueDot = true;
                if (p.IsMasterClient)
                    isBlueDotM = true;
            }
        }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            properties["IsRedDot"] = p == player ? frame.activeSelf : frame.activeSelf ? false : p.IsMasterClient;
            if (p.IsMasterClient && !isBlueDot && frame.activeSelf)
                properties["IsBlueDot"] = true;
            p.SetCustomProperties(properties);
        }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            if (!frame.activeSelf && isBlueDotM)
                properties["IsBlueRedDots"] = p.IsMasterClient;
            if (frame.activeSelf)
                properties["IsBlueRedDots"] = false;
            p.SetCustomProperties(properties);
        }
    }

    public void Tools(Player player, Transform admin)
    {
        var frame = admin.GetChild(3).GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        if (frame.activeSelf)
            foreach (Transform p in _sceneData.ListNames)
            {
                var v = p.GetChild(1);
                if (v != admin)
                    v.GetChild(3).GetChild(0).gameObject.SetActive(false);
            }
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            properties["IsTools"] = p == player ? frame.activeSelf : frame.activeSelf ? false : p.IsMasterClient;
            p.SetCustomProperties(properties);
        }
    }

    public void Chat(Player player, Transform admin)
    {
        var frame = admin.GetChild(4).GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        var properties = player.CustomProperties;
        properties["IsChat"] = frame.activeSelf;
        player.SetCustomProperties(properties);
    }

    public void Delete(Player player)
    {
        foreach (var p in PhotonNetwork.PlayerList)
        {
            var properties = p.CustomProperties;
            properties["Delete"] = player.NickName;
            p.SetCustomProperties(properties);
        }
    }

    public void Lock(Transform lockTr)
    {
        var frame = lockTr.GetChild(0).gameObject;
        frame.SetActive(!frame.activeSelf);
        var roomProperties = new Hashtable { ["Lock"] = frame.activeSelf };
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }
}
