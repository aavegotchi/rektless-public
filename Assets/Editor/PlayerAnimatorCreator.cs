using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Editor
{
    public class PlayerAnimatorCreator : MonoBehaviour
    {
        [MenuItem("MyMenu/Create Controller")]
        static void CreateController()
        {
            // Creates the controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/Anims/player/player.controller");

            // Add parameters
            controller.AddParameter("crouch", AnimatorControllerParameterType.Bool);
            controller.AddParameter("jump", AnimatorControllerParameterType.Bool);
            controller.AddParameter("hit", AnimatorControllerParameterType.Bool);
            controller.AddParameter("attack", AnimatorControllerParameterType.Bool);
            controller.AddParameter("range_attack", AnimatorControllerParameterType.Bool);
            controller.AddParameter("velocity_y_direction", AnimatorControllerParameterType.Float);
            controller.AddParameter("velocity", AnimatorControllerParameterType.Float);
            controller.AddParameter("death", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("is_sliding", AnimatorControllerParameterType.Bool);

            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;

            // Add States
            var idleState = rootStateMachine.AddState("idle");
            var walkState = rootStateMachine.AddState("walk");
            var crouchIdleState = rootStateMachine.AddState("crouch_idle");
            var crouchWalkState = rootStateMachine.AddState("crouch_walk");
            var jumpUpState = rootStateMachine.AddState("jump_up");
            var jumpDownState = rootStateMachine.AddState("jump_down");
            var hitState = rootStateMachine.AddState("hit");
            var crouchHitState = rootStateMachine.AddState("crouch_hit");
            var attackState = rootStateMachine.AddState("attack");
            var crouchAttackState = rootStateMachine.AddState("crouch_attack");
            var rangeAttackState = rootStateMachine.AddState("range_attack");
            var deathState = rootStateMachine.AddState("death");
            var slideState = rootStateMachine.AddState("slide");

            // Set default state
            rootStateMachine.defaultState = idleState;

            // Add Transitions
            // Idle transitions
            AddTransition(idleState, walkState, "velocity", AnimatorConditionMode.Greater, 0.1f);
            AddTransition(idleState, crouchIdleState, "crouch", AnimatorConditionMode.If, true);
            AddTransition(idleState, jumpUpState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Greater, 0);
            AddTransition(idleState, jumpDownState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Less, 0);
            AddTransition(idleState, hitState, "hit", AnimatorConditionMode.If, true, "crouch",
                AnimatorConditionMode.IfNot,
                true, "jump", AnimatorConditionMode.IfNot, true, "attack", AnimatorConditionMode.IfNot, true);
            AddTransition(idleState, attackState, "attack", AnimatorConditionMode.If, true);
            AddTransition(idleState, rangeAttackState, "range_attack", AnimatorConditionMode.If, true);

            // Walk transitions
            AddTransition(walkState, idleState, "velocity", AnimatorConditionMode.Less, 0.1f, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(walkState, crouchWalkState, "crouch", AnimatorConditionMode.If, true, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(walkState, jumpUpState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Greater, 0, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(walkState, jumpDownState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Less, 0, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(walkState, attackState, "attack", AnimatorConditionMode.If, true);
            AddTransition(walkState, rangeAttackState, "range_attack", AnimatorConditionMode.If, true);
            AddTransition(walkState, slideState, "is_sliding", AnimatorConditionMode.If, true);

            // Crouch idle transitions
            AddTransition(crouchIdleState, idleState, "crouch", AnimatorConditionMode.IfNot, true);
            AddTransition(crouchIdleState, crouchWalkState, "velocity", AnimatorConditionMode.Greater, 0.1f);
            AddTransition(crouchIdleState, crouchHitState, "hit", AnimatorConditionMode.If, true);
            AddTransition(crouchIdleState, crouchAttackState, "attack", AnimatorConditionMode.If, true);

            // Crouch walk transitions
            AddTransition(crouchWalkState, crouchIdleState, "velocity", AnimatorConditionMode.Less, 0.1f, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(crouchWalkState, walkState, "crouch", AnimatorConditionMode.IfNot, true, "is_sliding",
                AnimatorConditionMode.IfNot, true);
            AddTransition(crouchWalkState, crouchHitState, "hit", AnimatorConditionMode.If, true);
            AddTransition(crouchWalkState, crouchAttackState, "attack", AnimatorConditionMode.If, true);
            AddTransition(crouchWalkState, slideState, "is_sliding", AnimatorConditionMode.If, true);

            // Crouch hit transitions
            AddTransition(crouchHitState, crouchIdleState, "hit", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(crouchHitState, crouchWalkState, "hit", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);
            AddTransition(crouchHitState, idleState, "hit", AnimatorConditionMode.IfNot, true, "crouch",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Less, 0.1f);
            AddTransition(crouchHitState, walkState, "hit", AnimatorConditionMode.IfNot, true, "crouch",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Greater, 0.1f);

            // Jump up transitions
            AddTransition(jumpUpState, idleState, "jump", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(jumpUpState, walkState, "jump", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);
            AddTransition(jumpUpState, jumpDownState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Less, 0);
            AddTransition(jumpUpState, hitState, "hit", AnimatorConditionMode.If, true);
            AddTransition(jumpUpState, attackState, "attack", AnimatorConditionMode.If, true);

            // Jump down transitions
            AddTransition(jumpDownState, idleState, "jump", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(jumpDownState, walkState, "jump", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);
            AddTransition(jumpDownState, jumpUpState, "jump", AnimatorConditionMode.If, true, "velocity_y_direction",
                AnimatorConditionMode.Greater, 0);
            AddTransition(jumpDownState, hitState, "hit", AnimatorConditionMode.If, true);
            AddTransition(jumpDownState, attackState, "attack", AnimatorConditionMode.If, true);

            // Hit transitions
            AddTransition(hitState, attackState, "hit", AnimatorConditionMode.IfNot, true, "attack",
                AnimatorConditionMode.If, true);
            AddTransition(hitState, jumpDownState, "hit", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.If, true, "attack", AnimatorConditionMode.IfNot, true, "velocity_y_direction",
                AnimatorConditionMode.Less, 0);
            AddTransition(hitState, jumpUpState, "hit", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.If,
                true, "attack", AnimatorConditionMode.IfNot, true, "velocity_y_direction",
                AnimatorConditionMode.Greater,
                0);
            AddTransition(hitState, idleState, "hit", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.IfNot, true, "attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(hitState, walkState, "hit", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.IfNot, true, "attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);

            // Attack transitions
            AddTransition(attackState, jumpDownState, "attack", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.If, true, "velocity_y_direction", AnimatorConditionMode.Less, 0);
            AddTransition(attackState, jumpUpState, "attack", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.If, true, "velocity_y_direction", AnimatorConditionMode.Greater, 0);
            AddTransition(attackState, idleState, "attack", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Less, 0.1f);
            AddTransition(attackState, walkState, "attack", AnimatorConditionMode.IfNot, true, "jump",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Greater, 0.1f);

            // Crouch attack transitions
            AddTransition(crouchAttackState, crouchIdleState, "attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(crouchAttackState, crouchWalkState, "attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);
            AddTransition(crouchAttackState, idleState, "attack", AnimatorConditionMode.IfNot, true, "crouch",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Less, 0.1f);
            AddTransition(crouchAttackState, walkState, "attack", AnimatorConditionMode.IfNot, true, "crouch",
                AnimatorConditionMode.IfNot, true, "velocity", AnimatorConditionMode.Greater, 0.1f);

            // Range attack transitions
            AddTransition(rangeAttackState, idleState, "range_attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(rangeAttackState, walkState, "range_attack", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);

            // Slide transitions
            AddTransition(slideState, idleState, "is_sliding", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Less, 0.1f);
            AddTransition(slideState, walkState, "is_sliding", AnimatorConditionMode.IfNot, true, "velocity",
                AnimatorConditionMode.Greater, 0.1f);

            // Death transitions (from all states to death)
            AddTransitionToDeath(idleState, deathState);
            AddTransitionToDeath(walkState, deathState);
            AddTransitionToDeath(crouchIdleState, deathState);
            AddTransitionToDeath(crouchWalkState, deathState);
            AddTransitionToDeath(jumpUpState, deathState);
            AddTransitionToDeath(jumpDownState, deathState);
            AddTransitionToDeath(hitState, deathState);
            AddTransitionToDeath(crouchHitState, deathState);
            AddTransitionToDeath(attackState, deathState);
            AddTransitionToDeath(crouchAttackState, deathState);
            AddTransitionToDeath(rangeAttackState, deathState);
        }

        static void AddTransition(AnimatorState fromState, AnimatorState toState, params object[] conditions)
        {
            var transition = fromState.AddTransition(toState);
            transition.hasExitTime = false;
            transition.duration = 0;

            for (int i = 0; i < conditions.Length; i += 3)
            {
                string paramName = (string)conditions[i];
                AnimatorConditionMode mode = (AnimatorConditionMode)conditions[i + 1];
                object threshold = conditions[i + 2];

                if (threshold is bool)
                    transition.AddCondition(mode, (bool)threshold ? 1 : 0, paramName);
                else if (threshold is int)
                    transition.AddCondition(mode, (int)threshold, paramName);
                else if (threshold is float)
                    transition.AddCondition(mode, (float)threshold, paramName);
            }
        }

        static void AddTransitionToDeath(AnimatorState fromState, AnimatorState deathState)
        {
            var transition = fromState.AddTransition(deathState);
            transition.hasExitTime = false;
            transition.duration = 0;
            transition.AddCondition(AnimatorConditionMode.If, 0, "death");
        }

        [Serializable]
        public struct AnimationContext
        {
            public string name;
            public string filename;
            public int rowIndex;
            public int columnIndex;
            public int frameCount;
            public int frameWidth;
            public int frameHeight;
            public bool loop;
            public AnimationEvent[] events;
        }

        [MenuItem("MyMenu/Create Animator and Animations")]
        static void CreateAnimatorAndAnimations()
        {
            // Create the animator controller
            CreateController();

            // Create animation clips
            List<AnimationClip> createdClips = CreateAnimationClips();

            // Assign animation clips to the animator controller
            AssignClipsToController(createdClips);
        }

        static List<AnimationClip> CreateAnimationClips()
        {
            List<AnimationClip> createdClips = new List<AnimationClip>();

            // Define your animation contexts
            AnimationContext[] contexts =
            {
                new AnimationContext
                {
                    name = "idle", filename = "player1", rowIndex = 0, columnIndex = 0, frameCount = 6,
                    frameWidth = 100,
                    frameHeight = 100, loop = true
                },
                new AnimationContext
                {
                    name = "walk", filename = "player1", rowIndex = 1, columnIndex = 0, frameCount = 7,
                    frameWidth = 100,
                    frameHeight = 100, loop = true
                },
                new AnimationContext
                {
                    name = "jump_up", filename = "player1", rowIndex = 2, columnIndex = 0, frameCount = 5,
                    frameWidth = 100,
                    frameHeight = 100, loop = false
                },
                new AnimationContext
                {
                    name = "jump_down", filename = "player1", rowIndex = 4, columnIndex = 0, frameCount = 4,
                    frameWidth = 100,
                    frameHeight = 100, loop = false
                },
                new AnimationContext
                {
                    name = "crouch_idle", filename = "player1", rowIndex = 5, columnIndex = 0, frameCount = 8,
                    frameWidth = 100,
                    frameHeight = 100, loop = true
                },
                new AnimationContext
                {
                    name = "crouch_walk", filename = "player1", rowIndex = 6, columnIndex = 0, frameCount = 7,
                    frameWidth = 100,
                    frameHeight = 100, loop = true
                },
                new AnimationContext
                {
                    name = "slide", filename = "player1", rowIndex = 7, columnIndex = 0, frameCount = 4,
                    frameWidth = 100,
                    frameHeight = 100, loop = true
                },
                new AnimationContext
                {
                    name = "hit", filename = "player1", rowIndex = 8, columnIndex = 0, frameCount = 4, frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.5f, functionName = "OnHitAnimationEnd"
                        }
                    }
                },
                new AnimationContext
                {
                    name = "crouch_hit", filename = "player1", rowIndex = 9, columnIndex = 0, frameCount = 4,
                    frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.5f, functionName = "OnHitAnimationEnd"
                        }
                    }
                },
                new AnimationContext
                {
                    name = "range_attack", filename = "player1", rowIndex = 10, columnIndex = 0, frameCount = 3,
                    frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.4f, functionName = "OnRangeAttackAnimationEnd"
                        }
                    }
                },
                new AnimationContext
                {
                    name = "death", filename = "player1", rowIndex = 11, columnIndex = 0, frameCount = 7,
                    frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.8f, functionName = "OnDeathAnimationEnd"
                        }
                    }
                },
                new AnimationContext
                {
                    name = "attack", filename = "player1", rowIndex = 12, columnIndex = 0, frameCount = 6,
                    frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.7f, functionName = "OnAttackAnimationRealEnd"
                        }
                    }
                },
                new AnimationContext
                {
                    name = "crouch_attack", filename = "player1", rowIndex = 13, columnIndex = 0, frameCount = 6,
                    frameWidth = 100,
                    frameHeight = 100, loop = false, events = new[]
                    {
                        new AnimationEvent
                        {
                            time = 0.7f, functionName = "OnAttackAnimationRealEnd"
                        }
                    }
                },
            };

            foreach (var context in contexts)
            {
                AnimationClip clip = CreateAnimationClip(context);
                createdClips.Add(clip);
            }

            return createdClips;
        }

        static AnimationClip CreateAnimationClip(AnimationContext context)
        {
            AnimationClip clip = new AnimationClip
            {
                name = context.name,
                frameRate = 12,
            };

            float frameTime = 1f / clip.frameRate;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            string assetPath = $"Assets/Textures/{context.filename}.png";
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            var sprites = assets.Where(asset => asset is Sprite).ToArray();
            Texture2D texture = assets.FirstOrDefault(asset => asset is Texture2D) as Texture2D;

            if (sprites.Length == 0)
            {
                Debug.LogError($"No sprites found in the asset: {assetPath}");
                return null;
            }

            List<ObjectReferenceKeyframe> spriteKeyFrames = new List<ObjectReferenceKeyframe>();

            for (int i = 0; i < context.frameCount; i++)
            {
                float correctXPosition = context.columnIndex * context.frameWidth + i * context.frameWidth;
                float correctYPosition = texture.height - (context.rowIndex + 1) * context.frameHeight;
                int spriteIndex = sprites.ToList().FindIndex(sprite =>
                {
                    Rect rect = ((Sprite)sprite).rect;
                    return rect.x == correctXPosition && rect.y == correctYPosition;
                });
                if (spriteIndex == -1)
                {
                    Debug.LogError(
                        $"No sprite found at position ({correctXPosition}, {correctYPosition}) for the animation {context.name}");
                    return null;
                }

                if (spriteIndex < sprites.Length)
                {
                    Sprite sprite = sprites[spriteIndex] as Sprite;
                    if (sprite != null)
                    {
                        spriteKeyFrames.Add(new ObjectReferenceKeyframe
                        {
                            time = i * frameTime,
                            value = sprite
                        });
                    }
                }
            }

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, spriteKeyFrames.ToArray());

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = context.loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            if (context.events != null)
            {
                AnimationUtility.SetAnimationEvents(clip, context.events);
            }

            clip.legacy = false;

            AssetDatabase.CreateAsset(clip, $"Assets/Anims/player/{clip.name}.anim");
            AssetDatabase.SaveAssets();

            return clip;
        }

        static void AssignClipsToController(List<AnimationClip> clips)
        {
            // Load the existing animator controller
            AnimatorController controller =
                AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Anims/player/player.controller");

            if (controller != null)
            {
                // Get all states from the controller
                AnimatorControllerLayer layer = controller.layers[0];
                ChildAnimatorState[] states = layer.stateMachine.states;

                // Assign clips to states based on name matching
                foreach (var state in states)
                {
                    AnimationClip matchingClip =
                        clips.Find(clip =>
                            string.Equals(clip.name, state.state.name, StringComparison.CurrentCultureIgnoreCase));
                    if (matchingClip != null)
                    {
                        state.state.motion = matchingClip;
                    }
                }

                // Save the changes
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
            }
        }
    }
}