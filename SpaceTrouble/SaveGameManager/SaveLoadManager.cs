using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using SpaceTrouble.GameObjects;
using SpaceTrouble.GameObjects.Creatures.friendly;
using SpaceTrouble.World;
using static System.String;

namespace SpaceTrouble.SaveGameManager
{
    /// <summary>
    ///  works with the Newtonsoft JSON library for serializing and deserializing data.
    /// Also provides methods to save and load primitive data to files via a dictionary. These two concepts should not be mixed up.
    /// I.e. you can't use a file that uses Newtonsoft to save serialized data and the SaveSetting() method at the same time.
    /// To enforce this we use 2 Dictionaries to separate the corresponding files.
    /// </summary>

    internal enum DictionarySavingFiles {
        GameSettings,
        Statistics
    }

    internal static class SaveLoadManager
    {
        // Used for primitve data dictionaries
        private static Dictionary<string, object> sSettingsDictionary = new Dictionary<string, object>();

        private static readonly Dictionary<DictionarySavingFiles, string> sDictionaryFiles =
            new Dictionary<DictionarySavingFiles, string>
            {
                {DictionarySavingFiles.GameSettings, "settings.json"},
                {DictionarySavingFiles.Statistics, "statistics.json"}
            };


        // Used for serialized data
        private enum SerializationSavingFiles
        {
            GameState,
            GameData,
            GameDataDebug
        }

        private static readonly Dictionary<SerializationSavingFiles, string> sSerializationFiles =
            new Dictionary<SerializationSavingFiles, string>
            {
                {SerializationSavingFiles.GameState, "gameState.json"},
                {SerializationSavingFiles.GameData, "gameData.json"},
                {SerializationSavingFiles.GameDataDebug, "gameDataDebug.json"}
            };


        public static void SaveSetting(string key, int value,
            DictionarySavingFiles filename = DictionarySavingFiles.GameSettings)
        {
            LoadSettingsFromFile(filename);
            sSettingsDictionary[key] = value;
            WriteSettingsToFile(filename);
        }

        public static void SaveSetting(string key, double value,
            DictionarySavingFiles filename = DictionarySavingFiles.GameSettings)
        {
            LoadSettingsFromFile(filename);
            sSettingsDictionary[key] = value;
            WriteSettingsToFile(filename);
        }

        public static int LoadSettingAsInt(string key, int alt = 0,
            DictionarySavingFiles filename = DictionarySavingFiles.GameSettings)
        {
            LoadSettingsFromFile(filename);
            if (sSettingsDictionary.ContainsKey(key))
            {
                return (int) (long) sSettingsDictionary[key];
            }

            return alt;
        }

        public static double LoadSettingAsDouble(string key, double alt = 0,
            DictionarySavingFiles filename = DictionarySavingFiles.GameSettings)
        {
            LoadSettingsFromFile(filename);
            if (sSettingsDictionary.ContainsKey(key))
            {
                return (double) sSettingsDictionary[key];
            }

            return alt;
        }

        public static void LoadSettingsFromFile(DictionarySavingFiles file = DictionarySavingFiles.GameSettings)
        {
            var filename = sDictionaryFiles[file];

            if (!File.Exists(filename))
            {
                sSettingsDictionary = new Dictionary<string, object>();
                WriteSettingsToFile(file);
                return;
            }

            var sr = new StreamReader(filename);
            var line = sr.ReadLine();

            while (line != null)
            {
                if (line.EndsWith("SettingsDictionary"))
                {
                    sSettingsDictionary =
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(sr.ReadLine() ?? Empty);
                }

                line = sr.ReadLine();
            }


            sr.Close();
        }

        private static void WriteSettingsToFile(DictionarySavingFiles file = DictionarySavingFiles.GameSettings)
        {
            var filename = sDictionaryFiles[file];

            using var sw = new StreamWriter(filename);
            using JsonWriter writer = new JsonTextWriter(sw);

            sw.WriteLine("SettingsDictionary");
            sw.Write(JsonConvert.SerializeObject(sSettingsDictionary));

            writer.Close();
            sw.Close();
        }

        public static bool SavedGameExists()
        {
            return File.Exists(sSerializationFiles[SerializationSavingFiles.GameData]);
        }

        public static void SaveGameState(GameTime gameTime)
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            using var sw = new StreamWriter(sSerializationFiles[SerializationSavingFiles.GameState]);
            using JsonWriter writer = new JsonTextWriter(sw);

