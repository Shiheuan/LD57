using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    private Player player;
    private int currentDepth;
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && player.GetDepth() != currentDepth)
        {
            text.text = currentDepth.ToString();
            currentDepth = player.GetDepth();
        }
    }
}
