using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class FollowTransformManager : NetworkBehaviour
{
    private readonly Dictionary<Transform, Transform> _followersTargets = new();
    
    public static FollowTransformManager Instance;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void Follow(Transform follower, Transform target)
    {
        _followersTargets.Add(follower, target);
    }
    
    public void Unfollow(Transform follower)
    {
        _followersTargets.Remove(follower);
    }

    private void LateUpdate()
    {
        foreach (var (follower, target) in _followersTargets)
        {
            follower.position = target.position;
            follower.rotation = target.rotation;
        }
    }
}