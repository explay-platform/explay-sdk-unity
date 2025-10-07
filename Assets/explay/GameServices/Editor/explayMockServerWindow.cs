#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using explay.GameServices;
using System.Collections.Generic;

namespace explay.GameServices.Editor
{
    public class explayMockServerWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private string newKey = "";
        private string newValue = "";
        private bool newIsPublic = false;

        // Mock User Settings
        private bool mockUserSignedIn = false;
        private int mockUserId = 0;
        private string mockUsername = "";
        private string mockAvatar = "";

        private List<MockDataEntry> dataEntries = new List<MockDataEntry>();

        [MenuItem("explay/Mock Server Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<explayMockServerWindow>("explay Mock Server");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnEnable()
        {
            LoadMockData();
            LoadMockUserSettings();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawUserSettings();
            EditorGUILayout.Space(10);
            DrawDataManagement();
            EditorGUILayout.Space(10);
            DrawActions();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("explay Mock Server Manager", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Configure mock data for testing the explay SDK in the Unity Editor. " +
                "This data will be used when running in Play Mode.",
                MessageType.Info
            );
            EditorGUILayout.Space(5);
        }

        private void DrawUserSettings()
        {
            EditorGUILayout.LabelField("Mock User Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            mockUserSignedIn = EditorGUILayout.Toggle("User Signed In", mockUserSignedIn);

            EditorGUI.BeginDisabledGroup(!mockUserSignedIn);
            mockUserId = EditorGUILayout.IntField("User ID", mockUserId);
            mockUsername = EditorGUILayout.TextField("Username", mockUsername);
            mockAvatar = EditorGUILayout.TextField("Avatar URL", mockAvatar);
            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
            {
                SaveMockUserSettings();
            }
        }

        private void DrawDataManagement()
        {
            EditorGUILayout.LabelField("Mock Game Data", EditorStyles.boldLabel);

            // Add new data section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Add New Data", EditorStyles.miniBoldLabel);

            newKey = EditorGUILayout.TextField("Key", newKey);
            newValue = EditorGUILayout.TextField("Value", newValue);
            newIsPublic = EditorGUILayout.Toggle("Is Public", newIsPublic);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Data", GUILayout.Width(100)))
            {
                AddMockData();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // Display existing data
            EditorGUILayout.LabelField($"Stored Data ({dataEntries.Count} items)", EditorStyles.miniBoldLabel);

            if (dataEntries.Count == 0)
            {
                EditorGUILayout.HelpBox("No mock data stored. Add some data above to test the SDK.", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < dataEntries.Count; i++)
                {
                    DrawDataEntry(i);
                }
            }
        }

        private void DrawDataEntry(int index)
        {
            var entry = dataEntries[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Key: {entry.key}", EditorStyles.boldLabel, GUILayout.Width(200));

            if (entry.isPublic)
            {
                EditorGUILayout.LabelField("Public", EditorStyles.miniLabel);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                dataEntries.RemoveAt(index);
                SaveMockData();
                return;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            entry.value = EditorGUILayout.TextField("Value", entry.value);
            entry.isPublic = EditorGUILayout.Toggle("Is Public", entry.isPublic);

            if (EditorGUI.EndChangeCheck())
            {
                SaveMockData();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reload Data"))
            {
                LoadMockData();
                LoadMockUserSettings();
            }

            if (GUILayout.Button("Clear All Data"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear All Mock Data",
                    "Are you sure you want to clear all mock data?",
                    "Yes",
                    "No"))
                {
                    dataEntries.Clear();
                    SaveMockData();
                }
            }

            if (GUILayout.Button("Add Sample Data"))
            {
                AddSampleData();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddMockData()
        {
            if (string.IsNullOrWhiteSpace(newKey))
            {
                EditorUtility.DisplayDialog("Invalid Key", "Key cannot be empty.", "OK");
                return;
            }

            // Check for duplicate keys
            if (dataEntries.Exists(e => e.key == newKey))
            {
                EditorUtility.DisplayDialog("Duplicate Key", $"Key '{newKey}' already exists.", "OK");
                return;
            }

            dataEntries.Add(new MockDataEntry
            {
                key = newKey,
                value = newValue,
                isPublic = newIsPublic
            });

            SaveMockData();

            // Clear inputs
            newKey = "";
            newValue = "";
            newIsPublic = false;
        }

        private void AddSampleData()
        {
            var samples = new List<MockDataEntry>
            {
                new MockDataEntry { key = "level", value = "5", isPublic = true },
                new MockDataEntry { key = "highScore", value = "9999", isPublic = true },
                new MockDataEntry { key = "jsonData", value = "{\"level\":5,\"experience\":1250,\"coins\":500}", isPublic = false }
            };

            foreach (var sample in samples)
            {
                if (!dataEntries.Exists(e => e.key == sample.key))
                {
                    dataEntries.Add(sample);
                }
            }

            SaveMockData();
        }

        private void LoadMockData()
        {
            string json = PlayerPrefs.GetString(explayMockServer.MOCK_DATA_KEY, "");

            LoadMockUserSettings();

            if (!string.IsNullOrEmpty(json))
            {
                var container = JsonUtility.FromJson<MockDataContainer>(json);
                dataEntries = container.entries ?? new List<MockDataEntry>();
            }
            else
            {
                dataEntries = new List<MockDataEntry>();
            }
        }

        private void SaveMockData()
        {
            var container = new MockDataContainer { entries = dataEntries };
            string json = JsonUtility.ToJson(container);
            PlayerPrefs.SetString(explayMockServer.MOCK_DATA_KEY, json);

            // Also save to explayMockServer
            explayMockServer.UpdateMockData(dataEntries);
        }

        private void LoadMockUserSettings()
        {
            mockUserSignedIn = PlayerPrefs.GetInt(explayMockServer.MOCK_LOGGED_IN_KEY, 1) == 1;
            mockUserId = PlayerPrefs.GetInt(explayMockServer.MOCK_USER_ID_KEY, 1);
            mockUsername = PlayerPrefs.GetString(explayMockServer.MOCK_USERNAME_KEY, "TestUser");
            mockAvatar = PlayerPrefs.GetString(explayMockServer.MOCK_AVATAR_KEY, "https://placehold.co/400x400?text=TEST");

            explayMockServer.UpdateMockUser(mockUserSignedIn, mockUserId, mockUsername, mockAvatar);
        }

        private void SaveMockUserSettings()
        {
            PlayerPrefs.SetInt(explayMockServer.MOCK_LOGGED_IN_KEY, mockUserSignedIn ? 1 : 0);
            PlayerPrefs.SetInt(explayMockServer.MOCK_USER_ID_KEY, mockUserId);
            PlayerPrefs.SetString(explayMockServer.MOCK_USERNAME_KEY, mockUsername);
            PlayerPrefs.SetString(explayMockServer.MOCK_AVATAR_KEY, mockAvatar);

            explayMockServer.UpdateMockUser(mockUserSignedIn, mockUserId, mockUsername, mockAvatar);
        }
    }

    [System.Serializable]
    public class MockDataEntry
    {
        public string key;
        public string value;
        public bool isPublic;
    }

    [System.Serializable]
    public class MockDataContainer
    {
        public List<MockDataEntry> entries;
    }
}
#endif