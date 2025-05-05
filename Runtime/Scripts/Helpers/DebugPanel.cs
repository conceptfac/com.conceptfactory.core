using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Concept.Helpers
{

    /// <summary>
    /// This class manages the Degub Panel on UI
    /// </summary>
public class DebugPanel : MonoBehaviour
{

        #region Singleton Instance
        private static DebugPanel _instance;
        public static DebugPanel Instance { get { return _instance; } }
        #endregion

        #region Fields

        [SerializeField] bool _startFolded = true;

        [SerializeField] bool _followUser = true;
        [Range(0.1f, 1f)]
        [SerializeField] float _followSpeed = 1f;

        [Header("Components")]
        [SerializeField] TextMeshProUGUI TMP_DebugText;
        [SerializeField] bool _showMessages = false;
        [SerializeField] bool _showWarnings = true;
        [SerializeField] bool _showErrors = true;
        private Transform _transform;
        private Transform _mainCameraTransform;
        
        public Animator animator;
        #endregion


        #region MonoBehaviour Methods
        private void OnEnable()
        {
            Application.logMessageReceived += Debug;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= Debug;
        }

        private void Awake()
        {
            _transform = transform;
            animator = GetComponent<Animator>();
        }

        private void Start()
        {
            if (!_instance) _instance = this;
            else
                Destroy(gameObject);
        
            _mainCameraTransform = Camera.main.transform;
            
            
            animator?.SetBool("folded", _startFolded);

#if OCULUS
            if(GestureMonitor.Instance)
            GestureMonitor.Instance.OnPinchLeft += SetFold;
#endif
        }

        private void Update()
        {
            if (_followUser) FollowUser();
        }

        private void OnDestroy()
        {
#if OCULUS
            if (GestureMonitor.Instance)
                GestureMonitor.Instance.OnPinchLeft -= SetFold;
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sends a debug log to Debug Panel
        /// </summary>
        /// <param name="condition">Message</param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>

        private void Debug(string condition, string stackTrace = "", LogType type = LogType.Log)
        {
            string msg = null;

            // Se o stackTrace não for fornecido, tente gerar um a partir de uma exceção
            if (string.IsNullOrEmpty(stackTrace))
            {
                stackTrace = GetStackTrace();
            }


            switch (type)
            {
                case LogType.Error:
                    msg = _showErrors ? $"\n<color=#ff0000>{condition}\n{stackTrace}</color>":null;
                    break;
                case LogType.Warning:
                    msg = _showWarnings ? $"\n<color=#F77E00>{condition}</color>" : null;
                    break;
                case LogType.Log:
                    msg = _showMessages ? $"\n<color=#ffffff>{condition}</color>" : null;
                    break;
                    default:
                    msg = $"\n<color=#8184BE>{condition}</color>";
                    break;
                        

            }

            
            if(msg != null)
            TMP_DebugText.text += msg;
        }


        /// <summary>
        /// This Method captures a stack trace manually
        /// </summary>
        /// <returns></returns>
        private string GetStackTrace()
        {
            var stackTrace = new StackTrace(true); // "true" permite incluir a linha do código
            return stackTrace.ToString(); // Retorna o stack trace com número de linha e nome do arquivo
        }

        /// <summary>
        /// This method makes HUD follow the user
        /// </summary>
        /// 
        private void FollowUser()
        {
            Quaternion targetRotation = Quaternion.Euler(0f, _mainCameraTransform.eulerAngles.y, 0f);

            _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, (10f * _followSpeed) * Time.deltaTime);

            Vector3 desiredPosition = _mainCameraTransform.position;
            desiredPosition.y = 0f;

            _transform.position = desiredPosition;
        }


        [ContextMenu("Set Fold|Unfold")]
        private void SetFold()
        {
            bool folded = animator.GetBool("folded");
            animator.SetBool("folded", !folded);
        }


        #endregion

    }
}

