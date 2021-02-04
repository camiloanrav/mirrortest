using System;
using System.Security.Cryptography;
using System.Text;
using Mirror;
using UnityEngine;

namespace CardDragging {

    [System.Serializable]
    public class Match {
        public string matchID;
        public bool publicMatch;
        public bool inMatch;
        public bool matchFull;
        public SyncListGameObject players = new SyncListGameObject ();

        public Match (string matchID, GameObject player, bool publicMatch) {
            matchFull = false;
            inMatch = false;
            this.matchID = matchID;
            this.publicMatch = publicMatch;
            players.Add (player);
        }

        public Match () { }
    }

    [System.Serializable]
    public class SyncListGameObject : SyncList<GameObject> { }

    [System.Serializable]
    public class SyncListMatch : SyncList<Match> { }

    public class MatchMaker : NetworkBehaviour {

        public static MatchMaker instance;

        public SyncListMatch matches = new SyncListMatch ();
        public SyncListString matchIDs = new SyncListString ();

        [SerializeField] GameObject turnManagerPrefab;
        [SerializeField] int maxMatchPlayers = 12;

        void Start () {
            instance = this;
        }

        public bool HostGame (string _matchID, GameObject _player, bool publicMatch, out int playerIndex) {
            playerIndex = -1;

            if (!matchIDs.Contains (_matchID)) {
                matchIDs.Add (_matchID);
                Match match = new Match (_matchID, _player, publicMatch);
                matches.Add (match);
                Debug.Log ($"Match generated");
                _player.GetComponent<Player> ().currentMatch = match;
                playerIndex = 1;
                return true;
            } else {
                Debug.Log ($"Match ID already exists");
                return false;
            }
        }

        public bool JoinGame (string _matchID, GameObject _player, out int playerIndex) {
            playerIndex = -1;

            if (matchIDs.Contains (_matchID)) {

                for (int i = 0; i < matches.Count; i++) {
                    if (matches[i].matchID == _matchID) {
                        if (!matches[i].inMatch && !matches[i].matchFull) {
                            matches[i].players.Add (_player);
                            _player.GetComponent<Player> ().currentMatch = matches[i];
                            playerIndex = matches[i].players.Count;

                            if (matches[i].players.Count == maxMatchPlayers) {
                                matches[i].matchFull = true;
                            }
                        } else {
                            return false;
                        }
                        break;
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
                if (!matches[i].inMatch && !matches[i].matchFull) {
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
                        turnManager.AddPlayer (_player);
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