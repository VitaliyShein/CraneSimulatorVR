using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;
using TMPro;

public class CargoZoneTrigger : MonoBehaviour
{
    [SerializeField] private bool sExerciseActive = false; // Флаг активности упражнения
    [SerializeField] private TextMeshPro great;
    [SerializeField] 
    private List<string> targetTags = new List<string> { "BigContainer", "PipeBundle" }; // Тэг груза

    private bool isExerciseActive = false; // Флаг активности упражнения

    private void Start()
    {
        gameObject.SetActive(false); // Деактивируем триггер до начала упражнения
    }
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, активен ли триггер
        //if (!isExerciseActive) return;

        if (targetTags.Contains(other.tag))
        {
            great.text = $"Отлично, вы доставили груз {other.tag}!";
            Debug.Log($"Груз [{other.tag}] успешно доставлен на баржу.");
        }
    }

    // Метод для активации триггера
    public void StartExercise()
    {
        isExerciseActive = true;
        great.text = "Упражнение начато. Триггер активирован.";
        Debug.Log("Упражнение начато. Триггер активирован.");
    }

    // Опционально: метод для деактивации триггера
    public void StopExercise()
    {
        isExerciseActive = false;
        great.text = "Упражнение остановлено. Триггер деактивирован.";
        Debug.Log("Упражнение остановлено. Триггер деактивирован.");
    }

    public void OnTrig()
    {
        sExerciseActive = !sExerciseActive;
        gameObject.SetActive(sExerciseActive);
    }
}