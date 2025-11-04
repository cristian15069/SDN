using UnityEngine;

public class MobileUIEnabler : MonoBehaviour
{
    void Awake()
    {
        if (Application.isMobilePlatform)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}