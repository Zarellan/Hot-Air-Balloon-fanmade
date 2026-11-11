using UnityEngine;

public class LineAttachment : MonoBehaviour
{
    [SerializeField] GameObject attach1, attach2;
    LineRenderer lineRender;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lineRender = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRender.SetPosition(0, attach1.transform.position);
        lineRender.SetPosition(1, attach2.transform.position);

    }
}
