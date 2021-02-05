using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorBasics {

    public class UILobby : MonoBehaviour {

        public static UILobby instance;

        public RoomList roomList;

        [Header ("Host Join")]
        [SerializeField] InputField joinMatchInput;
        [SerializeField] List<Selectable> lobbySelectables = new List<Selectable> ();
        [SerializeField] Canvas lobbyCanvas;
        [SerializeField] Canvas searchCanvas;
        bool searching = false;

        [Header ("Lobby")]
        [SerializeField] Transform UIPlayerParent;
        [SerializeField] GameObject UIPlayerPrefab;
        [SerializeField] Text matchIDText;
        [SerializeField] GameObject beginGameButton;

        GameObject localPlayerLobbyUI;

        void Start () {
            instance = this;
        }

        public void HostPublic () {
            lobbySelectables.ForEach (x => x.interactable = false);
            //PlayerNetwork.localPlayer.HostGame (true);
            roomList.SetCurrentScreen(1);
        }

        public void HostPrivate () {
            lobbySelectables.ForEach (x => x.interactable = false);
            //PlayerNetwork.localPlayer.HostGame (false);
            roomList.SetCurrentScreen(1);
        }

        public void HostSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;

                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (PlayerNetwork.localPlayer);
                matchIDText.text = matchID;
                beginGameButton.SetActive (true);
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void Join () {
            lobbySelectables.ForEach (x => x.interactable = false);
            PlayerNetwork.localPlayer.JoinGame (joinMatchInput.text.ToUpper ());
            roomList.SetCurrentScreen(1);
        }

        public void JoinFromButton(string roomID){
            PlayerNetwork.localPlayer.JoinGame (roomID.ToUpper());
            roomList.SetCurrentScreen(1);
        }

        public void JoinSuccess (bool success, string matchID) {
            if (success) {
                lobbyCanvas.enabled = true;

                if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
                localPlayerLobbyUI = SpawnPlayerUIPrefab (PlayerNetwork.localPlayer);
                matchIDText.text = matchID;
            } else {
                lobbySelectables.ForEach (x => x.interactable = true);
            }
        }

        public void DisconnectGame () {
            MatchMaker.instance.roomListManager.CallSpawnsRooms();
            if (localPlayerLobbyUI != null) Destroy (localPlayerLobbyUI);
            PlayerNetwork.localPlayer.DisconnectGame ();

            lobbyCanvas.enabled = false;
            lobbySelectables.ForEach (x => x.interactable = true);
            roomList.SetCurrentScreen(0);
            //beginGameButton.SetActive (false);
        }

        public GameObject SpawnPlayerUIPrefab (PlayerNetwork player) {
            GameObject newUIPlayer = Instantiate (UIPlayerPrefab, UIPlayerParent);
            newUIPlayer.GetComponent<UIPlayer> ().SetPlayer (player);
            newUIPlayer.transform.SetSiblingIndex (player.playerIndex - 1);

            return newUIPlayer;
        }

        public void BeginGame () {
            PlayerNetwork.localPlayer.BeginGame ();
        }

        public void SearchGame () {
            StartCoroutine (Searching ());
        }

        public void CancelSearchGame () {
            searching = false;
        }

        public void SearchGameSuccess (bool success, string matchID) {
            if (success) {
                searchCanvas.enabled = false;
                searching = false;
                JoinSuccess (success, matchID);
            }
        }

        IEnumerator Searching () {
            searchCanvas.enabled = true;
            searching = true;

            float searchInterval = 1;
            float currentTime = 1;

            while (searching) {
                if (currentTime > 0) {
                    currentTime -= Time.deltaTime;
                } else {
                    currentTime = searchInterval;
                    PlayerNetwork.localPlayer.SearchGame ();
                }
                yield return null;
            }
            searchCanvas.enabled = false;
        }

    }
}