using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip movingSound;
    
    private Vector3[] waypoints = new Vector3[]
    {
        new Vector3(2f, -2f, 0f),  // 左上角
        new Vector3(5f, -2f, 0f),  // 右上角
        new Vector3(5f, -4f, 0f),  // 右下角
        new Vector3(2f, -4f, 0f)   // 左下角
    };
    
    private int currentWaypointIndex = 0;
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        transform.position = waypoints[0];
        
        if (audioSource && movingSound)
        {
            audioSource.clip = movingSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    
    void Update()
    {
        MoveTowardsWaypoint();
    }
    
    void MoveTowardsWaypoint()
    {
        Vector3 targetPosition = waypoints[currentWaypointIndex];
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // 帧率独立移动
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // 设置动画方向
        SetAnimationDirection(direction);
        
        // 检查是否到达路径点
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
    
    void SetAnimationDirection(Vector3 direction)
    {
        if (animator == null) return;
        
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // 水平移动
            if (direction.x > 0)
                animator.Play("WalkRight");
            else
                animator.Play("WalkLeft");
        }
        else
        {
            // 垂直移动
            if (direction.y > 0)
                animator.Play("WalkUp");
            else
                animator.Play("WalkDown");
        }
    }
}
