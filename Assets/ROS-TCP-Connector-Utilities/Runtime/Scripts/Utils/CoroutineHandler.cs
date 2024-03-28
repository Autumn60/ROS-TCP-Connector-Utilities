using System.Collections;
using UnityEngine;

namespace Unity.Robotics.ROSTCPConnector.Utilities
{
    public class CoroutineHandler : MonoBehaviour
    {
        static protected CoroutineHandler _instance;
        static public CoroutineHandler instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject o = new GameObject("CoroutineHandler");
                    DontDestroyOnLoad(o);
                    _instance = o.AddComponent<CoroutineHandler>();
                }

                return _instance;
            }
        }

        public void OnDisable()
        {
            if (_instance)
                Destroy(_instance.gameObject);
        }

        public static Coroutine StartStaticCoroutine(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

        public static void PauseStaticCoroutine(Coroutine coroutine)
        {
            instance.StopCoroutine(coroutine);
        }

        public static void StopStaticCoroutine(ref Coroutine coroutine)
        {
            instance.StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
