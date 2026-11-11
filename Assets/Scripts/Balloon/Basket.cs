using UnityEngine;

public class Basket : MonoBehaviour
{
    [SerializeField] GameObject ballon;

    public bool touched;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        touched = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        touched = false;
    }
}
