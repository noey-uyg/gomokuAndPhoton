using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    GameManager gameManager;

    /*MainScene ���� �����*/
    [Header("MainScene_Main")]
    public GameObject lobbyPanel;
    public Text text;
    public InputField inputName;

    [Header("MainScene_Lobby")]
    public GameObject mainPanel;
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
    private bool isGameStart = false;

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
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            GameSceneInit();
        }
        else if(SceneManager.GetActiveScene().name == "MainScene")
        {
            MainSceneInit();
        }
        
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene") return;
        else if (SceneManager.GetActiveScene().name == "MainScene")
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

    //MainScene���� ����� �͵�
    void MainSceneInit()
    {
        gameManager = GameManager.Instance;
        lobbyPanel.transform.localScale = Vector3.zero;
        mainPanel.transform.localScale = Vector3.one;

        maxShowPage = roomBtn.GetLength(0);
    }
    //���� ���� �õ�
    public void ConnectServer()
    {
        text.text = "���� ��";
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    //���� ���� ����
    public void DisConnectServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    //���� ���� ���� �ݹ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� ����");
        //���� ���� ���� �� �κ�����
        PhotonNetwork.JoinLobby();
    }

    //�κ� ���� �������� ��
    public override void OnJoinedLobby()
    {
        PhotonNetwork.NickName = inputName.text;
        lobbyPanel.transform.localScale = Vector3.one;
        mainPanel.transform.localScale = Vector3.zero;
        welcomeLobby.text = "\"" + PhotonNetwork.NickName + "\"�� ȯ���մϴ�!";
        roomList.Clear();
    }

    //���� ���� ���� �ݹ�
    public override void OnDisconnected(DisconnectCause cause)
    {
        lobbyPanel.transform.localScale = Vector3.zero;
        mainPanel.transform.localScale = Vector3.one;
        Debug.Log("���� ���� ����");
    }

    //�� ����� �ٷ� ����
    public void CreateRoom() {
        string inputRoomName = roomName.text;
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
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
                string gameState = isGameStart ? "������ " : "����� ";
                roomBtn[i].transform.GetChild(0).GetComponent<Text>().text = roomList[roomIndex].Name;
                roomBtn[i].transform.GetChild(1).GetComponent<Text>().text = gameState + roomList[roomIndex].PlayerCount + "/" + roomList[roomIndex].MaxPlayers;
                roomBtn[i].gameObject.SetActive(true);
            }
            else
            {
                roomBtn[i].gameObject.SetActive(false);
            }
        }
    }

    public void Send()
    {
        pv.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + inputChat.text);
        inputChat.text = "";
    }

    //�濡 �ִ� ������� RPC����
    [PunRPC]
    void NoticeRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < noticeText.Length; i++)
            if (noticeText[i].text == "")
            {
                isInput = true;
                noticeText[i].text = msg;
                break;
            }
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < noticeText.Length; i++) noticeText[i - 1].text = noticeText[i].text;
            noticeText[noticeText.Length - 1].text = msg;
        }
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < chatText.Length; i++)
            if (chatText[i].text == "")
            {
                isInput = true;
                chatText[i].text = msg;
                break;
            }
        if (!isInput) // ������ ��ĭ�� ���� �ø�
        {
            for (int i = 1; i < chatText.Length; i++) chatText[i - 1].text = chatText[i].text;
            chatText[chatText.Length - 1].text = msg;
        }
    }
}

