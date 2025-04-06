using MCommon.Unity.Utils;
using TMPro;
using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    private Player player;
    private int currentDepth;
    private int ammoCount;
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
            currentDepth = player.GetDepth();
            scoreText.text = (currentDepth - GameManager.Instance.DepthBaseline).ToString();
        }

        if (player != null && player.GetLeftJumpCount() != ammoCount)
        {
            ammoCount = player.GetLeftJumpCount();
            ammoText.text = ammoCount.ToString();
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
