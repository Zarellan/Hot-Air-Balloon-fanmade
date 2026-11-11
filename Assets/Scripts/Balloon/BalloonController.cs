using UnityEngine;

public class BalloonController : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    public float blowForce = 10f; // Strength of the blow
    public float fanMaxDistance = 3f;
    private Rigidbody2D rb;

    [SerializeField] GameObject balloon_face;
    [SerializeField] GameObject basket, basketVisual, basketTrigger;
    [SerializeField] float faceSideOffset = 0.15f;   // how far face can move
    [SerializeField] float faceFollowSpeed = 8f;     // smoothing

    Animator anim;
    private float currentBlowForce;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // rb.linearDamping = 2;
        basket.GetComponent<Rigidbody2D>().linearDamping = 2;

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButton(0)) // Left mouse button held
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 inputDir = (rb.position - mousePos).normalized;
            Vector2 forward = Vector2.up; // assume balloon faces up
            float angle = Vector2.Angle(forward, inputDir);

            if (angle <= maxAngle)
            {
                float dist = Vector2.Distance(transform.position, mousePos);
                float strength = Mathf.Clamp01(1f - dist / fanMaxDistance);
                rb.AddForce(inputDir * blowForce * strength  * Time.deltaTime, ForceMode2D.Force);



                Vector2 toMouse = mousePos - (Vector2)transform.position;

                // Normalize so it’s just direction
                toMouse.Normalize();

                // We only care about LEFT / RIGHT
                float side_x = Mathf.Clamp(-toMouse.x, -1f, 1f);

                float side_y = Mathf.Clamp(-toMouse.y, -1f, 1f);

                Vector3 faceTargetLocalPos = new Vector3(
                    side_x * faceSideOffset,   // move left/right only
                    side_y * faceSideOffset,
                    balloon_face.transform.localPosition.z
                );

                balloon_face.transform.localPosition =
                    Vector3.Lerp(
                        balloon_face.transform.localPosition,
                        faceTargetLocalPos,
                        faceFollowSpeed * Time.deltaTime
                    );
            }
        }
        else
        {
            balloon_face.transform.localPosition =
            Vector3.Lerp(
                balloon_face.transform.localPosition,
                new Vector3(0,0),
                faceFollowSpeed * Time.deltaTime
            );
        }

        AnimationManager();
    }

    void AnimationManager()
    {
        
    }

    public float maxSpeed = 5f;
    public float maxAngle = 45f;

    public float maxDistance;

    bool isTouched;

    public float stableBloonGround = 1.1f;

    public float pull_strength;
    public float basket_weight;

    [SerializeField] float maxRopeDistance = 4.5f;
    [SerializeField] float ropePullStrength = 6f;
    [SerializeField] float ropeDamping = 4f;

    bool lockBasketX = false;
    [SerializeField] float verticalDeadZone = 0.4f;
    void FixedUpdate()
    {
        // Clamp balloon speed
        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            

        Vector2 balloonPos = transform.position;
        Vector2 basketPos = basket.transform.position;

        Vector2 delta = basketPos - balloonPos;
        float dist = delta.magnitude;

        // --- Basket "weight effect" ---
        // --- Basket "weight effect" ---
        float verticalPull = Mathf.Max(0f, balloonPos.y - basketPos.y); // positive if basket is below balloon
        float weightFactor = (verticalPull / maxDistance) * basket_weight; // 0..1

        // Reduce balloon blowForce
        currentBlowForce = blowForce * (1f - weightFactor);

        float xOffset = basketPos.x - balloonPos.x;
        // --- Apply downward pull only if basket not grounded ---
        if (!basket.GetComponent<Basket>().touched)
        {
            rb.AddForce(Vector2.down * weightFactor, ForceMode2D.Force);
            rb.gravityScale = -0.2f;
        }
        else
        {
            rb.AddForce(Vector2.down * stableBloonGround, ForceMode2D.Force);
            if (Mathf.Abs(basketPos.y - transform.position.y) > 1.1f)
                rb.gravityScale = -0.1f;
            else
                rb.gravityScale = -0.2f;
            // Basket is on ground → reduce downward pull to stabilize balloon
            float centerStrength = 10f; // tune this (small value)
            rb.linearVelocity /= new Vector2(stableBloonGround,stableBloonGround);

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            float dista = Vector2.Distance(transform.position, mousePos);

            if (!Input.GetMouseButton(0) || dista > fanMaxDistance) //continue
                rb.AddForce(Vector2.right * xOffset * centerStrength, ForceMode2D.Force);
        }
        
        bool hitRight = HitDetection(new Vector3(0,-0.17f,0), 0.12f).collider != null;
        bool hitLeft  = HitDetection(new Vector3(0,-0.17f,0), -0.3f).collider != null;

        bool sideBlocked = hitRight || hitLeft;

        float verticalOffset = balloonPos.y - basketPos.y;
        float verticalFollowSpeed = 3f; // tune this
        // --- Basket follow ---
        if (dist > maxDistance)
        {
            Vector2 dir = delta / dist;
            Vector2 targetPos = balloonPos + dir * maxDistance;
            if (basket.GetComponent<Basket>().touched && dist <= maxDistance * 2f)
            {
                targetPos = basketPos;
                isTouched = true;
            }
            else if (dist > maxDistance * 2f && isTouched && rb.linearVelocityY > 2f)
            {
                targetPos += new Vector2(0,0.0001f);
                isTouched = false;
            }

            if (dist > maxRopeDistance)
            {
                Vector2 pullDir = delta.normalized;
                
                // Pull balloon back toward basket
                rb.AddForce(pullDir * ropePullStrength, ForceMode2D.Force);

                // Kill velocity that goes AWAY from basket
                float awaySpeed = Vector2.Dot(rb.linearVelocity, -pullDir);
                if (awaySpeed > 0f)
                {
                    rb.linearVelocity -= (-pullDir) * awaySpeed;
                }
            }
            
            Vector2 newPos;

            // if (!basket.GetComponent<Basket>().touched)
            newPos = Vector2.Lerp(basketPos, targetPos, Time.fixedDeltaTime * pull_strength);
            // else
            //     newPos = Vector2.Lerp(basketPos, targetPos, Time.fixedDeltaTime * 2);
            // if (!basket.GetComponent<Basket>().touched || dist > maxDistance * 2)
            //     basket.transform.position = Vector2.Lerp(basketPos, targetPos, Time.fixedDeltaTime * pull_strength);
            // --- Stop vertical follow if close ---
            // if (basket.GetComponent<Basket>().touched && Mathf.Abs(basketPos.y - transform.position.y) < 1f)
            // {
            //     newPos.y = basketPos.y; // lock Y, only follow X
            // }

            float horizontalOffset = balloonPos.x - basketPos.x;   // how far balloon is from basket
            float followSpeed = 3f; // tune this: higher = basket moves faster to balloon


            if (sideBlocked && Vector2.Distance(transform.position,basket.transform.position) > verticalDeadZone)
            {
                print("jklklklsd");
                // Keep X stable, follow ONLY Y
                basket.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(
                    basket.GetComponent<Rigidbody2D>().linearVelocity.x,
                    verticalOffset * verticalFollowSpeed
                );
            }
            if (basketTrigger.GetComponent<BasketTrigger>().touched && Mathf.Abs(basketPos.y - transform.position.y) < 1.4f)
                basket.GetComponent<Rigidbody2D>().GetComponent<Rigidbody2D>().linearVelocity = new Vector2(horizontalOffset * followSpeed, basket.GetComponent<Rigidbody2D>().linearVelocity.y);
            else
                basket.transform.position = newPos;

                
            // if (HitDetection(new Vector3(0,-0.17f,0), -0.3f).collider != null && Mathf.Abs(basketPos.x - transform.position.x) > 1f)
            // {
            //     basket.GetComponent<Rigidbody2D>().GetComponent<Rigidbody2D>().linearVelocity = new Vector2(basket.GetComponent<Rigidbody2D>().linearVelocity.x + 0.1f, basket.GetComponent<Rigidbody2D>().linearVelocity.y + 0.2f);
            //     // basket.transform.position += new Vector3(0.2f,0);
            // }
            // else if (HitDetection(new Vector3(0,-0.17f,0), 0.12f).collider != null && Mathf.Abs(basketPos.x - transform.position.x) > 1f)
            // {
            //     basket.GetComponent<Rigidbody2D>().GetComponent<Rigidbody2D>().linearVelocity = new Vector2(basket.GetComponent<Rigidbody2D>().linearVelocity.x - 0.1f, basket.GetComponent<Rigidbody2D>().linearVelocity.y + 0.2f);
            //     // basket.transform.position -= new Vector3(0.2f,0);
            // }
        }

        print(verticalOffset > verticalDeadZone);
        Vector2 v = rb.linearVelocity;

        // Dampen X only (balloon)
        float xDamping = 3f; // tune this
        v.x = Mathf.Lerp(v.x, 0f, xDamping * Time.fixedDeltaTime);

        rb.linearVelocity = v;

        // Clamp basket Y velocity
        Rigidbody2D basketRb = basket.GetComponent<Rigidbody2D>();

        Vector2 dir2 = balloonPos - basketPos;

        // Angle so basket "faces" the balloon
        float angle = Mathf.Atan2(dir2.y, dir2.x) * Mathf.Rad2Deg - 90f;

        // Apply rotation
        basketVisual.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        if (basketRb.linearVelocity.y < -3f)
            basketRb.linearVelocity = new Vector2(basketRb.linearVelocity.x, -3f);
    }

    RaycastHit2D HitDetection(Vector3 offset, float distance)
    {
        Vector3 vec = Vector3.right;
        Vector2 origin = basket.transform.position + vec * 0.1f + offset;
        RaycastHit2D hit = Physics2D.Raycast(origin, vec, distance, groundLayer);
        Debug.DrawRay(origin, vec * distance, hit.collider ? Color.red : Color.green);
        return hit;

    }

    string animName;
    public void ChangeAnimation(string animNamePar)
    {
        if (this.animName == animNamePar) return;

        this.animName = animNamePar;

        anim.Play(animNamePar);

    }

    public void BlinkChance()
    {
        if (Random.Range(0.0f,1.0f) < 0.05)
        {
            ChangeAnimation("Blink");
        }
    }

}