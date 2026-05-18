using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PointConfigurator))]
public class PointConfiguratorEditor : Editor 
{
    PointConfigurator thisPointConfigurator;
    

    //private SerializedProperty points;
    
    void Update()
    {
        //OnInspectorGUI();
    }
    
    void OnEnable()
    {
        EditorApplication.update += Update;
        thisPointConfigurator = target as PointConfigurator;
        //points = serializedObject.FindProperty("points");
    }
    
    void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("postfix"));
        if (thisPointConfigurator.target != null)
        {
            string[] setCommands = thisPointConfigurator.getSetCommands(thisPointConfigurator.target, thisPointConfigurator.postfix);
            //thisPointConfigurator.updateComandsArguments();
            GUILayout.Label("Доступные команды");
            GUILayout.Label("    Index        Команда");
            if (setCommands.Length == 0)
                GUILayout.Label("Не найдено команд!");
            for (int i = 0; i < setCommands.Length; i++)
            {
                string[] splited = setCommands[i].Split(' ');
                string[] splited2 = splited[1].Split('(');
                GUILayout.Label("    " + i + "              " + splited2[0]);
            }
                
        }
        
        GUILayout.Label(" ");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("drowGizmos"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("testTarget"));
        GUILayout.Label("   ");
        GUILayout.Label("Настройки");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("points"), true);
        
        /*if (GUILayout.Button("Отрисовать Gizmos - "+ thisPointConfigurator.drowGizmos))
        {
            thisPointConfigurator.drowGizmosUpdate();
        }
        if (GUILayout.Button("Отрисовывать сферы - "+ thisPointConfigurator.renderOject))
        {
            thisPointConfigurator.renderObjectUpdate();
        }*/

        if (serializedObject.ApplyModifiedProperties())
        {
            thisPointConfigurator.updatePointsData();
            thisPointConfigurator.updatePoints();
        }
    }
}
