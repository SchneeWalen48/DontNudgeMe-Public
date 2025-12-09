//using UnityEngine;
//using UnityEngine.Assertions.Must;

//[ExecuteAlways]
//public class AudioRangeFromCamera : MonoBehaviour
//{
//    [SerializeField] private AudioSource audioSource;
//    [SerializeField] private Transform listener;

//    private void Update()
//    {
//        if (Input.GetMouseButtonDown(0))
//        {
//            audioSource.Play();
//        }
//    }

//    private void Awake()
//    {
//        audioSource = transform.Find("AudioManager").GetComponent<AudioSource>();
        
//    }

//    private void OnDrawGizmos()
//    {
//        if (audioSource == null)
//            return;

//        // 카메라의 AudioListener 찾기
//        if (listener == null)
//        {
//            var audioListener = FindObjectOfType<AudioListener>();
//            if (audioListener != null && (audioListener.enabled == true))
//                listener = audioListener.transform;
//        }

//        if (listener == null)
//            return;

//        // 카메라(Listener) 위치에서 AudioSource까지 거리
//        float distance = Vector3.Distance(listener.position, audioSource.transform.position);

//        Gizmos.color = Color.cyan;
//        Gizmos.DrawLine(listener.position, audioSource.transform.position);
//        Gizmos.DrawWireSphere(audioSource.transform.position, audioSource.minDistance);
//        Gizmos.DrawWireSphere(audioSource.transform.position, audioSource.maxDistance);

//        // 현재 카메라와 사운드의 거리 표시
//        UnityEditor.Handles.Label(audioSource.transform.position + Vector3.up * 0.5f,
//            $"Dist: {distance:F2} / Min: {audioSource.minDistance:F1} / Max: {audioSource.maxDistance:F1}");
//    }
//}
