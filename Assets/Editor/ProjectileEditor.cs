using UnityEditor;
using UnityEngine;
using Skills;
using Projectiles;

[CustomEditor(typeof(SO_Projectile))]
public class ProjectileEditor : Editor
{
    #region Fields
    private bool _showBehaviourFoldout;
    private Editor projectileDataEditor;

    private SerializedProperty _destructionCondProp;
    private SerializedProperty _lifeTimeProp;
    private SerializedProperty _movementDataProp;
    private SerializedProperty _rotateProp;
    private SerializedProperty _speedProp;
    private SerializedProperty _movementTypeProp;
    private SerializedProperty _trajectoryCurveProp;
    private SerializedProperty _trajectoryLengthProp;
    private SerializedProperty _viewDataProp;
    private SerializedProperty _impactProp;

    private Object lastData;
    #endregion
    #region Drawing Methods
    private void DrawMovementData()
    {
        DrawPropertyField(_movementTypeProp, "Movement Type");
        MovementType movementType = (MovementType)_movementTypeProp.enumValueIndex;

        switch (movementType)
        {
            case MovementType.None: break;
            case MovementType.TargetedLinear:
                DrawSpeedAndRotateProps();
                break;
            case MovementType.TargetedCurve:
                DrawSpeedAndRotateProps();
                DrawPropertyField(_trajectoryCurveProp, "Movement Curve");
                break;
            case MovementType.DistanceLinear:
                DrawSpeedAndRotateProps();
                DrawPropertyField(_trajectoryLengthProp, "Trajectory Length");
                break;
            default: break;
        }
    }
    private void DrawSpeedAndRotateProps()
    {
        DrawPropertyField(_rotateProp, "Rotate");
        DrawPropertyField(_speedProp, "Speed");
    }
    private void DrawPropertyField(SerializedProperty property, string label) =>
        EditorGUILayout.PropertyField(property, new GUIContent(label));
    #endregion
    #region Helpers / Utils
    private void CheckDataUpdate()
    {
        if (_viewDataProp.objectReferenceValue != lastData)
        {
            lastData = _viewDataProp.objectReferenceValue;
            if (lastData == null) return;

            if (projectileDataEditor == null)
                projectileDataEditor = CreateEditor(lastData);

            projectileDataEditor.OnInspectorGUI();
            Repaint();
        }
        else if (projectileDataEditor != null)
            projectileDataEditor.OnInspectorGUI();
    }
    #endregion
    #region Unity Methods
    private void OnEnable()
    {
        _viewDataProp = serializedObject.FindProperty("viewData");
        _destructionCondProp = serializedObject.FindProperty("destructionCondition");
        _lifeTimeProp = serializedObject.FindProperty("lifeTime");
        _impactProp = serializedObject.FindProperty("impact");
        _movementDataProp = serializedObject.FindProperty("movementData");

        _rotateProp = _movementDataProp.FindPropertyRelative("_rotate");
        _speedProp = _movementDataProp.FindPropertyRelative("_speed");
        _movementTypeProp = _movementDataProp.FindPropertyRelative("_movementType");
        _trajectoryCurveProp = _movementDataProp.FindPropertyRelative("_trajectoryCurve");
        _trajectoryLengthProp = _movementDataProp.FindPropertyRelative("_trajectoryLength");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Projectile Data", EditorStyles.boldLabel);

        DrawPropertyField(_viewDataProp, "View Data");
        CheckDataUpdate();

        EditorGUILayout.Space();
        _showBehaviourFoldout = EditorGUILayout.Foldout(_showBehaviourFoldout, "Behaviour");

        if (_showBehaviourFoldout)
        {
            DrawPropertyField(_destructionCondProp, "Destruction Condition");

            if ((DestructionCondition)_destructionCondProp.enumValueIndex == DestructionCondition.OnTime)
                DrawPropertyField(_lifeTimeProp, "Life Time");

            if (_movementDataProp != null)
                DrawMovementData();
            else
                EditorGUILayout.HelpBox("Movement Data not initialized. Check SO_Projectile.", MessageType.Warning);
        }

        DrawPropertyField(_impactProp, "Impact");

        serializedObject.ApplyModifiedProperties();
    }
    #endregion
}
