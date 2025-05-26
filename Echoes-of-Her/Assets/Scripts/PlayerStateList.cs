using UnityEngine;

public class PlayerStateList : MonoBehaviour
{
    public bool jumping = false;
    public bool dashing = false;
    public bool recoilingX, recoilingY;
    public float recoilingFromHitVertical = 0f;
    public float recoilingFromHitHorizontal = 0f;
    public bool lookingRight = true;
    public bool invincible = false;
    public bool beenHit = false;
    public bool healing = false;
    public bool casting = false;
    public bool cutscene = false;
    public bool alive = true;
}
