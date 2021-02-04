using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MirrorBasics;
using System.Linq;

public class RoomList : NetworkBehaviour {

    #region [Global Variables]    
    public Transform roomParent;
    public GameObject roomPrefab;
    public MatchMaker matchManager;
    public UILobby uiLobby;
    [SerializeField]
    public SyncListRoomData roomData = new SyncListRoomData();
    public CurrentScreen currentScreen;
    #endregion
    
    #region Start Functions  
    void Update(){
        /* if(currentScreen == CurrentScreen.lobby){
            StartCoroutine(ListRequest());
        } */
    }
    #endregion

    #region [Server Functions]
    #endregion

    #region [Client Functions]
    //Call the method SpawnRooms()
    public void CallSpawnsRooms(){
        SpawnRooms();
    }

    //Update the Player list when a Room already exist
    [ClientRpc]
    public void RpcFillList(GameObject[] roomPlayers, string roomID){
        for (int i = 0; i < roomData.Count; i++)
        {
            if(roomData.ToArray()[i].roomName.Equals(roomID))
                roomData.ToArray()[i].roomPlayers = roomPlayers;
        }
        SpawnRooms();
    }

    //Fill the player list only with the player who create the Room
    public void FillListHost(GameObject[] _player, string roomID){
        RoomData tempData = new RoomData(roomID, _player);
        roomData.Add(tempData);
        SpawnRooms();
    }

    #region UI Methods
    public void SetCurrentScreen(int index){
        switch (index)
        {
            case 0:
                currentScreen = CurrentScreen.lobby;
            break;
            case 1:
                currentScreen = CurrentScreen.inRoom;
            break;
        }
    }

    //Display the Rooms UI
    public void SpawnRooms(){
        Debug.Log("SpawnRoom");
        NetworkIdentity playerIdentity = netIdentity;

        int totalRoom = matchManager.matches.Count;
        Debug.Log(totalRoom);
        
        for (int x = 0; x < roomParent.transform.childCount; x++)
        {
            Destroy(roomParent.transform.GetChild(x).gameObject);
        }

        for (int i = 0; i < totalRoom; i++)
        {
            if(matchManager.matches[i].publicMatch){
                GameObject temporalRoom = Instantiate(roomPrefab,Vector3.one,Quaternion.identity);
                temporalRoom.transform.parent = roomParent;
                temporalRoom.transform.localScale = Vector3.one;

                temporalRoom.transform.GetChild(0).GetComponent<Text>().text = matchManager.matchIDs[i];
                temporalRoom.transform.GetChild(1).GetComponent<Text>().text = roomData.ToArray()[i].roomPlayers.Length + "/" + matchManager.GetMaxMatchPlayers();
                temporalRoom.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(()=>JoinRoom(i-1));
             }
        }
    }

    //Button event Function who Join the player to a Room
    public void JoinRoom(int i){
        uiLobby.JoinFromButton(matchManager.matchIDs[i]);
    }

    //Call the method spawnRooms() every 1.5 seconds
    private bool isUpdate = false;
    private IEnumerator ListRequest(){
        while(!isUpdate){
            isUpdate=true;
            CallSpawnsRooms();
            yield return new WaitForSeconds(1.5f);
            isUpdate=false;
        }
    }
    #endregion
    #endregion

    #region Custom Classes
    [System.Serializable]
    public class RoomData {
        public string roomName;
        public GameObject[] roomPlayers;

        public RoomData(string _roomName, GameObject[] _roomPlayers){
            roomName = _roomName;
            roomPlayers = _roomPlayers;
        }

        public RoomData(){}
    }
      
    [System.Serializable]
    public class SyncListRoomData : SyncList<RoomData> { }

    public enum CurrentScreen
    {
        lobby,inRoom
    }
     #endregion

}

