using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;


public class TaskStateController : MonoBehaviour
{
    [SerializeField] GameManager _gameManager;
    [SerializeField] private TowelStateController _towelStateController; // 追記（山田）
    [Tooltip("Attach the Slider of the handling task")]
    [SerializeField] Slider _sliderTask0;
    [SerializeField] Slider _sliderTask1;
    [SerializeField] Slider _sliderTask2;
    [SerializeField] Slider _sliderTask3;
    [Tooltip("Attach the Text of the handling task (Child of Slider)")]
    [SerializeField] TextMeshProUGUI _textTask0;
    [SerializeField] TextMeshProUGUI _textTask1;
    [SerializeField] TextMeshProUGUI _textTask2;
    [SerializeField] TextMeshProUGUI _textTask3;


    private bool isOngoing = false;
    private float elapsedTime = 0.0f;

    public GameObject TaskText;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isOngoing)
        {
            elapsedTime += Time.deltaTime;
        }


    }

    void OnTriggerEnter(Collider other)
    {
        // GameManagerのtaskStateに基づいて処理を分岐
        switch (_gameManager.taskState)
        {
            case GameManager.TaskState.Task0:
                if(other.gameObject.name == "HeadCollider" && isTowelStateSqueezed()) // 追記（山田）
                {
                    isOngoing = true;
                } 
                break;

            case GameManager.TaskState.Task1:
                if (other.gameObject.name == "RightArmCollider" && isTowelStateSqueezed())
                {
                    isOngoing = true;
                }
                break;

            case GameManager.TaskState.Task2:
                if (other.gameObject.name == "LeftArmCollider" && isTowelStateSqueezed())
                {
                    isOngoing = true;
                }
                break;

            case GameManager.TaskState.Task3:
                if (other.gameObject.name == "BodyCollider" && isTowelStateSqueezed())
                {
                    isOngoing = true;
                }
                break;

            case GameManager.TaskState.Task4:
                Debug.Log("Task4 triggered");
                // Task4の処理
                break;

            case GameManager.TaskState.Task5:
                Debug.Log("Task5 triggered");
                // Task5の処理
                break;

            default:
                Debug.LogWarning("Unknown TaskState");
                break;
        }
    }

    void OnTriggerStay(Collider other)
    {
        // GameManagerのtaskStateに基づいて処理を分岐
        switch (_gameManager.taskState)
        {
            case GameManager.TaskState.Task0:
                if (other.gameObject.name == "HeadCollider" && isTowelStateSqueezed())  //追記（山田）
                {
                    if(elapsedTime >= 5.0f)
                    {
                        _gameManager.taskState = GameManager.TaskState.Task1;
                        isOngoing = false;
                        elapsedTime = 0.0f;
                        _textTask0.text = "Task0 : " + "     " + "Completed";
                    }
                    else
                    {
                        _sliderTask0.value = Mathf.Clamp01(elapsedTime / 5.0f);
                    }
                }
                break;

            case GameManager.TaskState.Task1:
                if (other.gameObject.name == "RightArmCollider" && isTowelStateSqueezed())
                {
                    if (elapsedTime >= 5.0f)
                    {
                        _gameManager.taskState = GameManager.TaskState.Task2;
                        isOngoing = false;
                        elapsedTime = 0.0f;
                        _textTask1.text = "Task1 : " + "     " + "Completed";
                    }
                    else
                    {
                        _sliderTask1.value = Mathf.Clamp01(elapsedTime / 5.0f);
                    }
                }
                break;

            case GameManager.TaskState.Task2:
                if (other.gameObject.name == "LeftArmCollider" && isTowelStateSqueezed())
                {
                    if (elapsedTime >= 5.0f)
                    {
                        _gameManager.taskState = GameManager.TaskState.Task3;
                        isOngoing = false;
                        elapsedTime = 0.0f;
                        _textTask2.text = "Task2 : " + "     " + "Completed";
                    }
                    else
                    {
                        _sliderTask2.value = Mathf.Clamp01(elapsedTime / 5.0f);
                    }
                }
                break;

            case GameManager.TaskState.Task3:
                if (other.gameObject.name == "BodyCollider" && isTowelStateSqueezed())
                {
                    if (elapsedTime >= 5.0f)
                    {
                        _gameManager.taskState = GameManager.TaskState.Task4;
                        isOngoing = false;
                        elapsedTime = 0.0f;
                        _textTask3.text = "Task3 : " + "     " + "Completed";
                    }
                    else
                    {
                        _sliderTask3.value = Mathf.Clamp01(elapsedTime / 5.0f);
                    }
                }
                break;

            case GameManager.TaskState.Task4:
                Debug.Log("Task4 triggered");
                // Task4の処理
                break;

            case GameManager.TaskState.Task5:
                Debug.Log("Task5 triggered");
                // Task5の処理
                break;

            default:
                Debug.LogWarning("Unknown TaskState");
                break;
        }
    }

    void OnTriggerExit(Collider other)
    {
        switch (_gameManager.taskState)
        {
            case GameManager.TaskState.Task0:
                if (other.gameObject.name == "HeadCollider" && isTowelStateSqueezed()) // 追記（山田）
                {
                    isOngoing = false;
                }
                break;

            case GameManager.TaskState.Task1:
                if (other.gameObject.name == "RightArmCollider" && isTowelStateSqueezed())
                {
                    isOngoing = false;
                }
                break;

            case GameManager.TaskState.Task2:
                if (other.gameObject.name == "LeftArmCollider" && isTowelStateSqueezed())
                {
                    isOngoing = false;
                }
                break;

            case GameManager.TaskState.Task3:
                if (other.gameObject.name == "BodyCollider" && isTowelStateSqueezed())
                {
                    isOngoing = false;
                }
                break;

            case GameManager.TaskState.Task4:
                Debug.Log("Task4 triggered");
                // Task4の処理
                break;

            case GameManager.TaskState.Task5:
                Debug.Log("Task5 triggered");
                // Task5の処理
                break;

            default:
                Debug.LogWarning("Unknown TaskState");
                break;
        }
    }

    private bool isTowelStateSqueezed()
    {
        if (_towelStateController == null || _towelStateController._towelState != TowelStateController.TowelState.Squeezed )
        {
            TaskText.SetActive(true);
            return false;
        }

        TaskText.SetActive(false);
        return true;
    }
}
