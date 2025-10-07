using UnityEngine;
using UnityEngine.UI;
using explay.GameServices;
using System;
using TMPro;

public class explaySDKDemo : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private TextMeshProUGUI displayText;

    [Header("Buttons")]
    [SerializeField] private Button getUserInfoButton;
    [SerializeField] private Button getUserDataButton;
    [SerializeField] private Button setDataButton;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField keyInputField;
    [SerializeField] private TMP_InputField valueInputField;
    [SerializeField] private Toggle isPublicToggle;

    private void Start()
    {
        if (displayText != null)
            displayText.text = "Welcome to the explay SDK demo!\n\nClick a button to test the SDK";

        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (getUserInfoButton != null)
            getUserInfoButton.onClick.AddListener(OnGetUserInfoClicked);

        if (getUserDataButton != null)
            getUserDataButton.onClick.AddListener(OnGetUserDataClicked);

        if (setDataButton != null)
            setDataButton.onClick.AddListener(OnSetDataClicked);
    }

    private void OnGetUserInfoClicked()
    {
        Debug.Log("[Demo] Getting user info...");

        if (displayText != null)
            displayText.text = "Loading user info...";

        explayGameServices.Instance.GetUserDetails((user) =>
        {
            if (user != null)
            {
                string info = $"<b>User Info</b>\n\n" +
                             $"ID: {user.id}\n" +
                             $"Username: {user.username}\n" +
                             $"Avatar: {(string.IsNullOrEmpty(user.avatar) ? "None" : user.avatar)}";

                if (displayText != null)
                    displayText.text = info;

                Debug.Log($"[Demo] User: {user.username} (ID: {user.id})");
            }
            else
            {
                if (displayText != null)
                    displayText.text = "<color=red>Not logged in</color>";

                Debug.Log("[Demo] No user logged in");
            }
        });
    }

    private void OnGetUserDataClicked()
    {
        Debug.Log("[Demo] Getting all user data...");

        if (displayText != null)
            displayText.text = "Loading user data...";

        explayGameServices.Instance.ListData((dataList) =>
        {
            if (dataList != null && dataList.data != null && dataList.data.Length > 0)
            {
                string info = $"<b>User Data ({dataList.data.Length} items)</b>\n\n";

                foreach (var item in dataList.data)
                {
                    info += $"<b>{item.key}</b>: {item.value}\n";
                    info += $"Public: {item.isPublic}\n\n";
                }

                if (displayText != null)
                    displayText.text = info;

                Debug.Log($"[Demo] Found {dataList.data.Length} data items");
            }
            else
            {
                if (displayText != null)
                    displayText.text = "<color=yellow>No data stored</color>";

                Debug.Log("[Demo] No data stored");
            }
        });
    }

    private void OnSetDataClicked()
    {
        string key = keyInputField != null ? keyInputField.text : "";
        string value = valueInputField != null ? valueInputField.text : "";
        bool isPublic = isPublicToggle != null ? isPublicToggle.isOn : false;

        if (string.IsNullOrWhiteSpace(key))
        {
            if (displayText != null)
                displayText.text = "<color=red>Error: Key cannot be empty</color>";
            Debug.LogError("[Demo] Key cannot be empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            if (displayText != null)
                displayText.text = "<color=red>Error: Value cannot be empty</color>";
            Debug.LogError("[Demo] Value cannot be empty");
            return;
        }

        Debug.Log($"[Demo] Setting data: {key} = {value} (public: {isPublic})");

        if (displayText != null)
            displayText.text = $"Saving data...\nKey: {key}";

        explayGameServices.Instance.SetData(key, value, isPublic, (data) =>
        {
            if (data != null)
            {
                string info = $"<b>Data Saved!</b>\n\n" +
                             $"Key: {data.key}\n" +
                             $"Value: {data.value}\n" +
                             $"Public: {data.isPublic}";

                if (displayText != null)
                    displayText.text = info;

                Debug.Log($"[Demo] Data saved: {data.key} = {data.value}");

                if (keyInputField != null)
                    keyInputField.text = "";
                if (valueInputField != null)
                    valueInputField.text = "";
                if (isPublicToggle != null)
                    isPublicToggle.isOn = false;
            }
            else
            {
                if (displayText != null)
                    displayText.text = "<color=red>Failed to save data</color>";

                Debug.LogError("[Demo] Failed to save data");
            }
        });
    }
}
