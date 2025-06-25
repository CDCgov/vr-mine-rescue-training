using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class BH20RacePlayerDisplay : MonoBehaviour
{
    public BH20RaceData PlayerData;
    public TextMeshProUGUI PlayerStatusText;

    private StringBuilder _sbText;
    // Start is called before the first frame update
    void Start()
    {
        _sbText = new StringBuilder();
    }

    // Update is called once per frame
    void Update()
    {
        var carryLoad = PlayerData.GetCarriedLoad();
        _sbText.Clear();
        _sbText.AppendLine($"Carrying {carryLoad:F0} lb");
        _sbText.AppendLine($"Mined {PlayerData.CoalMinedLb:F0} lb");
        PlayerStatusText.text = _sbText.ToString();
    }
}
