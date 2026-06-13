// public class HoistExercise : MonoBehaviour
// {
//     public float targetHeight = 5f;
//     public float completionTolerance = 0.2f;
//     public GameObject cargoObject;
//     public TrainingLogger logger;
//     public VRInstructionUI ui;
    
//     private float startCargoHeight;
//     private bool isExerciseActive = false;
    
//     public void StartExercise()
//     {
//         startCargoHeight = cargoObject.transform.position.y;
//         isExerciseActive = true;
//         logger.LogEvent("EXERCISE_STARTED", "Type", "Hoist");
//         ui.UpdateInstruction("Поднимите груз на высоту 5 метров. Используйте левый рычаг (подъём).", 1, 1);
//     }
    
//     private void Update()
//     {
//         if (!isExerciseActive) return;
        
//         float currentHeight = cargoObject.transform.position.y;
//         float targetReached = startCargoHeight + targetHeight;
        
//         if (Mathf.Abs(currentHeight - targetReached) <= completionTolerance)
//         {
//             CompleteExercise();
//         }
//     }
    
//     private void CompleteExercise()
//     {
//         isExerciseActive = false;
//         logger.LogExerciseCompleted("Hoist Exercise");
//         ui.ShowCompletionMessage();
        
//         // Отключаем управление подъёмом
//         FindObjectOfType<EngineVRAdapter>().enabled = false;
//     }
// }

using UnityEngine;
using System.Collections.Generic;
public class CargoZoneTrigger: MonoBehaviour
{
    [SerializeField]
    private List<string> targetTags = new List<string> {"BigContainer", "PipeBundle"}; // Тэг груза

    private void OnTriggerEnter(Collider other) {
        if (targetTags.Contains(other.tag))
        {
            Debug.Log($"Груз [{other.tag}] успешно доставлен на баржу.");
        }
    }
}

