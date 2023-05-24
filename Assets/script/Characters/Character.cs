using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum CharacterType
{
    None = 0,
    Lion = 1,
    Rose = 2    
}


public class Character : MonoBehaviour
{
    public int team;
    public int currentX;
    public int currentY;
    public CharacterType type;

    private Vector3 desiredPosition;
    private Vector3 diseredScale = new Vector3 (0.3f,0.3f,0.3f);


    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.MoveTowards(transform.localScale, diseredScale, Time.deltaTime * 10);
    }

    public virtual void SetPosition(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }   
    
    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredPosition = scale;
        if (force)
            transform.position = diseredScale;
    }
}
