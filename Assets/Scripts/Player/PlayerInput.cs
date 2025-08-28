using System;
using System.Collections.Generic;
using SFIT.RTS.EventBus;
using SFIT.RTS.Events;
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
        [SerializeField] private LayerMask selectableLayers;
        [SerializeField] private LayerMask floorLayers;
        [SerializeField] private RectTransform selectionBox;

        private Vector2 startingMousePosition;

        private CinemachineFollow cinemachineFollow;
        private float zoomStartTime;
        private float rotationStartTime;
        private Vector3 startingFollowOffset;

        private float maxRotationAmount;

        private List<ISelectable> selectedUnits = new(12);
        private HashSet<AbstractUnit> aliveUnits = new(100);


        private void Awake() {
            if (!cinemachineCamera.TryGetComponent(out cinemachineFollow)) {
                Debug.LogError("Cinemachine Camera does not have a Cinemachine Follow component.");
            }

            startingFollowOffset = cinemachineFollow.FollowOffset;
            maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

            Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            Bus<UnitSpawnedEvent>.OnEvent += HandleUnitSpawned;
        }

        private void OnDestroy() {
            Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
            Bus<UnitSpawnedEvent>.OnEvent -= HandleUnitSpawned;

        }
        private void HandleUnitSelected(UnitSelectedEvent evt) => selectedUnits.Add(evt.Unit);
        private void HandleUnitDeselected(UnitDeselectedEvent evt) => selectedUnits.Remove(evt.Unit);
        private void HandleUnitSpawned(UnitSpawnedEvent args) => aliveUnits.Add(args.Unit);

        private void Update() {
            HandlePanning();
            HandleZooming();
            HandleRotation();
            HandleLeftClick();
            HandleRightClick();
            HandleDragSelect();
        }


        private void ResizeSelectionBox() {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float width = mousePosition.x - startingMousePosition.x;
            float height = mousePosition.y - startingMousePosition.y;

            selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2f, height / 2f);
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        }

        private void ResetSelectionBox() {
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(0), Mathf.Abs(0));
        }

        private void HandleRightClick() {
            if (camera == null || selectedUnits == null || selectedUnits is not IMoveable) { return; }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Mouse.current.rightButton.wasReleasedThisFrame
                && Physics.Raycast(cameraRay, out RaycastHit hitInfo, float.MaxValue, floorLayers)) {
                if (selectedUnits is IMoveable worker) {
                    worker.MoveTo(hitInfo.point);
                }
            }
        }

        private void HandleLeftClick() {
            if (camera == null) { return; }

            Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                if (selectedUnits != null) {
                    selectedUnits[0].Deselect();
                }

                if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, float.MaxValue, selectableLayers)
                    && hitInfo.collider.TryGetComponent(out ISelectable selectable)) {
                    selectable.Select();
                }
            }

        }
        private void HandleDragSelect() {
            if (selectionBox == null) { return; }
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                selectionBox.gameObject.SetActive(true);

                startingMousePosition = Mouse.current.position.ReadValue();
            } else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame) {
                ResizeSelectionBox();
            } else if (Mouse.current.leftButton.wasReleasedThisFrame) {
                selectionBox.gameObject.SetActive(false);
                ResetSelectionBox();
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
