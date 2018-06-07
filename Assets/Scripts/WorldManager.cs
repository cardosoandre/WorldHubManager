//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Types of world point "looks".
/// <para>"sphereLook" rotates the world sphere on the X and Y axis.</para>
/// <para>"cameraLook" rotates the camera pivot on the X and Y axis.</para>
/// <para>"sphereAndCameraLook" rotates the world sphere on the X axis and the camera on the Y axis</para>
/// </summary>
public enum Rotations { sphereLook, cameraLook, sphereAndCameraLook }

/// <summary>
/// <para>This script should be attached to an empty GameObject that has another empty GameObject as a child, let´s call this new child "WorldParent".</para>
/// <para>Inside the WorldParent, we create the actual Sphere and call it something like "WorldSphere".</para>
/// </summary>
public class WorldManager : MonoBehaviour {

    [Header("List of World Points")]
    /// The list of world points, all changes can be made in the inspector!
    /// Check the WorldPoint Class at the end of the code to see the clas variables.
    public List<WorldPoint> worldPointsList = new List<WorldPoint>();
    [Space]
    [Header("Parameters")]
    public Rotations rotationType;
    public float lookVelocity = 10;
    [Space]
    [Header("Public References")]
    /// Reference to the Sphere's Parent Transform.
    public Transform worldParent;
    /// Reference to the 3D World Sphere Transform.
    public Transform worldSphere;
    /// Reference to the Camera's Parent Transform.
    public Transform cameraParent;
    [Header("Prefabs")]
    /// Reference to the World Point Prefab. The prefab should be an empty GameObject that has a child that is moved on the Z-axis.
    /// The child has to be moved to the edge of the 3D sphere. In Unity's default sphere, that's 0.5 on the Z-axis.
    public GameObject worldPointPrefab;
    [Space]
    [Header("UI Stuff")]
    public Transform _ButtonUiHolder;

    /// Private variables
    private Transform target;
    private Vector3 rotation = Vector3.zero;

    void Start () {

        /// If World Points List is empty, create a few examples
        if (worldPointsList.Count == 0)
        {
            WorldPoint a = new WorldPoint();
            a.name = "Example 1"; a.color = Color.red; a.x = 0; a.y = 20;
            WorldPoint b = new WorldPoint();
            b.name = "Example 2"; b.color = Color.green; b.x = 50; b.y = -20;
            WorldPoint c = new WorldPoint();
            c.name = "Example 3"; c.color = Color.blue; c.x = -20; c.y = -10;
            worldPointsList.Add(a); worldPointsList.Add(b); worldPointsList.Add(c);
        }

        /// Loop through the list of world points
        for (int i = 0; i < worldPointsList.Count; i++)
        {
            /// Create the world point for the according element in the loop.
            CreateWorldPoint(worldPointsList[i]);
            /// Create the debug buttons for the according element in the loop.
            CreateDebugWorldPointButton(worldPointsList[i]);
        }

        /// Look at the first world point (If the list count is bigger than 0).
        if (worldPointsList.Count > 0)
        {
            LookAtWorldPoint(worldPointsList[0]);
        }

    }

    /// <summary>
    /// Function that creates the visual representation of a World Point class.
    /// </summary>
    /// <param name="wp">Reference to the World Point to be created.</param>
    void CreateWorldPoint(WorldPoint wp)
    {
        /// Instantiate the World Point point prefab as a child of the world sphere (As new GameObject)
        GameObject pivot = Instantiate(worldPointPrefab, worldSphere.transform);
        /// Use the World Point coordinates to rotate the New GameObject in euler angles.
        pivot.transform.eulerAngles = new Vector3(wp.y, -wp.x, 0);
        /// Name the New GameObject 
        pivot.name = wp.name;
        /// Set the target transform of the reference World Point.
        wp.SetTarget(pivot.transform.GetChild(0));
        /// Change the color of the new Game Object's renderer.
        pivot.GetComponentInChildren<Renderer>().material.color = wp.color;
    }

	void Update () {

        /// Use the keyboard numbers to look at the world points.
        for (int i = 0; i < worldPointsList.Count; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                LookAtWorldPoint(worldPointsList[i]);
            }
        }

        /// Lerp the angle of the world sphere on the Y (euler) axis using the X coordinates of the currently selected World Point. 
        Vector3 worldAngle = new Vector3(0, Mathf.LerpAngle(worldSphere.localEulerAngles.y, rotation.x, lookVelocity * Time.deltaTime), 0);
        /// Lerp the angle of the world sphere PARENT on the X (euler) axis using the Y coordinates of the currently selected World Point. 
        Vector3 worldParentAngle = new Vector3(Mathf.LerpAngle(worldParent.localEulerAngles.x, -rotation.y, lookVelocity * Time.deltaTime), 0, 0);

