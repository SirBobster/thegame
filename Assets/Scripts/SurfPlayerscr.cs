using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SurfPlayerscr : MonoBehaviour
{
public float speedforward = 0.1f;
public float speedleft = -0.1f;
public float speedright = 0.1f;
bool SwitchDirection;
bool GameStarted;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            GameStarted = true;
            SwitchDirection = true;
        }
        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            GameStarted = true;
            SwitchDirection = false;
        }
        if (GameStarted)
        {
            if (!SwitchDirection)
            {
                transform.position += new Vector3(speedleft, speedforward, 0f);
            }
            else
            {
                transform.position += new Vector3(speedright, speedforward, 0f);
            }
        }
    }
}
