using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCameraManager : MonoBehaviour {

    [SerializeField]
    private float wiggleMultiplier = 0.25f;
    [SerializeField]
    private float wiggleMagnitude = 4f;

    private float xD;
    private float yD;
    private float yOffset;

    private Vector3 startPos;

    // Use this for initialization
    void Start () {
        xD = Random.Range(0f, 0.5f);
        yD = Random.Range(0f, 0.3f);
        yOffset = Random.Range(2f, 15f);

        startPos = transform.position;
    }
    
    // Update is called once per frame
    void Update () {
        float x = Mathf.PerlinNoise(Time.time * wiggleMultiplier, xD);
        float y = Mathf.PerlinNoise(Time.time * wiggleMultiplier + yOffset, yD);
        transform.position = new Vector3(x * wiggleMagnitude + startPos.x, y * wiggleMagnitude + startPos.y, -10);
    }


}
