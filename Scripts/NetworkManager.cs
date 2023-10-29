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

    /*MainScene 변수 선언부*/
    [Header("MainScene_Main")]
    public Text mainConnectText;
    public InputField inputName;

    /*LobbtScene 변수 선언부*/
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

    /*GameScene 변수 선언부*/
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
            currentConnectedUsers.text = "현재 " + PhotonNetwork.CountOfPlayers + "명 접속 중";
        }
            
    }

    //GameScene에서 사용할 것들
    void GameSceneInit()
    {
        noticeScrollbar.value = 1;
        chatScrollbar.value = 1;
    }

    //LobbyScene에서 사용할 것들
    void LobbySceneInit()
    {
        maxShowPage = roomBtn.GetLength(0);
    }

    //서버 연결 시도
    public void ConnectServer()
    {
        mainConnectText.text = "연결 중";
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = inputName.text;
        }
    }

    //서버 연결 끊기
    public void DisConnectServer()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            gameManager.ChangeScene("MainScene");
        }
    }

    //서버 연결 성공 콜백
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 성공");
        //서버 연결 성공 시 로비접속
        PhotonNetwork.JoinLobby();
        gameManager.ChangeScene("LobbyScene");
    }

    //로비에 접속 성공했을 때
    public override void OnJoinedLobby()
    {
        welcomeLobby.text = "\"" + PhotonNetwork.NickName + "\"님 환영합니다!";
        roomList.Clear();
    }

    //서버 연결 실패 콜백
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("서버 연결 끊김");
    }

    //방 만들고 바로 참가
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

    //방 입장 콜백
    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장 성공");
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

    //방 나가기
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        gameManager.ChangeScene("LobbyScene");
    }

    //방에 새로운 플레이어가 들어왔을 때
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        string temp = "<" + newPlayer.NickName + ">님이 참가하셨습니다.";
        pv.RPC("NoticeRPC", RpcTarget.All, temp);
    }

    //방에 있던 플레이어가 나갔을 때
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        string temp = "<" + otherPlayer.NickName + ">님이 퇴장하셨습니다.";
        pv.RPC("NoticeRPC", RpcTarget.All, temp);
        if (!gameManager.IsGameOver())
        {
            gameManager.pv.RPC("SyncOver", RpcTarget.All);
            string soloText = "더 이상 게임을 진행할 수 없어, 게임을 종료합니다.";
            pv.RPC("NoticeRPC", RpcTarget.MasterClient, soloText);
        }
    }

    //방이 업데이트 될 때마다 호출
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

    //방 목록의 방 또는 페이지 이동 버튼 클릭시
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

    //방 정보 텍스트 UI
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
                string gameStateText = gameState ? "게임중 " : "대기중 ";
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

    //Notice와 Chat RPC
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
        if (!isInput) // 꽉차면 한칸씩 위로 올림
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

