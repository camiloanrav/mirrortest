using System;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;
using System.Linq;

namespace MirrorBasics {
  
    [System.Serializable]
    public class Match {
        public string matchID;
        public bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public int totalPlayersInMatch;
        public SyncListGameObject players = new SyncListGameObject ();

        public Match (string matchID, GameObject player, bool publicMatch) {
            matchFull = false;
            inMatch = false;
            this.matchID = matchID;
            this.publicMatch = publicMatch;
            players.Add (player);
            totalPlayersInMatch = players.Count;
        }

        public Match () { }
    }

    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> { }

    [System.Serializable]
    public class SyncListMatch : SyncList<Match> { }

    public class MatchMaker : NetworkBehaviour {

        public static MatchMaker instance;

        public RoomList roomListManager;

        public SyncListMatch matches = new SyncListMatch ();
        public SyncListString matchIDs = new SyncListString ();

        [SerializeField] GameObject turnManagerPrefab;
        [SerializeField] int maxMatchPlayers = 12;

        void Start () {
            instance = this;
        }

        public int GetMaxMatchPlayers(){
            return maxMatchPlayers;
        }

        [Command]
        public void CmdAddPlayer(GameObject _player, Match match){
            match.players.Add(_player);
            match.totalPlayersInMatch = match.players.Count;
        }

        [ClientRpc]
        public void RpcPrint(GameObject _player,Match match){
            Debug.Log("Player: " + _player);
            Debug.Log("Total Players: " + match.players.Count);
        }

        [ClientRpc]
        public void RpcRemovePlayer(GameObject _player, Match match){
            match.players.Remove(_player);
            match.totalPlayersInMatch = match.players.Count;
        }

        public bool HostGame (string _matchID, GameObject _player, bool publicMatch, out int playerIndex) {
            playerIndex = -1;

            if (!matchIDs.Contains (_matchID)) {
                matchIDs.Add (_matchID);
                Match match = new Match (_matchID, _player, publicMatch);
                matches.Add (match);
                Debug.Log ($"Match generated");
                _player.GetComponent<Player> ().currentMatch = match;

                for (int i = 0; i < matches.Count; i++)
                {
                    if(matches[i].matchID == _matchID){
                        roomListManager.FillListHost(matches[i].players.ToArray(), _matchID);
                    }
                }

                playerIndex = 1;
                return true;
            } else {
                Debug.Log ($"Match ID already exists");
                return false;
            }
        }

        public bool CheckPlayer(GameObject _player, string _matchID){
            bool isAlredyInRoom = false;
            int index = 0;

            for (int j = 0; j < matches.Count; j++)
            {
                if(matches[j].matchID == _matchID){
                    index = j;
                }
            }

            for (int i = 0; i < roomListManager.roomData[index].roomPlayers.Length; i++)
            {
                if(_player.GetComponent<NetworkIdentity>().netId == roomListManager.roomData[index].roomPlayers[i].GetComponent<NetworkIdentity>().netId){
                    isAlredyInRoom = true;
                }else{
                    isAlredyInRoom = false;
                }
            }
            return isAlredyInRoom;
        }

        public bool JoinGame (string _matchID, GameObject _player, out int playerIndex) {
            playerIndex = -1;

            if (matchIDs.Contains (_matchID) && !CheckPlayer(_player,_matchID)) {

                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].matchID == _matchID) {
                        if (!matches[i].inMatch && !matches[i].matchFull) {
                            matches[i].players.Add (_player);
                            roomListManager.RpcFillList(matches[i].players.ToArray(),matches[i].matchID);
                            _player.GetComponent<Player> ().currentMatch = matches[i];
                            playerIndex = matches[i].players.Count;

                            if (matches[i].players.Count == maxMatchPlayers) {
                                matches[i].matchFull = true;
                            }

                            break;
                        } else {
                            return false;
                        }
                    }
                }

                Debug.Log ($"Match joined");
                return true;
            } else {
                Debug.Log ($"Match ID does not exist");
                return false;
            }
        }

        public bool SearchGame (GameObject _player, out int playerIndex, out string matchID) {
            playerIndex = -1;
            matchID = "";

            for (int i = 0; i < matches.Count; i++) {
                Debug.Log ($"Checking match {matches[i].matchID} | inMatch {matches[i].inMatch} | matchFull {matches[i].matchFull} | publicMatch {matches[i].publicMatch}");
                if (!matches[i].inMatch && !matches[i].matchFull && matches[i].publicMatch) {
                    if (JoinGame (matches[i].matchID, _player, out playerIndex)) {
                        matchID = matches[i].matchID;
                        return true;
                    }
                }
            }

            return false;
        }

        public void BeginGame (string _matchID) {
            GameObject newTurnManager = Instantiate (turnManagerPrefab);
            NetworkServer.Spawn (newTurnManager);
            newTurnManager.GetComponent<NetworkMatchChecker> ().matchId = _matchID.ToGuid ();
            TurnManager turnManager = newTurnManager.GetComponent<TurnManager> ();

            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    matches[i].inMatch = true;
                    foreach (var player in matches[i].players) {
                        Player _player = player.GetComponent<Player> ();
                        _player.StartGame ();
                    }
                    break;
                }
            }
        }

        public static string GetRandomMatchID () {
            string _id = string.Empty;
            for (int i = 0; i < 5; i++) {
                int random = UnityEngine.Random.Range (0, 36);
                if (random < 26) {
                    _id += (char) (random + 65);
                } else {
                    _id += (random - 26).ToString ();
                }
            }
            Debug.Log ($"Random Match ID: {_id}");
            return _id;
        }

        public void PlayerDisconnected (Player player, string _matchID) {
            for (int i = 0; i < matches.Count; i++) {
                if (matches[i].matchID == _matchID) {
                    int playerIndex = matches[i].players.IndexOf (player.gameObject);
                    matches[i].players.RemoveAt (playerIndex);
                    roomListManager.RpcFillList(matches[i].players.ToArray(),matches[i].matchID);
                    Debug.Log ($"Player disconnected from match {_matchID} | {matches[i].players.Count} players remaining");
                    /* player.currentMatch = new Match(); */
                    if (matches[i].players.Count == 0) {
                        Debug.Log ($"No more players in Match. Terminating {_matchID}");
                        matches.RemoveAt (i);
                        matchIDs.Remove (_matchID);
                        roomListManager.roomData.RemoveAt(i);
                    }
                    break;
                }
            }
        }

    }

    public static class MatchExtensions {
        public static Guid ToGuid (this string id) {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider ();
            byte[] inputBytes = Encoding.Default.GetBytes (id);
            byte[] hashBytes = provider.ComputeHash (inputBytes);

            return new Guid (hashBytes);
        }
    }

}