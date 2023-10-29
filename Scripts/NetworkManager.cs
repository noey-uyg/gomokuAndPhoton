using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    GameManager gameManager;

    /*MainScene ���� �����*/
    [Header("MainScene_Main")]
    public Text mainConnectText;
    public InputField inputName;

    /*LobbtScene ���� �����*/
    [Header("MainScene_Lobby")]
    public Text welcomeLobby;
    public Text currentConnectedUsers;
    public Text roomPage;
    public Button nextBtn;
    public Button prevBtn;
    public Button[] roomBtn;
    public InputField roomName;

    [Header("MainScene_RoomInfo")]
    private int currentPage = 0;
    private int maxShowPage;
    List<RoomInfo> roomList = new List<RoomInfo>();

    /*GameScene ���� �����*/
    public PhotonView pv;
    [Header("GameScene_BottomHUD")]
    public Button gameStateBtn;
    public Text[] noticeText;
    public Scrollbar noticeScrollbar;

    [Header("GameScene_RightHUD")]
    public InputField inputChat;
    public Text[] chatText;
    public Scrollbar chatScrollbar;

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GameSceneInit();
        }
        else if(SceneManager.GetActiveScene().name == "LobbyScene")
        {
            LobbySceneInit();
        }
        
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene") {
            return;
        } 
        else if (SceneManager.GetActiveScene().name == "LobbyScene")
        {
            currentConnectedUsers.text = "���� " + PhotonNetwork.CountOfPlayers + "�� ���� ��";
        }
            
    }

    //GameScene���� ����� �͵�
    void GameSceneInit()
    {
        noticeScrollbar.value = 1;
        chatScrollbar.value = 1;
    }

    //LobbyScene���� ����� �͵�
    void LobbySceneInit()
    {
        maxShowPage = roomBtn.GetLength(0);
    }

    //���� ���� �õ�
    public void ConnectServer()
    {
        mainConnectText.text = "���� ��";
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = inputName.text;
        }
    }

    //���� ���� ����
    public void DisConnectServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            gameManager.ChangeScene("MainScene");
        }
    }

    //���� ���� ���� �ݹ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� ����");
        //���� ���� ���� �� �κ�����
        PhotonNetwork.JoinLobby();
        gameManager.ChangeScene("LobbyScene");
    }

    //�κ� ���� �������� ��
    public override void OnJoinedLobby()
    {
        welcomeLobby.text = "\"" + PhotonNetwork.NickName + "\"�� ȯ���մϴ�!";
        roomList.Clear();
    }

    //���� ���� ���� �ݹ�
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("���� ���� ����");
    }

    //�� ����� �ٷ� ����
    public void CreateRoom() {
        string inputRoomName = roomName.text;
        RoomOptions options = new RoomOptions{
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable
            {
                {"GameState", false}
            },
            CustomRoomPropertiesForLobby = new string[] { "GameState" }
        };
        PhotonNetwork.CreateRoom(inputRoomName, options);
    }

    //�� ���� �ݹ�
    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����");
        for (int i = 0; i < noticeText.Length; i++)
        {
            noticeText[i].text = "";
        }
        for(int i=0; i< chatText.Length; i++)
        {
            chatText[i].text = "";
        }

        gameManager.ChangeScene("GameScene");
    }

    //�� ������
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        gameManager.ChangeScene("LobbyScene");
    }

    //�濡 ���ο� �÷��̾ ������ ��
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string temp = "<" + newPlayer.NickName + ">���� �����ϼ̽��ϴ�.";
        pv.RPC("NoticeRPC", RpcTarget.All, temp);
    }

    //�濡 �ִ� �÷��̾ ������ ��
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string temp = "<" + otherPlayer.NickName + ">���� �����ϼ̽��ϴ�.";
        pv.RPC("NoticeRPC", RpcTarget.All, temp);
        if (!gameManager.IsGameOver())
        {
            gameManager.pv.RPC("SyncOver", RpcTarget.All);
            string soloText = "�� �̻� ������ ������ �� ����, ������ �����մϴ�.";
            pv.RPC("NoticeRPC", RpcTarget.MasterClient, soloText);
        }
    }

    //���� ������Ʈ �� ������ ȣ��
    public override void OnRoomListUpdate(List<RoomInfo> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].RemovedFromList)
            {
                if (!roomList.Contains(list[i])) roomList.Add(list[i]);
                else roomList[roomList.IndexOf(list[i])] = list[i];
            }
            else if (roomList.IndexOf(list[i]) != -1) roomList.RemoveAt(roomList.IndexOf(list[i]));
        }

        UpdatePage();
    }

    //�� ����� �� �Ǵ� ������ �̵� ��ư Ŭ����
    public void RoomPanelBtn(int num)
    {
        if (num == -2)
        {
            currentPage--;
        }
        else if (num == -1)
        {
            currentPage++;
        }
        else {
            int selectRoomIDX = currentPage * maxShowPage + num;
            if(selectRoomIDX < roomList.Count)
            {
                PhotonNetwork.JoinRoom(roomList[selectRoomIDX].Name);
            }
        }

        UpdatePage();
    }

    //�� ���� �ؽ�Ʈ UI
    private void UpdatePage()
    {
        int maxPage = Mathf.CeilToInt((float)roomList.Count / maxShowPage);

        prevBtn.interactable = currentPage > 0;
        nextBtn.interactable = currentPage < maxPage - 1;

        roomPage.text = currentPage + "/" + maxPage;

        for (int i = 0; i < maxShowPage; i++)
        {
            int roomIndex = currentPage * maxShowPage + i;

            if(roomIndex < roomList.Count)
            {
                bool gameState = (bool)roomList[roomIndex].CustomProperties["GameState"];
                string gameStateText = gameState ? "������ " : "����� ";
                roomBtn[i].transform.GetChild(0).GetComponent<Text>().text = roomList[roomIndex].Name;
                roomBtn[i].transform.GetChild(1).GetComponent<Text>().text = gameStateText + roomList[roomIndex].PlayerCount + "/" + roomList[roomIndex].MaxPlayers;
                roomBtn[i].interactable = true;
            }
            else
            {
                roomBtn[i].interactable = false;
            }
        }
    }

    public void Send()
    {
        pv.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + inputChat.text);
        inputChat.text = "";
    }

    //Notice�� Chat RPC
    [PunRPC]
    public void SendMessage(string msg, Text[] textArray)
    {
        bool isInput = false;
        for (int i = 0; i < textArray.Length; i++)
            if (textArray[i].text == "")
            {
                if(i == textArray.Length - 5)
                {
                    if (textArray == chatText)
                    {
                        chatScrollbar.value = 0;
                    }
                    else
                    {
                        noticeScrollbar.value = 0;
                    }
                }
                isInput = true;
                textArray[i].text = msg;
                break;
            }
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < textArray.Length; i++)
            {
                textArray[i - 1].text = textArray[i].text;
            }

            textArray[textArray.Length - 1].text = msg;  
        }
    }

    [PunRPC]
    void NoticeRPC(string msg)
    {
        SendMessage(msg, noticeText);
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        SendMessage(msg, chatText);
    }

}