        ///Camera Lerps
        Vector3 cameraAngle = new Vector3(Mathf.LerpAngle(cameraParent.localEulerAngles.x, rotation.y, lookVelocity * Time.deltaTime), Mathf.LerpAngle(cameraParent.localEulerAngles.y, -rotation.x, lookVelocity * Time.deltaTime), 0);
        Vector3 cameraAxisAngle = new Vector3(Mathf.LerpAngle(cameraParent.localEulerAngles.x, rotation.y, lookVelocity * Time.deltaTime), 0, 0);

        if (rotationType == Rotations.sphereLook)
        {
            worldSphere.localEulerAngles = worldAngle;
            worldParent.localEulerAngles = worldParentAngle;
        }

        if (rotationType == Rotations.cameraLook)
        {
            cameraParent.localEulerAngles = cameraAngle;
        }

        if (rotationType == Rotations.sphereAndCameraLook)
        {
            worldSphere.localEulerAngles = worldAngle;
            cameraParent.localEulerAngles = cameraAxisAngle;
        }

    }

    /// <summary>
    /// Function that changes the current focused World Point.
    /// </summary>
    /// <param name="wp"></param>
    void LookAtWorldPoint(WorldPoint wp)
    {
        rotation = new Vector3(wp.x, wp.y);
        target = wp.target;
    }

    /// Draw Gizmos to facilitate the World Points visualisation.
    void OnDrawGizmos()
    {
    #if UNITY_EDITOR
        /// Only draw the gizmos is there is at least one world point.
        if (worldPointsList.Count > 0)
        {
            /// Loop through all the world points.
            for (int i = 0; i < worldPointsList.Count; i++)
            {
                /// Set gizmos color to be equal the correspondent world point.
                Gizmos.color = worldPointsList[i].color;
                /// Just in case the color is "empty", set it to Red.
                if (Gizmos.color == Color.clear) Gizmos.color = Color.red;
                /// Create two empty gameobjects to simulate the world point prefab's position / rotation.
                GameObject gizmoPoint = new GameObject();
                GameObject gizmoParent = new GameObject();
                /// Move the gizmoPoint to the edge of the sphere (0.5f on the Z axis)
                gizmoPoint.transform.position += -Vector3.forward/2;
                /// Parent the gizmoPoint to the "gizmoParent" object in the center.
                gizmoPoint.transform.parent = gizmoParent.transform;
                /// Rotate the gizmoParent object based on the world point cooridates.
                gizmoParent.transform.eulerAngles += new Vector3(worldPointsList[i].y, -worldPointsList[i].x, 0);
                /// Keep track of the world point position in play mode in order for the gizmos to follow.
                if (Application.isPlaying)
                {
                    gizmoParent.transform.position += worldSphere.transform.position;
                    gizmoParent.transform.rotation = worldPointsList[i].target.parent.rotation;
                }
                /// Draw a gizmo sphere in the world point position.
                Gizmos.DrawSphere(gizmoPoint.transform.position, 0.07f);
                /// Destroy all the objects created.
                DestroyImmediate(gizmoPoint);
                DestroyImmediate(gizmoParent);
            }
        }
    #endif
    }

    /// ------------------------------------------------ DEBUG STUFF ------------------------------------------------------ ///

    void CreateDebugWorldPointButton(WorldPoint wp)
    {
        GameObject button = new GameObject();
        GameObject text = new GameObject();
        button.transform.SetParent(_ButtonUiHolder);
        button.transform.localScale = Vector3.one;
        button.transform.localPosition = Vector3.zero;
        button.AddComponent<Image>();
        button.AddComponent<Outline>();
        Button b = button.AddComponent<Button>();
        b.onClick.AddListener(ButtonClick);
        text.transform.SetParent(button.transform);
        text.transform.localScale = Vector3.one;
        text.transform.localPosition = Vector3.zero;
        Text txt = text.AddComponent<Text>();
        Font ArialFont = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        txt.font = ArialFont;
        txt.material = ArialFont.material;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = (wp.name == string.Empty) ? "No Name " + button.transform.GetSiblingIndex() : wp.name;
    }

    public void ButtonClick()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        int index = selected.transform.GetSiblingIndex();
        LookAtWorldPoint(worldPointsList[index]);
    }

}

/// <summary>
/// Class that contains all the information of a world point
/// </summary>
[System.Serializable]
public class WorldPoint
{
    /// World Point's Name.
    public string name;
    /// World Point's Description.
    public string description;
    /// World Point's Coordinates (in 360 degrees for both X and Y axis).
    [Range(-360, 360)]
    public float x, y;
    /// World Point's Renderer Color
    public Color color;
    /// World Point's 3D Transform Target.
    public Transform target;

    /// <summary>
    /// Function to set the World Point's 3D Transform Target.
    /// </summary>
    /// <param name="t">3D Transform Target Reference.</param>
    public void SetTarget(Transform t)
    {
        target = t;
    }
}
