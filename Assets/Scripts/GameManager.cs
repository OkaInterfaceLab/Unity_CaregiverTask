using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public enum TaskState
    {
        Task0, Task1, Task2, Task3, Task4, Task5,
    }

    public TaskState taskState;
    private TaskState lastTaskState;

    public TextMeshProUGUI _text;


    // Start is called before the first frame update
    void Start()
    {
        taskState = TaskState.Task0;
        lastTaskState = taskState;
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        if (taskState != lastTaskState)
        {
            UpdateText();
            lastTaskState = taskState;
        }
    }

    private void UpdateText()
    {
        _text.text = "Work in progress : " + taskState.ToString();
    }
}
