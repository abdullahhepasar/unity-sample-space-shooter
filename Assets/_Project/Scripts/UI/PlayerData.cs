using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private string _nickName = null;

    private void Start()
    {
        var count = FindObjectsOfType<PlayerData>().Length;
        if (count > 1)
        {
            Destroy(gameObject);
            return;
        };
        
        DontDestroyOnLoad(gameObject);
    }

    public void SetNickName(string nickName)
    {
        _nickName = nickName;
    }

    public string GetNickName()
    {
        if (string.IsNullOrWhiteSpace(_nickName))
        {
            _nickName = GetRandomNickName();
        }
        
        return _nickName;
    }

    public static string GetRandomNickName()
    {
        var PlayerNumber = Random.Range(0, 99);
        return $"Player {PlayerNumber.ToString("00")}";
    }        
}
