using UnityEngine;

namespace KsubakaPool
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static GameObject _instanceGO;
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    string typeName = typeof(T).Name;

                    // find the object name
                    GameObject _instanceGO = GameObject.Find(typeName);
                    _instance = _instanceGO.GetComponent<T>();

                    // making sure that there is only one object of this type at anytime
                    if (_instanceGO == null && _instance == null)
                    {
                        // create an empty gameobject
                        _instanceGO = new GameObject();

                        // track the object by its type name
                        _instanceGO.name = typeName;

                        // create singleton object
                        _instance = _instanceGO.AddComponent<T>();
                    }


                    // this makes sure that there is only one object of this type
                    // lifetime of this object is the application lifetime
                    GameObject.DontDestroyOnLoad(_instanceGO);
                }

                return _instance;
            }
        }

        protected virtual void Init()
        {

        }

        protected virtual void Awake()
        { }

        protected virtual void Start()
        { }

        protected virtual void Update()
        { }

        protected virtual void OnDestroy()
        {
            _instanceGO = null;
            _instance = null;
        }

    }
}
