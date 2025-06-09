#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(PlayerMovement))]
    public class PlayerMovementEditor : Editor
    {
        private string[] tabs = { "Assignables", "Movement", "Jump", "Crouch", "Advanced Movement", "Stamina", "Assist", "Others", "Events" };
        private int currentTab = 0;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            PlayerMovement myScript = target as PlayerMovement;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/playerMovement_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);


            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.SelectionGrid(currentTab, tabs, 6);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Assignables":
                        EditorGUILayout.LabelField("ASSIGNABLES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("graphics"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("whatIsGround"));
                        break;
                    case "Movement":
                        EditorGUILayout.LabelField("MOVEMENT SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoRun"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("runSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalLadderSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("verticalLadderSpeed"));
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("GRAVITY", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScale"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fallGravityMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFallSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxUpwardsSpeed"));
                        //EditorGUI.indentLevel++;
                        break;
                    case "Jump":
                        EditorGUILayout.LabelField("JUMP", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMethod"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("amountOfJumps"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("apexReachSharpness"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangGravityMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangTimeThreshold"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangAccelerationMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangMaxSpeedMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fastFallMultiplier"));
                        break;
                    case "Crouch":
                        EditorGUILayout.LabelField("CROUCHING", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowCrouch"));
                        if (myScript.allowCrouch)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("canCrouchSlideMidAir"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSlideSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSlideDuration"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Advanced Movement":
                        EditorGUILayout.LabelField("ADVANCED MOVEMENT", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("WALL JUMP", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowWallJump"));
                        if (myScript.allowWallJump)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpRunLerp"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpTime"));

                            EditorGUI.indentLevel--;
                        }

                        EditorGUILayout.LabelField("WALL SLIDE", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSlide"));
                        if (myScript.allowSlide)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlidingResetsJumps"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideVFXInterval"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideVFX"));
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.LabelField("DASH", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashMethod"));
                        if (myScript.dashMethod != DashMethod.None)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("amountOfDashes"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashGravityScale"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("dashInterval"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("invincibleWhileDashing"));
                            EditorGUI.indentLevel--;
                        }


                        EditorGUILayout.LabelField("GLIDE", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("canGlide"));
                        if (myScript.canGlide)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("glideSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("glideGravity"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("canDashWhileGliding"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("glideDurationMethod"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumGlideTime"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("handleOrientationWhileGliding"));
                            EditorGUI.indentLevel--;
                        }

                        break;
                    case "Stamina":
                        EditorGUILayout.LabelField("STAMINA", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("usesStamina"));
                        if (myScript.usesStamina)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minStaminaRequiredToRun"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStamina"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRegenMultiplier"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnJump"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnWallJump"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnCrouchSlide"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Assist":
                        EditorGUILayout.LabelField("ASSISTANCE OPTIONS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpInputBufferTime"));
                        break;
                    case "Events":
                        EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHER SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepHeight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerOrientationMethod"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landOnEnemyDamage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landOnEnemyImpulse"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("step"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsInterval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsVolume"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landVolume"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landCameraShake"));
                        EditorGUILayout.LabelField("CHECK SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingCheckSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("leftWallCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rightWallCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallCheckSize"));
                        break;

                }
            }

            #endregion
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif