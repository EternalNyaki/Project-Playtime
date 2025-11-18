using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIManager : MonoBehaviour
{
    public GameObject roomButtonPrefab;

    public PartyManager partyManager;

    public void AddButton(ushort port)
    {
        Button button = Instantiate(roomButtonPrefab, transform).GetComponent<Button>();
        button.onClick.AddListener(() => partyManager.JoinPort(port));
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        buttonText.text = port.ToString();
    }
}
