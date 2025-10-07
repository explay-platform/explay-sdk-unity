#if UNITY_EDITOR
using explay.GameServices;
using System.Collections.Generic;
using UnityEngine;

namespace explay.GameServices.Editor
{
    public static class explayMockServer
    {
        private static Dictionary<string, MockDataEntry> mockData = new Dictionary<string, MockDataEntry>();
        private static bool isUserSignedIn = true;
        private static int userId = 1;
        private static string username = "TestUser";
        private static string avatar = "https://placehold.co/400x400?text=TEST";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            LoadMockData();
            LoadMockUserSettings();
            Logger.log("Mock server ready");
        }

        public static void UpdateMockData(List<MockDataEntry> entries)
        {
            mockData.Clear();
            foreach (var entry in entries)
            {
                mockData[entry.key] = entry;
            }
        }

        public static void UpdateMockUser(bool signedIn, int id, string name, string avatarUrl)
        {
            isUserSignedIn = signedIn;
            userId = id;
            username = name;
            avatar = avatarUrl;
        }

        private static void LoadMockData()
        {
            string json = UnityEditor.EditorPrefs.GetString("explayMockData", "");

            if (!string.IsNullOrEmpty(json))
            {
                var container = JsonUtility.FromJson<MockDataContainer>(json);
                if (container.entries != null)
                {
                    mockData.Clear();
                    foreach (var entry in container.entries)
                    {
                        mockData[entry.key] = entry;
                    }
                }
            }
        }

        private static void LoadMockUserSettings()
        {
            isUserSignedIn = UnityEditor.EditorPrefs.GetBool("ExplayMockUserSignedIn", true);
            userId = UnityEditor.EditorPrefs.GetInt("ExplayMockUserId", 1);
            username = UnityEditor.EditorPrefs.GetString("ExplayMockUsername", "TestUser");
            avatar = UnityEditor.EditorPrefs.GetString("ExplayMockAvatar", "https://via.placeholder.com/150");
        }

        public static void SendMessage(string type, int requestId, string payload)
        {
            Logger.log($"Mock > SendMessage: {type}, RequestId: {requestId}, Payload: {payload}");

            string responseData = null;
            bool success = true;
            string error = null;

            try
            {
                switch (type)
                {
                    case "IS_USER_SIGNED_IN":
                        responseData = HandleIsUserSignedIn();
                        break;

                    case "GET_USER_DETAILS":
                        responseData = HandleGetUserDetails(out success, out error);
                        break;

                    case "GET_USER_DATA":
                        responseData = HandleGetUserData(payload, out success, out error);
                        break;

                    case "SET_USER_DATA":
                        responseData = HandleSetUserData(payload, out success, out error);
                        break;

                    case "LIST_USER_DATA":
                        responseData = HandleListUserData(out success, out error);
                        break;

                    case "DELETE_USER_DATA":
                        responseData = HandleDeleteUserData(payload, out success, out error);
                        break;

                    default:
                        success = false;
                        error = $"Unknown message type: {type}";
                        break;
                }
            }
            catch (System.Exception e)
            {
                success = false;
                error = $"Error: {e.Message}";
                Logger.error(error);
            }

            // Send response back
            SendResponse(requestId, success, responseData, error);
        }

        private static string HandleIsUserSignedIn()
        {
            var response = new SignedInResponse { signedIn = isUserSignedIn };
            return JsonUtility.ToJson(response);
        }

        private static string HandleGetUserDetails(out bool success, out string error)
        {
            success = true;
            error = null;

            if (!isUserSignedIn)
            {
                success = false;
                error = "User not signed in";
                return null;
            }

            var user = new User
            {
                id = userId,
                username = username,
                avatar = avatar
            };

            return JsonUtility.ToJson(user);
        }

        private static string HandleGetUserData(string payload, out bool success, out string error)
        {
            success = true;
            error = null;

            if (!isUserSignedIn)
            {
                success = false;
                error = "User not signed in";
                return null;
            }

            var request = JsonUtility.FromJson<PayloadGetData>(payload);

            if (mockData.TryGetValue(request.key, out MockDataEntry entry))
            {
                var data = new GameData
                {
                    key = entry.key,
                    value = entry.value,
                    isPublic = entry.isPublic
                };

                return JsonUtility.ToJson(data);
            }

            success = false;
            error = "Key not found";
            return null;
        }

        private static string HandleSetUserData(string payload, out bool success, out string error)
        {
            success = true;
            error = null;

            if (!isUserSignedIn)
            {
                success = false;
                error = "User not signed in";
                return null;
            }

            var request = JsonUtility.FromJson<PayloadSetData>(payload);

            var entry = new MockDataEntry
            {
                key = request.key,
                value = request.value,
                isPublic = request.isPublic
            };

            mockData[request.key] = entry;
            SaveMockDataToPrefs();

            var data = new GameData
            {
                key = entry.key,
                value = entry.value,
                isPublic = entry.isPublic
            };

            return JsonUtility.ToJson(data);
        }

        private static string HandleListUserData(out bool success, out string error)
        {
            success = true;
            error = null;

            if (!isUserSignedIn)
            {
                success = false;
                error = "User not signed in";
                return null;
            }

            var dataList = new List<GameData>();

            foreach (var entry in mockData.Values)
            {
                dataList.Add(new GameData
                {
                    key = entry.key,
                    value = entry.value,
                    isPublic = entry.isPublic
                });
            }

            var response = new GameDataList { data = dataList.ToArray() };
            return JsonUtility.ToJson(response);
        }

        private static string HandleDeleteUserData(string payload, out bool success, out string error)
        {
            success = true;
            error = null;

            if (!isUserSignedIn)
            {
                success = false;
                error = "User not signed in";
                return null;
            }

            var request = JsonUtility.FromJson<PayloadGetData>(payload);

            if (mockData.ContainsKey(request.key))
            {
                mockData.Remove(request.key);
                SaveMockDataToPrefs();
                return JsonUtility.ToJson(new { success = true });
            }

            success = false;
            error = "Key not found";
            return null;
        }

        private static void SaveMockDataToPrefs()
        {
            var entries = new List<MockDataEntry>(mockData.Values);
            var container = new MockDataContainer { entries = entries };
            string json = JsonUtility.ToJson(container);
            UnityEditor.EditorPrefs.SetString("ExplayMockData", json);
        }

        private static void SendResponse(int requestId, bool success, string data, string error)
        {
            var response = new SDKResponse
            {
                type = "RESPONSE",
                requestId = requestId,
                success = success,
                data = data,
                error = error
            };

            string jsonResponse = JsonUtility.ToJson(response);

            Logger.log($"Mock > Response: RequestId: {requestId}, Success: {success}, Data: {data}");

            // Send to SDK instance
            var instance = explayGameServices.Instance;
            if (instance != null)
            {
                instance.OnMessageReceived(jsonResponse);
            }
        }
    }
}
#endif