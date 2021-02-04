using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using MirrorBasics;
using System.Linq;

public class UIManager : MonoBehaviour {

    public static UIManager instance;

    [Header("Global Vars")]
    [Space(10)]
    public RoomList roomList;
    public CurrentScreen currentScreen;
    public List<GameObject> screens = new List<GameObject>();

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
}
