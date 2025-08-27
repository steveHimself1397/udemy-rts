using System;
using SFIT.RTS.Units;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SFIT.RTS.Player {
    public class PlayerInput : MonoBehaviour {
        [SerializeField] private Rigidbody cameraTarget;
        [SerializeField] private CinemachineCamera cinemachineCamera;
        [SerializeField] private new Camera camera;

        [SerializeField] private CameraConfig cameraConfig;
        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;

        private float maxRotationAmount;

        private ISelectable selectedUnit;


        private void Awake() {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow)) {
                Debug.LogError("Cinemachine Camera does not have a Cinemachine Follow component.");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
        }

        private void Update() {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleLeftClick();
        }

        private void HandleLeftClick() {
            if (camera == null) { return; }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                if (selectedUnit != null) {
                    selectedUnit.Deselect();
                    selectedUnit = null;
                }

                if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, float.MaxValue, LayerMask.GetMask("Default"))
                    && hitInfo.collider.TryGetComponent(out ISelectable selectable)) {
                    selectable.Select();
                    selectedUnit = selectable;
                }
            }

        }

        private void HandleRotation() {
            if (ShouldSetRotationStartTime()) {
                rotationStartTime = Time.time;
            }

            float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);

            Vector3 targetFollowOffset;

            if (Keyboard.current.pageUpKey.isPressed) {
                targetFollowOffset = new Vector3(maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
            } else if (Keyboard.current.pageDownKey.isPressed) {
                targetFollowOffset = new Vector3(-maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
            } else {
                targetFollowOffset = new Vector3(startingFollowOffset.x, cinemachineFollow.FollowOffset.y, startingFollowOffset.z);
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, rotationTime);
        }

        private bool ShouldSetRotationStartTime() {
            return Keyboard.current.pageUpKey.wasPressedThisFrame || Keyboard.current.pageDownKey.wasPressedThisFrame
                   || Keyboard.current.pageUpKey.wasReleasedThisFrame || Keyboard.current.pageDownKey.wasReleasedThisFrame;
        }

        private void HandleZooming() {

            if (ShouldSetZoom()) {
                zoomStartTime = Time.time;
            }

            Vector3 targetFollowOffset;

            float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);

            if (Keyboard.current.uKey.isPressed) {
                targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, cameraConfig.MinZoomDistance, cinemachineFollow.FollowOffset.z);
            } else {
                targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, startingFollowOffset.y, cinemachineFollow.FollowOffset.z);
            }

            cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, zoomTime);
        }

        private bool ShouldSetZoom() {
            return Keyboard.current.uKey.wasPressedThisFrame || Keyboard.current.uKey.wasReleasedThisFrame;
        }

        private void HandlePanning() {
            Vector2 moveAmount = GetKeyboardMoveAmount();
            moveAmount += GetMouseMoveAmount();

            cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0f, moveAmount.y);
        }

        private Vector2 GetMouseMoveAmount() {
            Vector2 moveAmount = Vector2.zero;

            if (!cameraConfig.EnableEdgePan) {
                return moveAmount;
            }

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            int screenWidth = Screen.width;
            int screenHeight = Screen.height;

            if (mousePosition.x <= cameraConfig.EdgePanSize) {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            } else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize) {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize) {
                moveAmount.y += cameraConfig.MousePanSpeed;
            } else if (mousePosition.y <= cameraConfig.EdgePanSize) {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        private Vector2 GetKeyboardMoveAmount() {
            Vector2 moveAmount = Vector2.zero;
            if (Keyboard.current.upArrowKey.isPressed) {
                moveAmount.y += cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.leftArrowKey.isPressed) {
                moveAmount.x -= cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.downArrowKey.isPressed) {
                moveAmount.y -= cameraConfig.KeyboardPanSpeed;
            }

            if (Keyboard.current.rightArrowKey.isPressed) {
                moveAmount.x += cameraConfig.KeyboardPanSpeed;
            }

            return moveAmount;

        }
    }
}
