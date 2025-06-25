using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class ScenarioMenuController : MonoBehaviour
{
    public TextMeshProUGUI MenuTypeLabel;
    public GameObject IpInput;
    public GameObject IpInputLabel;
    public GameObject IpLabelDM;
    public GameObject PortLabel;
    public GameObject PortInput;
    public GameObject[] UpdateRates;
    public GameObject ModeLabel;
    public GameObject ModeCombobox;
    public TextMeshProUGUI JoinGameButtonLabel;
    public GameObject SpectateButton;
    public SceneConfiguration SceneConfigurations;
    public TextMeshProUGUI PrimaryMenuLabel;

    private void Start()
    {
        SetIPLabel();
    }

    public void SetMenuTypeLabel(string label)
    {
        MenuTypeLabel.text = label;
    }

    public void SetIPLabel()
    {
        TextMeshProUGUI labelText = IpLabelDM.GetComponent<TextMeshProUGUI>();
        //string pubIp =  new System.Net.WebClient().DownloadString("https://api.ipify.org");
        //labelText.text = pubIp;
        labelText.text = $"Local IP: {Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString()}";

    }

    void SetMode()
    {
        //ScenarioMenuController scenarioMenuController = RootMenus[6].GetComponent<ScenarioMenuController>();
        PrimaryMenuLabel.text = "Multiplayer Launch";
        ModeLabel.SetActive(true);
        ModeCombobox.SetActive(true);
        if (SceneConfigurations.IsDM)
        {
            SetMenuTypeLabel("Host Session");
            IpInput.SetActive(true);
            IpInputLabel.SetActive(true);
            IpLabelDM.SetActive(false);
            SpectateButton.SetActive(false);
            ModeLabel.SetActive(false);
            ModeCombobox.SetActive(false);
            foreach(GameObject go in UpdateRates)
            {
                go.SetActive(true);
            }
            PortLabel.SetActive(true);
            PortInput.SetActive(true);
            JoinGameButtonLabel.text = "Host VR Mine";
            //SetIPLabel();
        }
        else if (SceneConfigurations.IsSinglePlayer)
        {
            SetMenuTypeLabel("");
            IpInput.SetActive(false);
            IpInputLabel.SetActive(false);
            IpLabelDM.SetActive(false);
            PortInput.SetActive(false);
            PortLabel.SetActive(false);
            foreach(GameObject go1 in UpdateRates)
            {
                go1.SetActive(false);
            }
            SpectateButton.SetActive(false);
            JoinGameButtonLabel.text = "Start";
            PrimaryMenuLabel.text = "Single Player Launch";
        }
        else
        {
            SetMenuTypeLabel("Join Session");
            IpInput.SetActive(true);
            IpInputLabel.SetActive(true);
            IpLabelDM.SetActive(false);
            SpectateButton.SetActive(true);
            PortInput.SetActive(true);
            PortLabel.SetActive(true);
            foreach (GameObject go1 in UpdateRates)
            {
                go1.SetActive(false);
            }
            JoinGameButtonLabel.text = "Participate";
        }
    }

    private void OnEnable()
    {
        SetMode();
    }
}
