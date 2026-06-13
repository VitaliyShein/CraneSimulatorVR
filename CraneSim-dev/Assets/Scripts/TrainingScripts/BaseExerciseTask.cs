using UnityEngine;

public abstract class BaseExerciseTask : MonoBehaviour
{
    [Header("Общие настройки")]
    public string taskName = "Новая задача";
    public string taskDescription = "Описание задачи";
    public bool isOptional = false;      // опциональная задача (не обязательна для выполнения)
    public float timeLimit = 0f;         // ограничение по времени (0 = без ограничений)
    
    protected bool isCompleted = false;
    protected bool isActive = false;
    protected float startTime;
    
    // События
    public System.Action<BaseExerciseTask> OnTaskCompleted;
    public System.Action<BaseExerciseTask, string> OnTaskError;
    
    // Свойства
    public bool IsCompleted => isCompleted;
    public float ElapsedTime => isActive ? Time.time - startTime : 0f;
    
    // Активация задачи
    public virtual void Activate()
    {
        isActive = true;
        isCompleted = false;
        startTime = Time.time;
        Debug.Log($"[Задача] Активирована: {taskName}");
        OnActivate();
    }
    
    // Деактивация задачи
    public virtual void Deactivate()
    {
        isActive = false;
        OnDeactivate();
    }
    
    // Сброс задачи (для перезапуска)
    public virtual void ResetTask()
    {
        isCompleted = false;
        isActive = false;
        OnReset();
    }
    
    // Проверка выполнения (вызывается каждый кадр)
    public virtual void CheckCompletion()
    {
        if (!isActive || isCompleted) return;
        
        // Проверка времени
        if (timeLimit > 0 && ElapsedTime > timeLimit)
        {
            TriggerError("Время выполнения превышено!");
            return;
        }
        
        // Вызываем специфичную проверку
        CheckSpecificCondition();
    }
    
    // Методы для переопределения в наследниках
    protected abstract void CheckSpecificCondition();
    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
    protected abstract void OnReset();
    
    // Завершение задачи
    protected void CompleteTask()
    {
        if (!isActive) return;
        
        isCompleted = true;
        isActive = false;
        Debug.Log($"[Задача] ВЫПОЛНЕНА: {taskName} (время: {ElapsedTime:F1} сек)");
        OnTaskCompleted?.Invoke(this);
    }
    
    // Ошибка в задаче
    protected void TriggerError(string errorMessage)
    {
        isActive = false;
        Debug.LogWarning($"[Задача] ОШИБКА: {taskName} - {errorMessage}");
        OnTaskError?.Invoke(this, errorMessage);
    }
}