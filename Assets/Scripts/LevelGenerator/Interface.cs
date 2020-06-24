using LevelGenerator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace LevelGenerator
{
    class Interface
    {
        /*
         * Prints the tree in the command line in a pretty structure
         */
        public static void PrintTree(Room root)
        {
            string typeString = "?";
            Room first = root;
            List<Room> firstStack = new List<Room>();
            List<Room> aux;
            firstStack.Add(first);

            List<List<Room>> childListStack = new List<List<Room>>();
            childListStack.Add(firstStack);

            while (childListStack.Count > 0)
            {
                List<Room> childStack = childListStack[childListStack.Count - 1];

                if (childStack.Count == 0)
                {
                    childListStack.RemoveAt(childListStack.Count - 1);
                }
                else
                {
                    first = childStack[0];
                    childStack.RemoveAt(0);

                    string indent = "";
                    for (int i = 0; i < childListStack.Count - 1; i++)
                    {
                        indent += (childListStack[i].Count > 0) ? "|  " : "   ";
                    }
                    //Sets the string representing the type of the room accordingly
                    Type type = first.Type;
                    if (type == Type.normal)
                        typeString = "N";
                    else if (type == Type.key)
                        typeString = "K";
                    else if (type == Type.locked)
                        typeString = "L";
                    else
                    {
                        Console.WriteLine("Something went wrong printing the tree!\n");
                        Console.WriteLine("This Room type does not exist!\n\n");
                    }
                    Console.WriteLine(indent + "+- " + first.RoomId + "-" + typeString);

                    aux = new List<Room>();

                    if (first.LeftChild != null)
                        aux.Add(first.LeftChild);
                    if (first.BottomChild != null)
                        aux.Add(first.BottomChild);
                    if (first.RightChild != null)
                        aux.Add(first.RightChild);

                    if (aux.Count > 0)
                    {
                        childListStack.Add(aux);
                    }
                }
            }
            Console.Write("\n");
        }

        public static void PrintGrid(RoomGrid grid)
        {
            Room actualRoom;
            Type type;
            for (int i = -Constants.MATRIXOFFSET; i < Constants.MATRIXOFFSET; ++i)
            {
                for (int j = -Constants.MATRIXOFFSET; j < Constants.MATRIXOFFSET; ++j)
                {
                    actualRoom = grid[i, j];
                    if (actualRoom != null)
                    {
                        type = actualRoom.Type;
                        if (type == Type.normal)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.Write("N-");
                        }
                        else if (type == Type.key)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("K-");
                        }
                        else if (type == Type.locked)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            Console.Write("L-");
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong printing the tree!\n");
                            Console.WriteLine("This Room type does not exist!\n\n");
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("O-");
                    }
                }
                Console.Write("\n");
            }
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintGridWithConnections(RoomGrid grid)
        {
            int size = Constants.MATRIXOFFSET * 4;
            int gridSize = Constants.MATRIXOFFSET * 2;
            char[,] map = new char[size, size];
            Room actualRoom, parent;
            Type type;
            int x, y, iPositive, jPositive;

            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    map[i, j] = ' ';
                }
            }

            for (int i = -Constants.MATRIXOFFSET; i < Constants.MATRIXOFFSET; ++i)
            {
                for (int j = -Constants.MATRIXOFFSET; j < Constants.MATRIXOFFSET; ++j)
                {
                    iPositive = i + Constants.MATRIXOFFSET;
                    jPositive = j + Constants.MATRIXOFFSET;
                    actualRoom = grid[i, j];
                    if (actualRoom != null)
                    {
                        type = actualRoom.Type;
                        if (type == Type.normal)
                        {
                            map[iPositive * 2, jPositive * 2] = 'N';
                        }
                        else if (type == Type.key)
                        {
                            map[iPositive * 2, jPositive * 2] = 'K';
                        }
                        else if (type == Type.locked)
                        {
                            map[iPositive * 2, jPositive * 2] = 'L';
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong printing the tree!\n");
                            Console.WriteLine("This Room type does not exist!\n\n");
                        }
                        parent = actualRoom.Parent;
                        if (parent != null)
                        {

                            x = parent.X - actualRoom.X + 2 * iPositive;
                            y = parent.Y - actualRoom.Y + 2 * jPositive;
                            map[x, y] = 'c';
                        }
                    }
                    else
                    {
                        //map[iPositive * 2, jPositive * 2] = 'O';
                    }
                }
            }
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    if (map[i, j] == 'N')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;

                    }
                    else if (map[i, j] == 'K')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    else if (map[i, j] == 'L')
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }
                    else if (map[i, j] == 'c')
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else if (map[i, j] == 's')
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if (i == Constants.MATRIXOFFSET * 2 && j == Constants.MATRIXOFFSET * 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    Console.Write(map[i, j]);
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }
        public static void PrintNumericalGridWithConnections(Dungeon dun)
        {
            Room actualRoom, parent;
            RoomGrid grid = dun.roomGrid;
            Type type;
            int x, y, iPositive, jPositive;
            string foldername = "Assets/Resources/Levels/";
            string filename, dungeonData = "";
            bool isRoom;
            filename = "R" + Constants.nV + "-K" + Constants.nK + "-L" + Constants.nL + "-L" + Constants.lCoef;
            List<int> lockedRooms = new List<int>();
            List<int> keys = new List<int>();
            int minX, minY, maxX, maxY;
            minX = Constants.MATRIXOFFSET;
            minY = Constants.MATRIXOFFSET;
            maxX = -Constants.MATRIXOFFSET;
            maxY = -Constants.MATRIXOFFSET;
            foreach (Room room in dun.RoomList)
            {
                if (room.Type == Type.key)
                    keys.Add(room.KeyToOpen);
                else if (room.Type == Type.locked)
                    lockedRooms.Add(room.KeyToOpen);
                if (room.X < minX)
                    minX = room.X;
                if (room.Y < minY)
                    minY = room.Y;
                if (room.X > maxX)
                    maxX = room.X;
                if (room.Y > maxY)
                    maxY = room.Y;
            }

            //IEnumerable<Dictionary> orderedKeys;
            //System.Console.Write("XMin: " + minX + " Xmax: "+maxX+ " YMin: "+minY+" YMax: "+maxY+"\n");
            int sizeX = maxX - minX + 1;
            int sizeY = maxY - minY + 1;
            int[,] map = new int[2 * sizeX, 2 * sizeY];
            dungeonData += 2*sizeX + "\n";
            dungeonData += 2*sizeY + "\n";
            for (int i = 0; i < 2 * sizeX; ++i)
            {
                for (int j = 0; j < 2 * sizeY; ++j)
                {
                    map[i, j] = Util.RoomType.NOTHING;
                }
            }

            /*foreach (KeyValuePair<int, int> keys in orderKeys)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine(string.Format("Key = {0}, Value = {1}", keys.Key, keys.Value));
            }*/

            for (int i = minX; i < maxX + 1; ++i)
            {
                for (int j = minY; j < maxY + 1; ++j)
                {
                    iPositive = i - minX;
                    jPositive = j - minY;
                    actualRoom = grid[i, j];
                    if (actualRoom != null)
                    {
                        type = actualRoom.Type;
                        if (type == Type.normal)
                        {
                            if(actualRoom.IsLeafNode())
                                map[iPositive * 2, jPositive * 2] = Util.RoomType.TREASURE;
                            else
                                map[iPositive * 2, jPositive * 2] = Util.RoomType.EMPTY;
                        }
                        else if (type == Type.key)
                        {
                            map[iPositive * 2, jPositive * 2] = keys.IndexOf(actualRoom.KeyToOpen) + 1;
                        }
                        else if (type == Type.locked)
                        {
                            if (lockedRooms.IndexOf(actualRoom.KeyToOpen) == lockedRooms.Count - 1)
                                map[iPositive * 2, jPositive * 2] = Util.RoomType.BOSS;
                            else
                                map[iPositive * 2, jPositive * 2] = Util.RoomType.EMPTY;
                        }
                        else
                        {
                            Console.WriteLine("Something went wrong printing the tree!\n");
                            Console.WriteLine("This Room type does not exist!\n\n");
                        }
                        parent = actualRoom.Parent;
                        if (parent != null)
                        {

                            x = parent.X - actualRoom.X + 2 * iPositive;
                            y = parent.Y - actualRoom.Y + 2 * jPositive;
                            if (type == Type.locked)
                                map[x, y] = -(keys.IndexOf(actualRoom.KeyToOpen) + 1);
                            else
                                map[x, y] = Util.RoomType.CORRIDOR;
                        }
                    }
                    else
                    {
                        //map[iPositive * 2, jPositive * 2] = 'O';
                    }
                }
            }

            for (int i = 0; i < sizeX * 2; ++i)
            {
                for (int j = 0; j < sizeY * 2; ++j)
                {
                    isRoom = false;
                    if (map[i, j] == Util.RoomType.EMPTY)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;

                    }
                    else if (map[i, j] == Util.RoomType.CORRIDOR)
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else if (map[i, j] == 7)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if (map[i, j] == Util.RoomType.NOTHING)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (map[i, j] == Util.RoomType.BOSS)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }
                    else if (map[i, j] > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                    }
                    else if (map[i, j] < 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                    }


                    if (map[i, j] == Util.RoomType.NOTHING)
                    {
                        Console.Write("  ");
                        //writer.WriteLine(" ");
                    }
                    else
                    {
                        dungeonData += i + "\n";
                        dungeonData += j + "\n";
                        //writerRG.WriteLine(i);
                        //writerRG.WriteLine(j);
                        if (i + minX * 2 == 0 && j + minY * 2 == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.Write(" s");
                            dungeonData += "s\n";
                            //writerRG.WriteLine("s");
                            isRoom = true;
                        }
                        else if (map[i, j] == Util.RoomType.CORRIDOR)
                        {
                            Console.Write(" c");
                            dungeonData += "c\n";
                            //writerRG.WriteLine("c");
                        }
                        else if (map[i, j] == Util.RoomType.BOSS)
                        {
                            Console.Write(" B");
                            dungeonData += "B\n";
                            //writerRG.WriteLine("B");
                            isRoom = true;
                        }
                        else if (map[i, j] < 0)
                        {
                            Console.Write("{0,2}", map[i, j]);
                            dungeonData += map[i, j] + "\n";
                            //writerRG.WriteLine(map[i, j]);
                        }
                        else if (map[i, j] == Util.RoomType.TREASURE)
                        {
                            Console.Write("{0,2}", map[i, j]);
                            dungeonData += "T\n";
                            //writerRG.WriteLine("T");
                            isRoom = true;
                        }
                        else if (map[i, j] > 0)
                        {
                            Console.Write("{0,2}", map[i, j]);
                            dungeonData += map[i, j] + "\n";
                            //writerRG.WriteLine(map[i, j]);
                            isRoom = true;
                        }
                        else
                        {
                            Console.Write("{0,2}", map[i, j]);
                            dungeonData += map[i, j] + "\n";
                            //writerRG.WriteLine(map[i, j]);
                            isRoom = true;
                        }

                        /*if (isRoom)
                        {
                            if (j > 0)
                            {
                                if (map[i, j - 1] < 0 || map[i, j - 1] == Util.RoomType.CORRIDOR)
                                    writerRG.WriteLine(1);
                                else
                                    writerRG.WriteLine(0);
                            }
                            else
                                writerRG.WriteLine(0);
                            if (i < sizeX * 2 - 1)
                            {
                                if (map[i + 1, j] < 0 || map[i + 1, j] == Util.RoomType.CORRIDOR)
                                    writerRG.WriteLine(1);
                                else
                                    writerRG.WriteLine(0);
                            }
                            else
                                writerRG.WriteLine(0);
                            if (j < sizeY * 2 - 1)
                            {
                                if (map[i, j + 1] < 0 || map[i, j + 1] == Util.RoomType.CORRIDOR)
                                    writerRG.WriteLine(1);
                                else
                                    writerRG.WriteLine(0);
                            }
                            else
                                writerRG.WriteLine(0);
                            if (i > 0)
                            {
                                if (map[i - 1, j] < 0 || map[i - 1, j] == Util.RoomType.CORRIDOR)
                                    writerRG.WriteLine(1);
                                else
                                    writerRG.WriteLine(0);
                            }
                            else
                                writerRG.WriteLine(0);
                        }*/
                    }
                }
                Console.Write("\n");
                //writer.Write("\r\n");
            }
            int count = 0;
            string path;
#if UNITY_EDITOR
            path = AssetDatabase.AssetPathToGUID(foldername + filename + ".txt");
            while (path != "")
            {
                count++;
                path = AssetDatabase.AssetPathToGUID(foldername + filename + count + ".txt");
            }
#endif
            if (count > 0)
                filename += count;
            filename = foldername + filename + ".txt";
            UnityEngine.Debug.Log("Filename: " + filename);
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                UnityEngine.Debug.Log("Writing dungeon data");
                //using (StreamWriter writerRG = new StreamWriter(filenameRG, false, Encoding.UTF8))
                //{
                    writer.Write(dungeonData);
                    writer.Flush();
                    writer.Close();
                    //writerRG.Flush();
                    //writerRG.Close();
                    Console.Write("\n");
                //}
            }
            UnityEngine.Debug.Log("Finished Writing dungeon data");
        }
    }
}
