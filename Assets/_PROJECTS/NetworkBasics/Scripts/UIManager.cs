using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MirrorBasics;
using System.Linq;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

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
        selectScreen(1);
    }
    #endregion

    #region Custom Methods

    #region UI Methods
    public void UpdateUI(){
        //Disable all Screens
        for (int i = 0; i < screens.Count; i++) {
            screens[i].SetActive(false);
        }

        switch (currentScreen) {   

            case CurrentScreen.username:
                screens[0].SetActive(true); //Active Username Input
                screens[1].SetActive(true); //Active Username Input
            break;

            case CurrentScreen.Menu:
                screens[1].SetActive(true); //Active Username Input
            break;

            case CurrentScreen.Rooms:
                screens[2].SetActive(true); //Active Username Input

                //Display Rooms UI 
                for (int i = 0; i < roomsPrefabs.Count; i++) {
                    
                }
            break;

            case CurrentScreen.Teams:
                screens[3].SetActive(true); //Active Username Input

                //Change Text and Inputs
                teamUI.teamID.text = "Bla Bla..";
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
                temporalRoom.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(()=>JoinRoom(i-1));
             }
        }
    }

    public void JoinRoom(int i){
        //uiLobby.JoinFromButton(matchManager.matchIDs[i]);
    }
    #endregion

    #endregion

    #region Select Username
    public void setUsername(){
        //Leer el Input de Username
        playerNameText.text = playerName.text;

        //Select and Update UI
        selectScreen(1);
    }
    #endregion

    #region Join Single Player
    public void joinRandomGame(){

    }
    #endregion

    #region Create Team
    public void createTeam(int quantity){

    }
    #endregion

    #region Join Team
    public void joinTeam(){
        //Select screen and Update UI
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
        username, Menu, Rooms, Teams
    }
    #endregion



    #region Importaciones




    #region Host Games [Public and Private]
    public void HostPublic() {
        lobbySelectables.ForEach(x => x.interactable = false);
    }

    public void HostPrivate() {
        lobbySelectables.ForEach(x => x.interactable = false);
        PlayerNetwork.localPlayer.HostGame(false);
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

            //Set First Player Name
            teamUI.playersName[0].text = PlayerNetwork.localPlayer.playerName;
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

    public void JoinFromButton(string roomID) {
        PlayerNetwork.localPlayer.JoinGame(roomID.ToUpper());
    }


    public void JoinSuccess(bool success, string matchID) {
        if (success) {
            
            //Enabled Team Room Panel
            selectScreen(3);
            //lobbyCanvas.enabled = true;

            //Set Team Room INFO
            teamUI.teamID.text = matchID;
            teamUI.readyButton.enabled = true;

            for (int i = 0; i < roomList.roomData.Count; i++) {
                if (roomList.roomData[i].roomName.Equals(matchID)) {
                    for (int k = 0; k < roomList.roomData[i].roomPlayers.Length; k++) {
                        if (roomList.roomData[i].roomPlayers[k] == null) { //CHECKING!
                            teamUI.readyText[k].text = roomList.roomData[i].roomPlayers[k].GetComponent<PlayerNetwork>().playerName;
                        }
                    }
                } 
            }

        } else {
            lobbySelectables.ForEach(x => x.interactable = true);
        }
    }
    #endregion


    #region Disconnect, Begin and Search
    public void DisconnectGame() {
        //MatchMaker.instance.roomListManager.CallSpawnsRooms();
        SpawnRooms();
        PlayerNetwork.localPlayer.DisconnectGame();

        lobbyCanvas.enabled = false;
        lobbySelectables.ForEach(x => x.interactable = true);
        roomList.SetCurrentScreen(0);
        //beginGameButton.SetActive (false);
    }

    public GameObject SpawnPlayerUIPrefab(Player player)
    {
        GameObject newUIPlayer = Instantiate(UIPlayerPrefab, UIPlayerParent);
        newUIPlayer.GetComponent<UIPlayer>().SetPlayer(player);
        newUIPlayer.transform.SetSiblingIndex(player.playerIndex - 1);

        return newUIPlayer;
    }

    public void BeginGame()
    {
        Player.localPlayer.BeginGame();
    }

    public void SearchGame()
    {
        StartCoroutine(Searching());
    }

    public void CancelSearchGame()
    {
        searching = false;
    }

    public void SearchGameSuccess(bool success, string matchID)
    {
        if (success)
        {
            searchCanvas.enabled = false;
            searching = false;
            JoinSuccess(success, matchID);
        }
    }

    IEnumerator Searching()
    {
        searchCanvas.enabled = true;
        searching = true;

        float searchInterval = 1;
        float currentTime = 1;

        while (searching)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = searchInterval;
                Player.localPlayer.SearchGame();
            }
            yield return null;
        }
        searchCanvas.enabled = false;
    }
    #endregion











    #endregion





}
