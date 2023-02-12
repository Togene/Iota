using UnityEngine;

public class Follow : MonoBehaviour {

    public Transform target;
    
    [Range(0.1f, 10f)]
    public float     followSpeed = 0.5f;

    [Range(0.1f, 100f)] public float offset = 1f;
    void Start() {
        
    }
    
    void Update() {
        
    }

    private void LateUpdate() {
        if (target == null) {
            return;
        }
        transform.position = Vector3.Lerp(
            transform.position, 
            // remove the y aspect (up and down) of the position vector
            new Vector3(target.position.x - offset, transform.position.y, target.position.z - offset), 
            followSpeed * Time.deltaTime);
    }
}
