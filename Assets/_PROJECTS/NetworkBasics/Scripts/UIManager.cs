﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MirrorBasics;
using System.Linq;

public class UIManager : MonoBehaviour {
    public static UIManager instance;
    public GameObject playerprueba;
    #region Global Vars
    [Header("Global Vars")]
    [Space(10)]
    public RoomList roomList;
    public CurrentScreen currentScreen;
    public List<GameObject> screens = new List<GameObject>();
    [SerializeField] List<Selectable> lobbySelectables = new List<Selectable> ();

    [Header("Player Menu")]
    [Space(10)]
    public Text playerNameText;
    public InputField playerName;
    
    [Header("Rooms Menu")]
    [Space(10)]
    private bool searching = false;
    public GameObject roomPrefabUI;
    public GameObject roomParent;
    public List<GameObject> roomsPrefabs = new List<GameObject>();

    [Header("Team Menu")]
    [Space(10)]
    public TeamUI teamUI;
    #endregion

    #region Init Methods
    void Start() {
        instance = this;
        Init();
    }

    public void Init(){
        selectScreen(0);
    }
    #endregion


    #region Custom Methods

    #region UI Methods
    public void UpdateUI(){
        
        switch (currentScreen) {   

            case CurrentScreen.username:
                //Disable all Screens
                screens.ForEach(x => x.SetActive(false));

                screens[0].SetActive(true); //Disable username Input
                screens[1].SetActive(true); //Active Username Input
            break;

            case CurrentScreen.Menu:
                //Disable all Screens
                screens.ForEach(x => x.SetActive(false));
                screens[1].SetActive(true); //Active Username Input
            break;

            case CurrentScreen.Rooms:
                //Disable all Screens
                screens.ForEach(x => x.SetActive(false));
                
                screens[2].SetActive(true); //Active Username Input
                screens[0].SetActive(false); //Disable username Input

                //Display Rooms UI 
                for (int i = 0; i < roomsPrefabs.Count; i++) {
                    
                }
            break;

            case CurrentScreen.Teams:
                //Disable all Screens
                screens.ForEach(x => x.SetActive(false));
                screens[3].SetActive(true); //Active Username Input
            break;

            case CurrentScreen.Searching:
                screens[4].SetActive(true); //Active Username Input
            break;
        }
    }

    public void selectScreen(int index){
        switch (index){
        
            case 0:
                currentScreen = CurrentScreen.username;
            break;

            case 1:
                currentScreen = CurrentScreen.Menu;
            break;

            case 2:
                currentScreen = CurrentScreen.Rooms;
            break;

            case 3:
                currentScreen = CurrentScreen.Teams;
            break;

            case 4:
                currentScreen = CurrentScreen.Searching;
            break;
        }

        UpdateUI();
    }

    #region Spawn Rooms Method
    public void SpawnRooms(){
        Debug.Log("SpawnRoom");
        roomsPrefabs.Clear();
        //NetworkIdentity playerIdentity = netIdentity;

        int totalRoom = MatchMaker.instance.matches.Count;
        Debug.Log(totalRoom);
        
        for (int x = 0; x < roomParent.transform.childCount; x++)
        {
            Destroy(roomParent.transform.GetChild(x).gameObject);
        }

        for (int i = 0; i < totalRoom; i++) {
            if(MatchMaker.instance.matches[i].publicMatch){

                GameObject temporalRoom = Instantiate(roomPrefabUI,Vector3.one,Quaternion.identity);
                roomsPrefabs.Add(temporalRoom);

                temporalRoom.transform.parent = roomParent.transform;
                temporalRoom.transform.localScale = Vector3.one;

                temporalRoom.transform.GetChild(0).GetComponent<Text>().text = MatchMaker.instance.matchIDs[i];
                temporalRoom.transform.GetChild(1).GetComponent<Text>().text = roomList.roomData.ToArray()[i].roomPlayers.Length + "/" + MatchMaker.instance.GetMaxMatchPlayers();
                //temporalRoom.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(()=>JoinRoom(i-1));
             }
        }
    
        //playerprueba = (GameObject)roomList.roomData.ToArray()[0].roomPlayers[0];
        //Debug.Log("Lider!: " + RoomList.instance.roomData.ToArray()[0].roomPlayers[0].name);
    }

    #endregion

    #endregion

    #region Select Username
    public void setUsernameButton(){
        PlayerNetwork.localPlayer.CmdSetUserName(playerName.text);
    }

    public void setUsername(){
        //Leer el Input de Username
        playerNameText.text = playerName.text;
        //Select and Update UI
        selectScreen(1);
    }
    #endregion

    
    #region Host Games [Public and Private]
    public void HostPublicGame() {
        //lobbySelectables.ForEach(x => x.interactable = false);
        PlayerNetwork.localPlayer.HostGame(true, 50, Match.MatchType.game);
    }

    public void HostTeamSquad(){
        lobbySelectables.ForEach(x => x.interactable = false);
        PlayerNetwork.localPlayer.HostGame(true, 4, Match.MatchType.team); //SIEMPRE DEBE SER FALSE!!
    }

