using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoalInTruck : MonoBehaviour
{
    private List<GameObject> _coalInZone;
    private int _score = 0;

    public Text ScoreLabel;
    public GameObject FireworkPrefab;
    public AudioSource ScoreChime;

    private void Start()
    {
        _coalInZone = new List<GameObject>();
        ScoreLabel.text = "Score: " + 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Coal")
        {
            _coalInZone.Add(other.gameObject);
            Debug.Log("Added: " + other.name + ", total: " + _coalInZone.Count);
            CoalLumpBehavior cb = other.GetComponent<CoalLumpBehavior>();
            if(cb != null)
            {
                cb.Active = false;
            }
        }
    }

    public int GetScore()
    {
        return _score;
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Coal")
        {
            _coalInZone.Remove(other.gameObject);
            Debug.Log("Lost: " + other.name);
            CoalLumpBehavior cb = other.GetComponent<CoalLumpBehavior>();
            if(cb != null)
            {
                cb.RestartLifespan();
                cb.Active = true;
            }
        }
    }

    public List<GameObject> GetCoalInZone()
    {
        return _coalInZone;
    }

    public GameObject RemoveCoalObject(GameObject coal)
    {
        _coalInZone.Remove(coal);
        return coal;
    }

    public void RemoveNextCoalObject()
    {
        if (_coalInZone.Count > 0)
        {
            GameObject coal = _coalInZone[0];
            Vector3 pos = coal.transform.position;
            GameObject firework = Instantiate(FireworkPrefab);
            int scoreToAdd = (int)coal.GetComponent<CoalLumpBehavior>().Value;
            firework.transform.position = pos;
            Destroy(coal);
            _coalInZone.RemoveAt(0);
            _score += scoreToAdd;
            ScoreLabel.text = "Score: " + _score.ToString();
            ScoreChime.Play();
        }
    }

    public void RestartScore()
    {
        _score = 0;
        ScoreLabel.text = "Score: 0";
    }
}