            sw.WriteLine(WorldGameState.DifficultyManager.GetType());
            serializer.Serialize(writer, WorldGameState.DifficultyManager);
            sw.WriteLine();

            sw.WriteLine(WorldGameState.GameMaster.GetType());
            serializer.Serialize(writer, WorldGameState.GameMaster);
            sw.WriteLine();

            sw.WriteLine(WorldGameState.TaskManager.GetType());
            serializer.Serialize(writer, WorldGameState.TaskManager);
            sw.WriteLine();

            sw.WriteLine(WorldGameState.PriorityManager.GetType());
            serializer.Serialize(writer, WorldGameState.PriorityManager);
            sw.WriteLine();

            sw.WriteLine(gameTime.GetType());
            serializer.Serialize(writer, gameTime);
            sw.WriteLine();

            writer.Close();
            sw.Close();
        }

        public static void LoadGameState(GameTime gameTime)
        {
            var sr = new StreamReader(sSerializationFiles[SerializationSavingFiles.GameState]);
            var line = sr.ReadLine();

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };

            while (line != null)
            {
                object deserializeObject;
                if (line.EndsWith("DifficultyManager"))
                {
                    deserializeObject =
                        JsonConvert.DeserializeObject<DifficultyManager>(sr.ReadLine() ?? Empty, settings);
                    WorldGameState.DifficultyManager = (DifficultyManager) deserializeObject;
                }
                else if (line.EndsWith("GameMaster"))
                {
                    deserializeObject =
                        JsonConvert.DeserializeObject<GameMaster>(sr.ReadLine() ?? Empty, settings);
                    WorldGameState.GameMaster = (GameMaster) deserializeObject;
                }
                else if (line.EndsWith("TaskManager"))
                {
                    deserializeObject =
                        JsonConvert.DeserializeObject<TaskManager>(sr.ReadLine() ?? Empty, settings);
                    WorldGameState.TaskManager = (TaskManager) deserializeObject;
                }
                else if (line.EndsWith("PriorityManager")) {
                    deserializeObject =
                        JsonConvert.DeserializeObject<PriorityManager>(sr.ReadLine() ?? Empty, settings);
                    WorldGameState.PriorityManager = (PriorityManager)deserializeObject;
                } else if (line.EndsWith("GameTime"))
                {
                    deserializeObject =
                        JsonConvert.DeserializeObject<GameTime>(sr.ReadLine() ?? Empty, settings);
                    var loadedGameTime = (GameTime) deserializeObject;
                    if (loadedGameTime != null)
                    {
                        gameTime.TotalGameTime = new TimeSpan(loadedGameTime.TotalGameTime.Ticks);
                    }
                }

                line = sr.ReadLine();
            }

            sr.Close();
        }

        public static void SaveGameObjects()
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            using var sw = new StreamWriter(sSerializationFiles[SerializationSavingFiles.GameData]);
            using JsonWriter writer = new JsonTextWriter(sw);

            var allGameObjects = WorldGameState.ObjectManager.GetAllObjects();

            serializer.Serialize(writer, allGameObjects);

            writer.Close();
            sw.Close();

            SaveGameObjectsDebug();
        }

        private static void SaveGameObjectsDebug()
        {
            var serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };

            using var sw = new StreamWriter(sSerializationFiles[SerializationSavingFiles.GameDataDebug]);
            using JsonWriter writer = new JsonTextWriter(sw);
            
            var allGameObjects = WorldGameState.ObjectManager.GetAllObjects();
            
            serializer.Serialize(writer, allGameObjects);

            writer.Close();
            sw.Close();
        }

        public static void LoadGameObjects()
        {
            var sr = new StreamReader(sSerializationFiles[SerializationSavingFiles.GameData]);
            var objectManager = WorldGameState.ObjectManager;

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };

            var allGameObjects =
                JsonConvert.DeserializeObject<List<GameObject>>(sr.ReadLine() ?? Empty, settings);

            if (allGameObjects != null)
            {
                foreach (var gameObj in allGameObjects)
                {
                    if (gameObj is Minion minion)
                    {
                        // fix to load TargetDestinations Stack in correct order
                        var cSharpIsStupidList = minion.TargetDestinations.ToArray();
                        var tempStack = new Stack<Vector2>(cSharpIsStupidList);
                        minion.TargetDestinations = tempStack;
                    }
                    objectManager.LoadObject(gameObj);
                }
            }

            sr.Close();
        }
    }
}