    public void HostSuccess(bool success, string matchID) {
        if (success) {
            //lobbyCanvas.enabled = true;
            selectScreen(3);

            //Set Team Room INFO
            teamUI.teamID.text = matchID;
            teamUI.readyButton.enabled = true;
            teamUI.readyText.ForEach(x => x.text = "Not Ready..");  
            teamUI.playersName.ForEach(x => x.text = "Waiting..");  

            PlayerNetwork.localPlayer.isReady = false;
            PlayerNetwork.localPlayer.CmdSetReadyState();
            teamUI.readyButton.onClick.AddListener(()=>PlayerNetwork.localPlayer.CmdSetReadyState());

            //Set First Player Name
            teamUI.playersName[0].text = PlayerNetwork.localPlayer.playerName;

            
            Debug.Log("Prueba camilo: " + roomList.roomData.Count);
        } else {
            lobbySelectables.ForEach(x => x.interactable = true);
            //Print message who say: "Cant Create Team Room"
        }
    }
    #endregion

    #region Join Game 
    public void Join() {
        lobbySelectables.ForEach(x => x.interactable = false);
        PlayerNetwork.localPlayer.JoinGame(teamUI.teamIDField.text.ToUpper());
    }

    public void JoinTeam() {
        lobbySelectables.ForEach(x => x.interactable = false);
        PlayerNetwork.localPlayer.JoinGame(teamUI.teamIDField.text.ToUpper());
        Debug.Log("Jugadores: " + roomList.roomData[0].roomPlayers.Length);
    }

    public void JoinFromButton(string roomID) {
        PlayerNetwork.localPlayer.JoinGame(roomID.ToUpper());
    }

    public void JoinSuccess(bool success, string matchID) {
        if (success) {     
            //Enabled Team Room Panel
            selectScreen(3);

            //Set Team Room INFO
            teamUI.teamID.text = matchID;
            teamUI.readyButton.enabled = true;

            //Verify Room
            for (int i = 0; i < roomList.roomData.Count; i++) {
                if (roomList.roomData[i].roomName.Equals(matchID)) {

                    int lastIndex = roomList.roomData[i].roomPlayers.Length-1;
                    
                    teamUI.playersName[lastIndex].text = PlayerNetwork.localPlayer.playerName;
                    //teamUI.readyText[lastIndex].text = roomList.roomData[roomIndex].roomPlayers[lastIndex].GetComponent<PlayerNetwork>().playerName;
                    PlayerNetwork.localPlayer.isReady = false;
                    teamUI.readyText[lastIndex].text = "Not Ready..";

                    //PlayerNetwork.localPlayer.CmdSetReadyState();
                    teamUI.readyButton.onClick.AddListener(() => PlayerNetwork.localPlayer.CmdSetReadyState());
                    Debug.Log("Match ID: " +roomList.matchManager.matches[0].matchID);
                } 
            }

        } else {
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }
    #endregion

    #region Ready Game
    //Este metodo es el que actualiza los nombres y si estan listos en la room
    public void UpdateReadyStates(int roomIndex){
        Debug.Log("Probando aca!!!!!!!!!!!");
        
        for (int i = 0; i < roomList.roomData.ToArray()[roomIndex].roomPlayers.Length; i++) {      
            //Update Player Name
            playerprueba = roomList.roomData.ToArray()[roomIndex].roomPlayers[i];
            teamUI.playersName[i].text = roomList.roomData.ToArray()[roomIndex].roomPlayers[i].gameObject.GetComponent<PlayerNetwork>().playerName;

            //If is Ready or not
            if (roomList.roomData[roomIndex].roomPlayers[i].gameObject.GetComponent<PlayerNetwork>().isReady) {
                teamUI.readyText[i].text = "¡Ready!";
            }else{
                teamUI.readyText[i].text = "Not Ready..";
            }
        }
    }
    #endregion

    #region Disconnect, Begin and Search
    public void DisconnectGame() {
        //MatchMaker.instance.roomListManager.CallSpawnsRooms();
        SpawnRooms();
        PlayerNetwork.localPlayer.DisconnectGame();
        
        lobbySelectables.ForEach(x => x.interactable = true);
        selectScreen(1);
        teamUI.readyButton.enabled = false;
    }

    /*public GameObject SpawnPlayerUIPrefab(Player player) {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);

        return newUIPlayer;
    }*/

    public void BeginGame() {
        PlayerNetwork.localPlayer.BeginGame();
    }

    public void SearchGame() {
        StartCoroutine(Searching());
    }

    public void CancelSearchGame() {
        searching = false;
        selectScreen(1);
    }

    public void SearchGameSuccess(bool success, string matchID) {
        if (success)  {
            selectScreen(3);
            searching = false;
            JoinSuccess(success, matchID);
        }
    }

    IEnumerator Searching() {
        selectScreen(4);
        searching = true;

        float searchInterval = 1;
        float currentTime = 1;

        while (searching) {
            if (currentTime > 0) {
                currentTime -= Time.deltaTime;
            } else {
                currentTime = searchInterval;
                PlayerNetwork.localPlayer.SearchGame();
            }
            yield return null;
        }
        selectScreen(3);
    }
    #endregion

    #endregion


    #region Custom Classes
    [System.Serializable]
    public class TeamUI{
        public Text teamID;
        public InputField teamIDField;
        public Button readyButton;
        public List<Text> playersName = new List<Text>();
        public List<Text> readyText = new List<Text>();
    }

    [System.Serializable]
    public class roomUI{
        public Text roomID;
        public Text roomQuantity;
        public Button roomJoin;
    }
    
    #endregion


    #region Custom Enums
    public enum CurrentScreen{
        username, Menu, Rooms, Teams, Searching
    }
    #endregion
}
