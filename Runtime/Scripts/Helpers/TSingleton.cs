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

        protected static bool firstInstancePersistent;
        #region Fields
        [SerializeField] protected bool _dontDestroyOnLoad;
        [SerializeField] protected bool _firstInstancePersistent;
        [SerializeField] protected bool _keepAllInstances;
        #endregion

        /// <summary>
        /// Initializes the singleton
        /// </summary>
        protected virtual void Init()
        {
            firstInstancePersistent = _firstInstancePersistent;
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);

            if (_keepAllInstances)
            {
                if(!firstInstancePersistent || _instance == null)
                _instance = this as T;
                return;
            }

            if (_instance == null) { 
                _instance = this as T;
                return;
            }
            
            if(firstInstancePersistent)
            {
                Destroy(this.gameObject);
                return;
            }
                Destroy(_instance.gameObject);
                _instance = this as T;
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