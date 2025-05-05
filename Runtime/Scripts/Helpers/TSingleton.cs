using UnityEngine;


namespace Concept.Helpers
{
    /// <summary>
    /// Twinny Singleton Helper
    /// This script turn a inheritance class to singleton
    /// </summary>
    /// <typeparam name="T">Class to turn singleton</typeparam>
    public abstract class TSingleton<T> : MonoBehaviour where T : TSingleton<T>
    {
        #region Singleton Instance
        protected static T _instance;
        public static T Instance { get { return _instance; } }
        #endregion

        #region Fields
        [SerializeField] private bool _dontDestroyOnLoad = false;
        [SerializeField] private bool _keepAllInstances = false;
        [SerializeField] private bool _firstInstancePersistent = false;
        [SerializeField] private bool _lastInstancePersistent = false;
        #endregion

        /// <summary>
        /// Initializes the singleton
        /// </summary>
        public virtual void Init()
        {
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            if (_keepAllInstances)
            {
                if(!_firstInstancePersistent || !_instance)
                _instance = this as T;
            }
            else
            {
                if (!_instance) _instance = this as T;
                else
                    if (_lastInstancePersistent) { 
                    Destroy(_instance.gameObject);
                    _instance = this as T;
                }
                else
                    Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate(){}
#endif

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Start(){}

        protected virtual void Update() {}
    }

   

}