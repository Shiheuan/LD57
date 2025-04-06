using System.Collections;
using System.Collections.Generic;
using MCommon.Unity.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ShowInfo : MonoBehaviour
{
    private Player player;
    private int currentDepth;
    public GameObject ScoreRoot;
    public TMP_Text scoreText;
    public GameObject AmmoRoot;
    public TMP_Text ammoText;
    
    public GameObject tipPanel;
    public GameObject tipPanelBG;
    public TMP_Text TipText;
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
            scoreText.text = currentDepth.ToString();
            currentDepth = player.GetDepth();
        }
    }

    public void ShowTips(string tips, bool bg = true)
    {
        tipPanel.SetGameObjectActive(true);
        TipText.text = tips;
        tipPanelBG.SetGameObjectActive(bg);
    }

    public void HideTips()
    {
        tipPanel.SetGameObjectActive(false);
    }
}
