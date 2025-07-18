using UnityEngine;
using UnityEngine.InputSystem;

public class TetriminoControl : MonoBehaviour
{
    private void Update()
    {
        ControlTetrimino();
    }

    private void ControlTetrimino()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position += new Vector3(-1, 0, 0);
        
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.position += new Vector3(1, 0, 0);
        
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            // rotate clockwise
            transform.Rotate(0, 0, 90);
        
        else if (Input.GetKeyDown(KeyCode.A))
            // rotate counterclockwise
            transform.Rotate(0, 0, -90);
        
        else if (Input.GetKey(KeyCode.DownArrow))
            transform.position += new Vector3(0, -1, 0);
    }
}