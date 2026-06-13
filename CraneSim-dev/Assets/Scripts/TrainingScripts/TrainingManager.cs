using UnityEngine;
using System.Collections.Generic;

public class TrainingManager : MonoBehaviour
{
    public enum ExerciseState { NotStarted, InProgress, Completed, Failed }
    public ExerciseState currentState = ExerciseState.NotStarted;
    
    public List<BaseExerciseTask> tasks; // список задач в упражнении
    public int currentTaskIndex = 0;
    
    // private void Update()
    // {
    //     if (currentState == ExerciseState.InProgress)
    //     {
    //         CheckCurrentTask();
    //     }
    // }
    
    // private void CheckCurrentTask()
    // {
    //     if (tasks[currentTaskIndex].IsCompleted())
    //     {
    //         CompleteCurrentTask();
    //     }
    // }
    
    // private void CompleteCurrentTask()
    // {
    //     tasks[currentTaskIndex].SetCompleted();
    //     currentTaskIndex++;
        
    //     if (currentTaskIndex >= tasks.Count)
    //     {
    //         CompleteExercise();
    //     }
    // }
    
    // private void CompleteExercise()
    // {
    //     currentState = ExerciseState.Completed;
    //     ShowCompletionUI();
    //     TrainingLogger.LogExerciseCompleted(GetComponent<VRInstructionUI>().exerciseName);
    // }
    
    // public void StartExercise()
    // {
    //     currentState = ExerciseState.InProgress;
    //     tasks[currentTaskIndex].Activate();
    // }
}