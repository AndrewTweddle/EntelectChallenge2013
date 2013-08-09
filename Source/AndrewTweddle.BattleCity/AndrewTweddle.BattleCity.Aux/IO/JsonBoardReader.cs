using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using AndrewTweddle.BattleCity.Core.Collections;
using System.IO;

namespace AndrewTweddle.BattleCity.Aux.IO
{
    public static class JsonBoardReader
    {
        public static BitMatrix LoadBoardFromJsonFile(string jsonFilePath)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(string[][]));
            using (FileStream fileStream = new FileStream(jsonFilePath, FileMode.Open))
            {
                string[][] cellStatusStrings = (string[][]) serializer.ReadObject(fileStream);

                short width = (short) cellStatusStrings.Length;
                short height = (short) cellStatusStrings[0].Length;

                BitMatrix board = new BitMatrix(width, height);

                for (short y = 0; y < height; y++)
                {
                    for (short x = 0; x < width; x++)
                    {
                        string statusTypeDesc = cellStatusStrings[x][y];
                        if (statusTypeDesc == "FULL")
                        {
                            board[x, y] = true;
                        }
                    }
                }

                return board;
            }
        }
    }
}
