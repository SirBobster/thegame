using UnityEngine;
using UnityEngine.InputSystem;


public class surfcamera : MonoBehaviour
{
bool GameStarted;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) {
        GameStarted = true;
        }
        if (GameStarted) {
                transform.position += new Vector3(0, 0.001f, 0);
        }
    }
}
