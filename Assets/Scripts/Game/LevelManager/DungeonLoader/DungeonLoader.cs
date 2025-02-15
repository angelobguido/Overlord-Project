﻿using System.Collections.Generic;
using System.Linq;
using Game.Events;
using Game.GameManager;
using Game.LevelGenerator.LevelSOs;
using Game.LevelManager.DungeonManager;
using Game.NarrativeGenerator.Quests;
using UnityEngine;
using Util;

namespace Game.LevelManager.DungeonLoader
{
    public class DungeonLoader : MonoBehaviour
    {
        private static Map _dungeonMap;
        public List<RoomBhv> roomPrefabs;
        public Dictionary<Coordinates, RoomBhv> roomBHVMap; //2D array for easy room indexing
        public int TotalTreasures { get; private set; }
        public static event StartMapEvent StartMapEventHandler;
        public Map LoadNewLevel(DungeonFileSo dungeonFileSo, QuestLine currentQuestLine)
        {
            Debug.Log("Loading new Level");
            LoadDungeon(dungeonFileSo);
            
            EnemyLoader.DistributeEnemiesInDungeon(_dungeonMap, currentQuestLine);
            ItemDispenser.DistributeItemsInDungeon(_dungeonMap, currentQuestLine);
            TotalTreasures = currentQuestLine.ItemParametersForQuestLine.TotalItems;
            NpcDispenser.DistributeNpcsInDungeon(_dungeonMap, currentQuestLine);

            roomBHVMap = new Dictionary<Coordinates, RoomBhv>();

            var selectedRoom = roomPrefabs[RandomSingleton.GetInstance().Random.Next(roomPrefabs.Count)];
            InstantiateRooms(selectedRoom, _dungeonMap.Dimensions);
            ConnectRoooms();
            OnStartMap(dungeonFileSo.BiomeName, _dungeonMap);

            return _dungeonMap;
        }
        
        private void OnStartMap(string mapName, Map map)
        {
            StartMapEventHandler?.Invoke(null, new StartMapEventArgs(mapName, map, TotalTreasures));
            roomBHVMap[map.StartRoomCoordinates].OnRoomEnter();
        }
        
        private void SetDestinations(Coordinates targetCoordinates, Coordinates sourceCoordinates, int orientation)
        {
            switch (orientation)
            {
                case 1:
                    roomBHVMap[sourceCoordinates].doorWest.SetDestination(roomBHVMap[targetCoordinates].doorEast);
                    roomBHVMap[targetCoordinates].doorEast.SetDestination(roomBHVMap[sourceCoordinates].doorWest);
                    break;
                case 2:
                    roomBHVMap[sourceCoordinates].doorNorth.SetDestination(roomBHVMap[targetCoordinates].doorSouth);
                    roomBHVMap[targetCoordinates].doorSouth.SetDestination(roomBHVMap[sourceCoordinates].doorNorth);
                    break;
            }
        }
        
        public List<int> CheckCorridor(Coordinates targetCoordinates)
        {
            if (!_dungeonMap.DungeonPartByCoordinates.ContainsKey(targetCoordinates)) return null;
            //Sets door
            var lockedCorridor = _dungeonMap.DungeonPartByCoordinates[targetCoordinates] as DungeonLockedCorridor;

            return lockedCorridor != null ? lockedCorridor.LockIDs : new List<int>();
        }
        
        private void LoadDungeon(DungeonFileSo dungeonFileSo)
        {
            _dungeonMap = new Map(dungeonFileSo, null);
        }

        private void InstantiateRooms(RoomBhv roomBhv, Dimensions dimensions)
        {
            foreach (var currentPart in _dungeonMap.DungeonPartByCoordinates.Values.OfType<DungeonRoom>())
            {
                var newRoom = RoomLoader.InstantiateRoom(currentPart, roomBhv);
                CheckConnections(currentPart, newRoom, dimensions);
                roomBHVMap.Add(currentPart.Coordinates, newRoom); 
            }
        }

        private void CheckConnections(DungeonRoom dungeonRoom, RoomBhv newRoom, Dimensions dimensions)
        {
            Coordinates targetCoordinates;
            if (IsLeftEdge(dungeonRoom.Coordinates))
            { // west
                targetCoordinates = new Coordinates(dungeonRoom.Coordinates.X - 1, dungeonRoom.Coordinates.Y);
                newRoom.westDoor = CheckCorridor(targetCoordinates);
            }
            if (IsBottomEdge(dungeonRoom.Coordinates))
            { // north
                targetCoordinates = new Coordinates(dungeonRoom.Coordinates.X, dungeonRoom.Coordinates.Y - 1);
                newRoom.northDoor = CheckCorridor(targetCoordinates);
            }
            if (IsRightEdge(dungeonRoom.Coordinates, dimensions.Width))
            {
                targetCoordinates = new Coordinates(dungeonRoom.Coordinates.X + 1, dungeonRoom.Coordinates.Y);
                newRoom.eastDoor = CheckCorridor(targetCoordinates);
            }
            if (IsTopEdge(dungeonRoom.Coordinates, dimensions.Height))
            {
                targetCoordinates = new Coordinates(dungeonRoom.Coordinates.X, dungeonRoom.Coordinates.Y + 1);
                newRoom.southDoor = CheckCorridor(targetCoordinates);
            }
        }
        
        private bool IsLeftEdge(Coordinates coordinates)
        {
            return coordinates.X > 1;
        }

        private bool IsRightEdge(Coordinates coordinates, int width)
        {
            return coordinates.X < (width - 1);
        }

        private bool IsBottomEdge(Coordinates coordinates)
        {
            return coordinates.Y > 1;
        }
        private bool IsTopEdge(Coordinates coordinates, int height)
        {
            return coordinates.Y < (height - 1);
        }
        
        public void ConnectRoooms()
        {
            foreach (RoomBhv currentRoom in roomBHVMap.Values)
            {
                CreateConnectionsBetweenRooms(currentRoom);
            }
        }
        
        public void CreateConnectionsBetweenRooms(RoomBhv currentRoom)
        {
            Coordinates targetCoordinates;
            if (currentRoom.westDoor != null)
            { // west
                targetCoordinates = new Coordinates(currentRoom.roomData.Coordinates.X - 2, currentRoom.roomData.Coordinates.Y);
                SetDestinations(targetCoordinates, currentRoom.roomData.Coordinates, 1);
            }
            if (currentRoom.northDoor != null)
            { // west
                targetCoordinates = new Coordinates(currentRoom.roomData.Coordinates.X, currentRoom.roomData.Coordinates.Y - 2);
                SetDestinations(targetCoordinates, currentRoom.roomData.Coordinates, 2);
            }
        }
    }
}