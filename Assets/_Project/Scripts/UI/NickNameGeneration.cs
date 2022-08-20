using UnityEngine;
using TMPro;

public class NickNameGeneration : MonoBehaviour
{
    private void Awake()
    {
        var nickNameInputField = GetComponentInChildren<TextMeshProUGUI>();
        nickNameInputField.text = PlayerData.GetRandomNickName();
    }
}
