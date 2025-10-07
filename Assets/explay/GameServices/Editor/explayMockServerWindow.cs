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
        private bool mockUserSignedIn = true;
        private int mockUserId = 1;
        private string mockUsername = "TestUser";
        private string mockAvatar = "https://via.placeholder.com/150";

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
            string json = EditorPrefs.GetString("explayMockData", "");

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
            EditorPrefs.SetString("explayMockData", json);

            // Also save to explayMockServer
            explayMockServer.UpdateMockData(dataEntries);
        }

        private void LoadMockUserSettings()
        {
            mockUserSignedIn = EditorPrefs.GetBool("explayMockUserSignedIn", true);
            mockUserId = EditorPrefs.GetInt("explayMockUserId", 1);
            mockUsername = EditorPrefs.GetString("explayMockUsername", "TestUser");
            mockAvatar = EditorPrefs.GetString("explayMockAvatar", "https://via.placeholder.com/150");

            explayMockServer.UpdateMockUser(mockUserSignedIn, mockUserId, mockUsername, mockAvatar);
        }

        private void SaveMockUserSettings()
        {
            EditorPrefs.SetBool("explayMockUserSignedIn", mockUserSignedIn);
            EditorPrefs.SetInt("explayMockUserId", mockUserId);
            EditorPrefs.SetString("explayMockUsername", mockUsername);
            EditorPrefs.SetString("explayMockAvatar", mockAvatar);

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