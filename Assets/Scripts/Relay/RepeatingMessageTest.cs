using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatingMessageTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating(nameof(TestMessage), 1.0f, 2.0f);
    }

    void TestMessage()
    {
        float fps = (float)Time.frameCount / Time.time;
        Debug.Log($"Test Message {Time.time,-10:F1} {Time.frameCount,-10} {fps,-10:F1}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Key Pressed");
        }
    }
}
