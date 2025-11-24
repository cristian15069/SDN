using UnityEngine;

public class MobileUIEnabler : MonoBehaviour
{
    void Awake()
    {
        if (Application.isMobilePlatform || Application.isEditor)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}