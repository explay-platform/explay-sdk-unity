using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_EDITOR
using explay.GameServices.Editor;
#endif

namespace explay.GameServices
{
    public static class Logger
    {
        static string format(string input) => "[EXPLAY SDK] " + input;

        public static void log(string message) => Debug.Log(format(message));
        public static void warn(string message) => Debug.LogWarning(format(message));
        public static void error(string message) => Debug.LogError(format(message));
    }

    public class explayGameServices : MonoBehaviour
    {
        private static explayGameServices _instance;
        private int _requestId = 0;
        private Dictionary<int, Action<string>> _pendingRequests = new Dictionary<int, Action<string>>();

        private static bool _isCreating = false;

        public static explayGameServices Instance
        {
            get
            {
                // If static reference is null, try to find existing GameObject
                if (_instance == null)
                {
                    _instance = FindObjectOfType<explayGameServices>();

                    if (_instance != null)
                    {
                        Logger.log("Found existing explayGameServices instance via FindObjectOfType");
                    }
                }

                if (_instance == null && !_isCreating)
                {
                    Logger.log($"Creating new explayGameServices instance (_instance is null: {_instance == null}, _isCreating: {_isCreating})");
                    _isCreating = true;
                    GameObject go = new GameObject("explayGameServices");
                    _instance = go.AddComponent<explayGameServices>();
                    DontDestroyOnLoad(go);
                    _isCreating = false;
                    Logger.log("explayGameServices instance created");
                }
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Logger.warn("Duplicate explayGameServices instance detected - destroying new instance");
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Logger.log("explayGameServices Awake - Instance initialized");
            NotifyGameReady();
        }

        #region JavaScript Interop

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SendMessageToParent(string type, int requestId, string payload);

        [DllImport("__Internal")]
        private static extern void NotifyReady();
#else
        private static void SendMessageToParent(string type, int requestId, string payload) => explayMockServer.SendMessage(type, requestId, payload);

        private static void NotifyReady()
        {
            Logger.log("Mock > Game Ready");
        }
#endif

        #endregion

        #region Message Handling

        private void NotifyGameReady()
        {
#if UNITY_EDITOR
            explayMockServer.Init();
#endif

            NotifyReady();
            Logger.log("Game Ready");
        }

        /// <summary>
        /// Called from JavaScript when a response is received
        /// DO NOT CALL THIS DIRECTLY - Called from JavaScript via SendMessage
        /// </summary>
        public void OnMessageReceived(string jsonResponse)
        {
            try
            {
                var response = JsonUtility.FromJson<SDKResponse>(jsonResponse);

                if (_pendingRequests.TryGetValue(response.requestId, out Action<string> callback))
                {
                    _pendingRequests.Remove(response.requestId);

                    if (response.success)
                    {
                        callback?.Invoke(response.data);
                    }
                    else
                    {
                        Logger.error($"Request failed: {response.error}");
                        callback?.Invoke(null);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.error($"Failed to parse response: {e.Message}");
            }
        }

        private void SendRequest(string type, string payload, Action<string> callback)
        {
            int requestId = ++_requestId;
            _pendingRequests[requestId] = callback;

            SendMessageToParent(type, requestId, payload);

            // Timeout after 10 seconds
            StartCoroutine(RequestTimeout(requestId));
        }

        private IEnumerator RequestTimeout(int requestId)
        {
            yield return new WaitForSeconds(10f);

            if (_pendingRequests.ContainsKey(requestId))
            {
                Logger.error($"Request {requestId} timed out");
                _pendingRequests.Remove(requestId);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Check if user is signed in
        /// </summary>
        public void IsUserSignedIn(Action<bool> callback)
        {
            SendRequest("IS_USER_SIGNED_IN", "{}", (response) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var result = JsonUtility.FromJson<SignedInResponse>(response);
                    callback?.Invoke(result.signedIn);
                }
                else
                {
                    callback?.Invoke(false);
                }
            });
        }

        /// <summary>
        /// Get the current authenticated user details
        /// </summary>
        public void GetUserDetails(Action<User> callback)
        {
            SendRequest("GET_USER_DETAILS", "{}", (response) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var user = JsonUtility.FromJson<User>(response);
                    callback?.Invoke(user);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Get a stored value by key
        /// </summary>
        public void GetData(string key, Action<GameData> callback)
        {
            var payload = JsonUtility.ToJson(new PayloadGetData { key = key });
            SendRequest("GET_USER_DATA", payload, (response) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var data = JsonUtility.FromJson<GameData>(response);
                    callback?.Invoke(data);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Set a value for a key
        /// </summary>
        public void SetData(string key, string value, bool isPublic = false, Action<GameData> callback = null)
        {
            var payload = JsonUtility.ToJson(new PayloadSetData
            {
                key = key,
                value = value,
                isPublic = isPublic
            });

            SendRequest("SET_USER_DATA", payload, (response) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var data = JsonUtility.FromJson<GameData>(response);
                    callback?.Invoke(data);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// List all stored data for the current user
        /// </summary>
        public void ListData(Action<GameDataList> callback)
        {
            SendRequest("LIST_USER_DATA", "{}", (response) =>
            {
                if (!string.IsNullOrEmpty(response))
                {
                    var dataList = JsonUtility.FromJson<GameDataList>(response);
                    callback?.Invoke(dataList);
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Delete a stored value by key
        /// </summary>
        public void DeleteData(string key, Action<bool> callback = null)
        {
            var payload = JsonUtility.ToJson(new PayloadGetData { key = key });
            SendRequest("DELETE_USER_DATA", payload, (response) =>
            {
                callback?.Invoke(!string.IsNullOrEmpty(response));
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Save game progress as JSON
        /// </summary>
        public void SaveProgress<T>(T progressData, bool isPublic = false, Action<GameData> callback = null)
        {
            string json = JsonUtility.ToJson(progressData);
            SetData("progress", json, isPublic, callback);
        }

        /// <summary>
        /// Load game progress
        /// </summary>
        public void LoadProgress<T>(Action<T> callback) where T : class
        {
            GetData("progress", (data) =>
            {
                if (data != null && !string.IsNullOrEmpty(data.value))
                {
                    try
                    {
                        T progressData = JsonUtility.FromJson<T>(data.value);
                        callback?.Invoke(progressData);
                    }
                    catch (Exception e)
                    {
                        Logger.error($"Failed to parse progress data: {e.Message}");
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    callback?.Invoke(null);
                }
            });
        }

        /// <summary>
        /// Save high score
        /// </summary>
        public void SaveHighScore(int score, bool isPublic = true, Action<GameData> callback = null)
        {
            SetData("highScore", score.ToString(), isPublic, callback);
        }

        /// <summary>
        /// Get high score
        /// </summary>
        public void GetHighScore(Action<int> callback)
        {
            GetData("highScore", (data) =>
            {
                if (data != null && !string.IsNullOrEmpty(data.value))
                {
                    if (int.TryParse(data.value, out int score))
                    {
                        callback?.Invoke(score);
                    }
                    else
                    {
                        callback?.Invoke(0);
                    }
                }
                else
                {
                    callback?.Invoke(0);
                }
            });
        }

        #endregion
    }

    #region Data Models

    [Serializable]
    public class User
    {
        public int id;
        public string username;
        public string avatar;
    }

    [Serializable]
    public class GameData
    {
        public string key;
        public string value;
        public bool isPublic;
    }

    [Serializable]
    public class GameDataList
    {
        public GameData[] data;
    }

    [Serializable]
    internal class SDKResponse
    {
        public string type;
        public int requestId;
        public bool success;
        public string data;
        public string error;
    }

    [Serializable]
    internal class PayloadGetData
    {
        public string key;
    }

    [Serializable]
    internal class PayloadSetData
    {
        public string key;
        public string value;
        public bool isPublic;
    }

    [Serializable]
    internal class SignedInResponse
    {
        public bool signedIn;
    }

    #endregion
}
