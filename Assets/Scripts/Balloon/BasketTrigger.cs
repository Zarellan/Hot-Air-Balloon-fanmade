using UnityEngine;

public class BasketTrigger : MonoBehaviour
{
    public bool touched = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        touched = true;
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        touched = false;
    }
    
}
