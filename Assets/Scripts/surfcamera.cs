using UnityEngine;
using UnityEngine.InputSystem;


public class surfcamera : MonoBehaviour
{
bool GameStarted;
public SurfPlayerscr surfPlayerscr;
    void Start()
    {
        
    }
    void Update()
    {
        if (surfPlayerscr.GameEnded)
        {
            return;
        }
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) {
        GameStarted = true;
        }
        if (GameStarted) {
                transform.position += new Vector3(0, 0.001f, 0);
        }
    }
}
