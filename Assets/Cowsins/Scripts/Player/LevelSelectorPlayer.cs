using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cowsins2D
{
    public class LevelSelectorPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class LevelData
        {
            public Transform levelTransform;
            public string levelName, levelDescription;

            public UnityEvent levelEvent;

            public LevelData(Transform transform, string name, string description)
            {
                levelTransform = transform;
                levelName = name;
                levelDescription = description;
            }

            public void TriggerLevelEvent() => levelEvent?.Invoke();

        }
        [System.Serializable]
        public class LevelDataUI
        {
            public GameObject UI;
            public TextMeshProUGUI levelName, levelDescription, worldName, enterLevelKeyText;

            public LevelDataUI(GameObject ui, TextMeshProUGUI levelText, TextMeshProUGUI worldText, TextMeshProUGUI description, TextMeshProUGUI levelKeyText)
            {
                UI = ui;
                levelName = levelText;
                worldName = worldText;
                levelDescription = description;
                enterLevelKeyText = levelKeyText;
            }
        }

        [SerializeField] private Animator animator, levelAnimator;
        [SerializeField] private int unlockIndex;
        [SerializeField] private float movementSpeed;
        [SerializeField] private string worldName;
        [SerializeField] private LevelData[] levelDataArray;
        [SerializeField] private LevelDataUI levelDataUI;
        [SerializeField] private AudioClip[] footstepsSFX;
        [SerializeField] private float footstepInterval = .15f, footstepVolume;
        [SerializeField] private AudioClip reachLevelSFX, loadSceneSFX;

        private bool isMoving = false;
        private int currentIndex = 0;
        private AudioSource audioSource;

        public static PlayerActions inputActions;


        private void Start()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerActions();
                inputActions.Enable();
            }
            // Move the player to the first level transform
            transform.position = levelDataArray[0].levelTransform.position;
            // Set initial UI Settings
            levelDataUI.worldName.text = this.worldName;
            levelDataUI.levelName.text = levelDataArray[currentIndex].levelName;
            levelDataUI.levelDescription.text = levelDataArray[currentIndex].levelDescription;
            // Play initial SFX
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = reachLevelSFX;
            audioSource.Play();
            // Gather device and display it
            string device = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard ? "Keyboard" : "Controller";
            levelDataUI.enterLevelKeyText.text = "Press " + inputActions.GameControls.Jumping.GetBindingDisplayString(InputBinding.MaskByGroup(device));
        }

        private void Update()
        {
            if (!isMoving)
            {
                // Gather input
                float horizontalInput = InputManager.playerInputs.HorizontalMovement;

                // Moving to the right
                if (horizontalInput == 1 && currentIndex < levelDataArray.Length - 1 && currentIndex + 1 <= unlockIndex)
                {
                    // Calculate direction to orientate the player. This will be passed to MovePlayer 
                    float xDifference = levelDataArray[currentIndex + 1].levelTransform.position.x - transform.position.x;
                    int moveDirection = (xDifference > 0) ? 1 : -1; // Set move direction based on the sign of xDifference

                    animator.SetTrigger("move");
                    StartCoroutine(MovePlayer(levelDataArray[currentIndex + 1].levelTransform.position, moveDirection));
                    currentIndex++;
                }
                else if (horizontalInput == -1 && currentIndex > 0) // Moving to the left
                {
                    float xDifference = levelDataArray[currentIndex - 1].levelTransform.position.x - transform.position.x;
                    int moveDirection = (xDifference > 0) ? 1 : -1; // Set move direction based on the sign of xDifference
                    animator.SetTrigger("move");
                    StartCoroutine(MovePlayer(levelDataArray[currentIndex - 1].levelTransform.position, moveDirection));
                    currentIndex--;
                }
                else
                    animator.SetTrigger("idle"); // Not moving at all

                // Perform different events depending on the current levle Transform
                if (InputManager.playerInputs.JumpingDown)
                {
                    levelDataArray[currentIndex].TriggerLevelEvent();
                }
            }
            else
            {
                // If moving, play the animation and disable the UI, as we only want to show it if idle.
                animator.SetTrigger("move");
                levelDataUI.UI.SetActive(false);
                levelDataUI.levelName.text = levelDataArray[currentIndex].levelName;
                levelDataUI.levelDescription.text = levelDataArray[currentIndex].levelDescription;
            }
        }

        private IEnumerator MovePlayer(Vector3 targetPosition, int moveDirection)
        {
            isMoving = true;
            // orientate the player 
            float targetScaleX = moveDirection;
            Vector3 targetScale = new Vector3(targetScaleX, transform.localScale.y, transform.localScale.z);
            animator.transform.parent.localScale = targetScale;

            // Play footsteps when moving
            float nextFootstepTime = Time.time;
            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * movementSpeed);

                // Check if it's time to play the next footstep sound
                if (Time.time >= nextFootstepTime)
                {
                    int rand = Random.Range(0, footstepsSFX.Length);
                    audioSource.clip = footstepsSFX[rand];
                    audioSource.volume = footstepVolume;
                    audioSource.Play();
                    nextFootstepTime = Time.time + footstepInterval; // Schedule next footstep sound
                }

                yield return null;
            }
            isMoving = false;

            levelDataUI.UI.SetActive(true);
            levelAnimator.SetTrigger("reached");
            audioSource.clip = reachLevelSFX;
            audioSource.volume = 1;
            audioSource.Play();
        }

        public void LoadScene(int id)
        {
            StartCoroutine(LoadSceneGivenId(id));
        }
        private IEnumerator LoadSceneGivenId(int id)
        {
            audioSource.clip = loadSceneSFX;
            audioSource.volume = 1;
            audioSource.Play();
            yield return new WaitForSeconds(1);
            SceneManager.LoadScene(id);
        }
    }


#if UNITY_EDITOR

    [System.Serializable]
    [CustomEditor(typeof(LevelSelectorPlayer))]
    public class LevelSelectorPlayerEditor : Editor
    {

        private string[] tabs = { "Global", "LevelData", "References", "Others" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelSelectorPlayer myScript = target as LevelSelectorPlayer;

            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.SelectionGrid(currentTab, tabs, 6);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {

                    case "References":
                        EditorGUILayout.LabelField("REFERENCES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelAnimator"));
                        break;
                    case "Global":
                        EditorGUILayout.LabelField("GLOBAL SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("unlockIndex"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("worldName"));
                        break;
                    case "LevelData":
                        EditorGUILayout.LabelField("LEVEL DATA", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDataArray"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDataUI"));
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHERS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsSFX"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepInterval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepVolume"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("reachLevelSFX"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("loadSceneSFX"));
                        break;


                }
            }

            #endregion
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }


#endif
}