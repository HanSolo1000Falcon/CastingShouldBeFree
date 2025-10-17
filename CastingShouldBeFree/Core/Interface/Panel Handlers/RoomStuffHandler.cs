using System.Collections.Generic;
using System.Linq;
using CastingShouldBeFree.Utils;
using GorillaNetworking;
using TMPro;
using UnityEngine.UI;

namespace CastingShouldBeFree.Core.Interface.Panel_Handlers;

public class RoomStuffHandler : Singleton<RoomStuffHandler>
{
    protected override void Awake()
    {
        TMP_InputField  roomNameInput  = transform.transform.Find("RoomInputField").GetComponent<TMP_InputField>();
        Button          joinRoomButton = transform.transform.Find("JoinRoom").GetComponent<Button>();
        TextMeshProUGUI roomNameText   = joinRoomButton.GetComponentInChildren<TextMeshProUGUI>();
        roomNameInput.onValueChanged.AddListener(value =>
                                                         roomNameText.text =
                                                                 $"<color=green>Join</color> Room \'{FilterRoomName(value)}\'");

        transform.Find("LeaveCurrent").GetComponent<Button>().onClick
                 .AddListener(() => NetworkSystem.Instance.ReturnToSinglePlayer());

        joinRoomButton.onClick.AddListener(() =>
                                                   PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(
                                                           FilterRoomName(roomNameInput.text),
                                                           JoinType.Solo));

        gameObject.SetActive(false);
    }

    private string FilterRoomName(string roomName)
    {
        string fallback = "12345";

        roomName = roomName.Trim();
        roomName = roomName.ToUpper();

        char[] acceptedLetters =
        [
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
                'V', 'W', 'X', 'Y', 'Z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
        ];

        List<char> unnaceptableLetters = new(); // will be killed if your room name contains any of these!!!!

        foreach (char c in roomName)
            if (!acceptedLetters.Contains(c) && !unnaceptableLetters.Contains(c))
                unnaceptableLetters.Add(c);

        foreach (char c in unnaceptableLetters)
            roomName = roomName.Replace(c.ToString(), string.Empty);

        if (string.IsNullOrWhiteSpace(roomName))
            return fallback;

        if (GorillaComputer.instance == null)
            return fallback;

        if (!GorillaComputer.instance.CheckAutoBanListForName(roomName))
            return fallback;

        if (roomName.Length > 12)
            return roomName.Substring(0, 12);

        return roomName;
    }
}