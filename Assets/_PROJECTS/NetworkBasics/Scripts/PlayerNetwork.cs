﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

namespace MirrorBasics {
    public class PlayerNetwork : NetworkBehaviour {

        #region [Global Variables]
        public static PlayerNetwork localPlayer;

        [Header("Player Info Variables")]
        [Space(10)]
        [SyncVar] public string playerName;
        public bool isLeader;
        [SyncVar] public bool isReady;

        [Header("Team Variables")]
        [Space(10)]
        public string teamID;

        [Header("Match Variables")]
        [Space(10)]
        [SyncVar] public string matchID;
        [SyncVar] public int playerIndex;
        NetworkMatchChecker networkMatchChecker;
        [SyncVar] public Match currentMatch;
        [SerializeField] GameObject playerLobbyUI;
        #endregion

        #region Start Methods
        void Awake () {
            networkMatchChecker = GetComponent<NetworkMatchChecker> ();
        }
        #endregion



        #region Custom Methods
        //Print My Player Network ID
        public void PrintIdentity(){
            Debug.Log("MyNetWork identity: // " + netIdentity.netId);
        }
        #endregion
        
        #region Callbacks
        public override void OnStartClient () {
            if (isLocalPlayer) {
                localPlayer = this;
            } else {
                Debug.Log ($"Spawning other player UI Prefab");
                //playerLobbyUI = UIManager.instance.SpawnPlayerUIPrefab (this);
            }
        }

        public override void OnStopClient () {
            Debug.Log ($"Client Stopped");
            ClientDisconnect ();
        }

        public override void OnStopServer () {
            Debug.Log ($"Client Stopped on Server");
            ServerDisconnect ();
        }
        #endregion


        #region Set username
        [Command]
        public void CmdSetUserName(string username){
            playerName = username;
            RpcUpdatePlayerName();
        }

        [ClientRpc]
        public void RpcUpdatePlayerName(){
            UIManager.instance.setUsername();
        }
        #endregion

        #region Ready Game
        [Command]
        public void CmdSetReadyState(){
            Debug.Log("Actualizando Ready State");
            isReady = !isReady;
            RpcupdateReadyStates(0);
        }

        [ClientRpc]
        public void RpcupdateReadyStates(int roomIndex){
            UIManager.instance.UpdateReadyStates(roomIndex);
        }
        #endregion







        #region Room Game Methods [Host, Join, Search, Start and Disconnect]
            #region  Host Game
            public void HostGame (bool publicMatch, int maxPlayers, Match.MatchType matchType) {
                string matchID = MatchMaker.GetRandomMatchID();
                CmdHostGame (matchID, publicMatch, maxPlayers, matchType);
                PrintIdentity();
            }

            [Command]
            void CmdHostGame (string _matchID, bool publicMatch, int maxPlayers, Match.MatchType matchType) {
                matchID = _matchID;
                if (MatchMaker.instance.HostGame(_matchID, gameObject, publicMatch, out playerIndex, maxPlayers, matchType)) {
                    Debug.Log ($"<color=green>Game hosted successfully</color>");
                    networkMatchChecker.matchId = _matchID.ToGuid ();
                    TargetHostGame (true, _matchID, playerIndex);
                } else {
                    Debug.Log ($"<color=red>Game hosted failed</color>");
                    TargetHostGame (false, _matchID, playerIndex);
                }
            }

            [TargetRpc]
            void TargetHostGame (bool success, string _matchID, int _playerIndex) {
                playerIndex = _playerIndex;
                matchID = _matchID;
                Debug.Log ($"MatchID: {matchID} == {_matchID}");
                UIManager.instance.HostSuccess(success, _matchID);
            }
            #endregion

            #region Join Game
            public void JoinGame (string _inputID) {
                CmdJoinGame (_inputID);
                PrintIdentity();
            }

            [Command]
            void CmdJoinGame (string _matchID) {
                matchID = _matchID;
                if (MatchMaker.instance.JoinGame(_matchID, gameObject, out playerIndex)) {
                    Debug.Log ($"<color=green>Game Joined successfully</color>");
                    networkMatchChecker.matchId = _matchID.ToGuid ();
                    TargetJoinGame (true, _matchID, playerIndex);
                } else {
                    Debug.Log ($"<color=red>Game Joined failed</color>");
                    TargetJoinGame (false, _matchID, playerIndex);
                }
            }

            [TargetRpc]
            void TargetJoinGame (bool success, string _matchID, int _playerIndex) {
                playerIndex = _playerIndex;
                matchID = _matchID;
                Debug.Log ($"MatchID: {matchID} == {_matchID}");
                
                UIManager.instance.JoinSuccess (success, _matchID);
                UIManager.instance.UpdateReadyStates(0); //Cambiar el 0 por el Index de la sala
            }
            #endregion

            #region Disconnect Game
            public void DisconnectGame () {
                CmdDisconnectGame ();
                PrintIdentity();
            }

            [Command]
            void CmdDisconnectGame () {
                ServerDisconnect ();
            }

            void ServerDisconnect () {
                MatchMaker.instance.PlayerDisconnected (this, matchID);
                RpcDisconnectGame ();
                networkMatchChecker.matchId = string.Empty.ToGuid ();
            }

            [ClientRpc]
            void RpcDisconnectGame () {
                ClientDisconnect ();
            }

            void ClientDisconnect () {
                if (playerLobbyUI != null) {
                    Destroy (playerLobbyUI);
                }
            }
            #endregion

            #region Search a Game
            public void SearchGame () {
                CmdSearchGame ();
            }

            [Command]
            void CmdSearchGame () {
                if (MatchMaker.instance.SearchGame (gameObject, out playerIndex, out matchID)) {
                    Debug.Log ($"<color=green>Game Found Successfully</color>");
                    networkMatchChecker.matchId = matchID.ToGuid ();
                    TargetSearchGame (true, matchID, playerIndex);
                } else {
                    Debug.Log ($"<color=red>Game Search Failed</color>");
                    TargetSearchGame (false, matchID, playerIndex);
                }
            }

            [TargetRpc]
            void TargetSearchGame (bool success, string _matchID, int _playerIndex) {
                playerIndex = _playerIndex;
                matchID = _matchID;
                Debug.Log ($"MatchID: {matchID} == {_matchID} | {success}");
                UIManager.instance.SearchGameSuccess (success, _matchID);
            }
            #endregion

            #region Start a Game  
            public void BeginGame () {
                CmdBeginGame ();
            }

            [Command]
            void CmdBeginGame () {
                MatchMaker.instance.BeginGame (matchID);
                Debug.Log ($"<color=red>Game Beginning</color>");
            }

            public void StartGame () { //Server
                TargetBeginGame ();
            }

            [TargetRpc]
            void TargetBeginGame () {
                Debug.Log ($"MatchID: {matchID} | Beginning");
                //Additively load game scene
                SceneManager.LoadScene (2, LoadSceneMode.Additive);
            }
            #endregion
        #endregion
    }
}