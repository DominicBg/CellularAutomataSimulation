using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatManager : MonoBehaviour
{
    static CheatManager Instance;

    [SerializeField] Button buttonPrefab;

    List<Button> buttons = new List<Button>();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        GameManager.Instance.OnGameModeEnd += CleanUp;
    }


    void CleanUp()
    {
        buttons.ForEach((btn) => Destroy(btn.gameObject));
        buttons.Clear();
    }

    public static void AddCheat(string name, Action action)
    {
        Button button = Instantiate(Instance.buttonPrefab, Instance.transform);
        button.GetComponentInChildren<Text>().text = name;
        button.onClick.AddListener(() => action.Invoke());
        Instance.buttons.Add(button);
    }
}
