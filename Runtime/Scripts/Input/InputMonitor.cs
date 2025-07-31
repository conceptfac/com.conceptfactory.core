using Concept.Helpers;
using Concept.UI;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Concept.Core
{
    public class InputMonitor : TSingleton<InputMonitor>
    {
        #region Properties

        private static bool _isDragging;
        public static bool isDragging { get => _isDragging; }

        private Vector2 _touchStartPos;
        private float _initialPinchDistance = 0f;

        #endregion

        #region Delegates

        public delegate void onSelect(RaycastHit hit);
        public static onSelect OnSelect;

        public delegate void onTouch(float x, float y);
        public static onTouch OnTouch;

        public delegate void onRelease(float x, float y);
        public static onTouch OnRelease;

        public delegate void onCancelDrag(float x, float y);
        public static onTouch OnCancelDrag;

        #endregion
      

        protected override void Start()
        {
            base.Start();
        }
        protected override void Update()
        {
            if (!Application.isPlaying) return;
            base.Update();
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.touchCount > 0)
            {
                Ray ray = default;
                RaycastHit hit;

                //Touch Actions
                if (Input.touchCount == 1)  //1 Finger Touch
                {
                    Touch touch = Input.GetTouch(0);  // Captura o primeiro toque
                    ray = Camera.main.ScreenPointToRay(touch.position);

                    switch (touch.phase)
                    {
                        case UnityEngine.TouchPhase.Began:


                            _touchStartPos.x = touch.position.x;
                            _touchStartPos.y = touch.position.y;

                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnTouch(_touchStartPos.x, _touchStartPos.y));
                            OnTouch?.Invoke(_touchStartPos.x, _touchStartPos.y);
                            break;

                        case UnityEngine.TouchPhase.Moved:

                            float deltaX = touch.position.x - _touchStartPos.x;
                            float deltaY = touch.position.y - _touchStartPos.y;
                            if (Mathf.Abs(deltaX) > 1.5f) //Avoid short movments
                            {
                                _isDragging = true;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingHorizontal(deltaX));
                            }
                            if (Mathf.Abs(deltaY) > 1.5f) //Avoid short movments
                            {
                                _isDragging = true;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDraggingVertical(deltaY));
                            }


                            break;

                        case UnityEngine.TouchPhase.Ended:
                            if (!_isDragging)
                            {
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnSelect(hit.collider.gameObject));
                                    OnSelect?.Invoke(hit);
                                    return;
                                }
                                else
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnRelease(touch.position.x, touch.position.y));
                                    OnRelease?.Invoke(touch.position.x, touch.position.y);
                                }
                            }
                            else
                            {

                                _isDragging = false;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDragEnded(touch.position.x, touch.position.y));
                                OnCancelDrag?.Invoke(touch.position.x, touch.position.y);
                            }
                            break;
                        case UnityEngine.TouchPhase.Canceled:
                            if (_isDragging)
                            {
                                _isDragging = false;
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnDragEnded(touch.position.x, touch.position.y));
                                OnCancelDrag?.Invoke(touch.position.x, touch.position.y);
                            }
                            break;
                    }

                }
                else if (Input.touchCount == 2) // 2 Fingers Touch
                {
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);

                    switch (touch1.phase)
                    {
                        case UnityEngine.TouchPhase.Began:
                            _initialPinchDistance = Vector2.Distance(touch1.position, touch2.position);
                            CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnPinchingStart(_initialPinchDistance));
                            break;

                        case UnityEngine.TouchPhase.Moved:
                            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                            if (currentDistance > 100f && Mathf.Abs(currentDistance - _initialPinchDistance) > 5f)
                            {
                                float pinchDelta = (currentDistance - _initialPinchDistance);
                                pinchDelta = Mathf.Clamp(pinchDelta, -0.1f, 0.1f);
                                CallbackHub.CallAction<IInputCallBacks>(callback => callback.OnPinching(pinchDelta));
                            }

                            _initialPinchDistance = currentDistance;  // Update for next movment

                            break;
                    }
                }
                else if (Input.touchCount == 3) // 3 Fingers Touch NOT IMPLEMENTED YET
                {

                }



            }


            // === MOUSE INPUT ===
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _touchStartPos = mousePos;
                    CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnTouch(mousePos.x, mousePos.y));
                    OnTouch?.Invoke(mousePos.x, mousePos.y);
                }

                if (Mouse.current.leftButton.isPressed)
                {
                    float deltaX = mousePos.x - _touchStartPos.x;
                    float deltaY = mousePos.y - _touchStartPos.y;

                    if (Mathf.Abs(deltaX) > 1.5f)
                    {
                        _isDragging = true;
                        CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnDraggingHorizontal(deltaX));
                    }

                    if (Mathf.Abs(deltaY) > 1.5f)
                    {
                        _isDragging = true;
                        CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnDraggingVertical(deltaY));
                    }
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    RaycastHit hit;

                    if (!_isDragging)
                    {
                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnSelect(hit.collider.gameObject));
                            OnSelect?.Invoke(hit);
                        }
                        else
                        {
                            CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnRelease(mousePos.x, mousePos.y));
                            OnRelease?.Invoke(mousePos.x, mousePos.y);
                        }
                    }
                    else
                    {
                        _isDragging = false;
                        CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnDragEnded(mousePos.x, mousePos.y));
                        OnCancelDrag?.Invoke(mousePos.x, mousePos.y);
                    }
                }

                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnPinching(scroll * 0.01f));
                }
            }
        }

    }
}