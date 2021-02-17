using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
{
    static CheatManager Instance;

    [SerializeField] Button buttonPrefab;

    Dictionary<string, Button> cheats = new Dictionary<string, Button>();

    private void Awake()
    {
        Instance = this;
        GameManager.Instance.OnGameModeEnd += CleanUp;
    }


    void CleanUp()
    {
        foreach(var button in cheats.Values)
        { 
           Destroy(button.gameObject);
        }
        cheats.Clear();
    }

    public static void AddCheat(string name, Action action)
    {
        Button button;
        if (Instance.cheats.TryGetValue(name, out button))
        {
            button.onClick.AddListener(() => action.Invoke());
        }
        else
        {
            button = Instantiate(Instance.buttonPrefab, Instance.transform);
            button.GetComponentInChildren<Text>().text = name;
            button.onClick.AddListener(() => action.Invoke());
            Instance.cheats.Add(name, button);
        }
    }
}
