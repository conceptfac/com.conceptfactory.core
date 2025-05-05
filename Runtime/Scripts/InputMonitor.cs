using Concept.Helpers;
using Concept.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
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
        private ScreenOrientation _lastScreenOrientation;

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

            /*
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                var touches = Touchscreen.current.touches;

                if (touches.Count == 1)
                {
                    var touch = touches[0];
                    var phase = touch.phase.ReadValue();
                    var pos = touch.position.ReadValue();

                    Ray ray = Camera.main.ScreenPointToRay(pos);
                    RaycastHit hit;

                    switch (phase)
                    {
                        case UnityEngine.InputSystem.TouchPhase.Began:
                            _touchStartPos = pos;
                            CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnTouch(pos.x, pos.y));
                            OnTouch?.Invoke(pos.x, pos.y);
                            break;

                        case UnityEngine.InputSystem.TouchPhase.Moved:
                            float deltaX = pos.x - _touchStartPos.x;
                            float deltaY = pos.y - _touchStartPos.y;

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
                            break;

                        case UnityEngine.InputSystem.TouchPhase.Ended:
                            if (!_isDragging)
                            {
                                if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnSelect(hit.collider.gameObject));
                                    OnSelect?.Invoke(hit);
                                }
                                else
                                {
                                    CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnRelease(pos.x, pos.y));
                                    OnRelease?.Invoke(pos.x, pos.y);
                                }
                            }
                            else
                            {
                                _isDragging = false;
                                CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnDragEnded(pos.x, pos.y));
                                OnCancelDrag?.Invoke(pos.x, pos.y);
                            }
                            break;
                    }
                }
                else if (touches.Count == 2)
                {
                    var touch1 = touches[0];
                    var touch2 = touches[1];

                    var pos1 = touch1.position.ReadValue();
                    var pos2 = touch2.position.ReadValue();

                    float dist = Vector2.Distance(pos1, pos2);
                    var phase1 = touch1.phase.ReadValue();

                    if (phase1 == UnityEngine.InputSystem.TouchPhase.Began)
                    {
                        _initialPinchDistance = dist;
                        CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnPinchingStart(_initialPinchDistance));
                    }
                    else if (phase1 == UnityEngine.InputSystem.TouchPhase.Moved)
                    {
                        float pinchDelta = dist - _initialPinchDistance;
                        if (Mathf.Abs(pinchDelta) > 5f)
                        {
                            pinchDelta = Mathf.Clamp(pinchDelta * 0.01f, -0.1f, 0.1f);
                            CallbackHub.CallAction<IInputCallBacks>(cb => cb.OnPinching(pinchDelta));
                        }

                        _initialPinchDistance = dist;
                    }
                }

                return; // Se for touch, ignora mouse abaixo
            }
            */

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


            if (_lastScreenOrientation != Screen.orientation)
            {
                _lastScreenOrientation = Screen.orientation;
                ScreenUtils.OnResolutionChanged?.Invoke(Screen.width,Screen.height);
                //ChangeOrientation(_lastScreenOrientation);
            }
#if UNITY_EDITOR
            if (Keyboard.current.oKey.wasPressedThisFrame)
            {
                //_isLandscape = !_isLandscape;

                //                SetSize(_isLandscape ? 6 : 5);
                //              ChangeOrientation(_isLandscape ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait);
            }


#endif
        }

        /*
        public static void ChangeOrientation(ScreenOrientation orientation)
        {
            Debug.Log("[InputMonitor] ChangeOrientation:" + orientation);

            if (orientation == ScreenOrientation.Portrait || orientation == ScreenOrientation.PortraitUpsideDown)
            {
                ScreenUtils.OnPortraitOrientation?.Invoke();
            }
            else
            {
                ScreenUtils.OnLandscapeOrientation?.Invoke();
            }
        }
*/
        public static int GetSize()
        {
            var gvWndType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            var size = selectedSizeIndexProp.GetValue(gvWnd, null);
            return (int)size;
        }

        public static void SetSize(int index)
        {
            var gvWndType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gvWndType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gvWndType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);

          //  ChangeOrientation((index == 6) ? ScreenOrientation.LandscapeLeft : ScreenOrientation.Portrait);


        }

    }
}