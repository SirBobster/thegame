using UnityEngine;
using UnityEngine.InputSystem;

public class SurfPlayerscr : MonoBehaviour
{
public float speedforward = 0.1f;
public float speedleft = -0.1f;
public float speedright = 0.1f;
bool SwitchDirection;
bool GameStarted;
public bool GameEnded;
    void Start()
    {
        
    }
    void Update()
    {
        if (GameEnded)
        {
            return;
        }
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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameEnded = true;
    }
}
