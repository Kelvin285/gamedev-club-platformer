using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool on_ground = false;
    public Vector3 motion;
    public Vector3 grounded_pos = Vector3.zero;
    public float dash = 0;
    public bool jump = false;
    public float coyote = 0;

    public GameObject camera;
    public GameObject camera_target;
    public bool can_jump = false;
    public float buffer_jump = 0;
    public bool can_dash = false;
    public Vector3 facing_dir = new Vector3(0, 0, 1);
    bool on_wall = false;
    public Vector3 wall_normal = Vector3.zero;
    public float faster = 1.0f;
    public float fastest = 4.0f;
    public float fasting = 0.5f;
    public float buffer_dash = 0.0f;
    public bool just_dashed = false;

    public Animator animator;

    void Start()
    {
    }

    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        just_dashed = false;
        float delta = Time.fixedDeltaTime;

        Quaternion old_cam_rot = new Quaternion(camera.transform.rotation.x, camera.transform.rotation.y, camera.transform.rotation.z, camera.transform.rotation.w);

        camera.transform.LookAt(transform.position);

        camera.transform.rotation = Quaternion.Slerp(old_cam_rot, camera.transform.rotation, delta * 8);

        camera_target.transform.position = Vector3.Lerp(camera_target.transform.position, transform.position, delta * 8);

        Vector3 input = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            input.z += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            input.z -= 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            input.x -= 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            input.x += 1;
        }
        Vector3 raw_input = new Vector3(input.x, 0, input.z);
        input = input.normalized;

        float movement_effect = on_ground ? 1 : 0.5f;

        float speed = 1.0f;

        bool running = false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = 2.0f;
            running = true;
        }

        input *= 8.0f * speed;

        if (dash <= 0)
        {
            if (input.magnitude > 0)
            {
                facing_dir = input.normalized;
            }
            motion = Vector3.Lerp(motion, new Vector3(input.x, motion.y, input.z), delta * 16 * movement_effect);
        } else
        {
            if (on_wall)
            {
                motion = Vector3.Lerp(motion, new Vector3(input.x, motion.y, input.z), delta * 16 * movement_effect);
            }
        }

        if (buffer_dash > 0)
        {
            buffer_dash -= delta;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            buffer_jump = 0.1f;

            if (jump && can_dash || on_wall && !on_ground && can_dash)
            {
                buffer_dash = 0.1f;
            }
        } else
        {
            can_dash = true;
        }

        if ((jump || on_wall || running) && Vector3.Dot(motion, input) > 0 && (dash <= 0 || on_wall) && buffer_dash > 0)
        {
            can_dash = false;
            buffer_dash = 0;
            if (!on_wall)
            {
                motion.x = input.x * 2;
                motion.z = input.z * 2;
                dash = 0.1f;
                transform.rotation = Quaternion.LookRotation(input.normalized) * Quaternion.AngleAxis(90.0f, new Vector3(-1, 0, 0));
                can_jump = false;
                buffer_jump = 0;
                facing_dir = new Vector3(motion.x, 0, motion.z).normalized;

                if (faster < fastest)
                {
                    faster += fasting;
                }
                just_dashed = true;
            }
            else
            {
                if (wall_normal.x != 0)
                {
                    motion.x = wall_normal.x * 8.0f * speed;
                }
                else
                {
                    //motion.x = raw_input.x * 8.0f;
                }
                if (wall_normal.z != 0)
                {
                    motion.z = wall_normal.z * 8.0f * speed;
                }
                else
                {
                    //motion.z = raw_input.z * 8.0f;
                }

                if (raw_input.x == 0 || raw_input.z == 0)
                {
                    motion.y = 10;
                }
                else
                {
                    motion.y = 5;
                }
                dash = 0.1f;
                transform.rotation = Quaternion.LookRotation(input.normalized) * Quaternion.AngleAxis(90.0f, new Vector3(-1, 0, 0));
                can_jump = false;
                buffer_jump = 0;
                on_wall = false;
                facing_dir = new Vector3(motion.x, 0, motion.z).normalized;

                if (faster < fastest)
                {
                    faster += fasting;
                }
                just_dashed = true;
            }
        }

        if (buffer_jump > 0)
        {
            buffer_jump -= delta;
            if (can_jump)
            {
                if (coyote > 0 && !jump)
                {
                    on_ground = false;
                    motion += Vector3.up * 10;
                    jump = true;
                    can_jump = false;
                    buffer_jump = 0;
                    dash = 0;
                    can_dash = false;
                }
            }

        }
        else
        {
            can_jump = true;
        }

        if (dash > 0 && !on_ground && !on_wall)
        {
            transform.rotation = Quaternion.LookRotation(motion.normalized) * Quaternion.AngleAxis(90.0f, new Vector3(-1, 0, 0));
        } else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(facing_dir.normalized) * Quaternion.AngleAxis(90.0f, new Vector3(-1, 0, 0)), delta * 16 * movement_effect);
        }



        //width = 0.5f;
        //height = 1.65
        Vector3 foot_pos = transform.position - new Vector3(0, 1.65f * 0.6f, 0);
        Vector3 head_pos = transform.position + new Vector3(0, 1.65f * 0.5f, 0);

        Vector3 movement = new Vector3(motion.x * faster, motion.y, motion.z * faster) * delta;

        Vector3 movement_xz = new Vector3(motion.x, 0, motion.z) * delta * faster;

        if (facing_dir.magnitude > 0)
        {
            bool last_on_wall = on_wall;
            on_wall = false;
            List<RaycastHit> hits = new List<RaycastHit>();
            hits.AddRange(Physics.RaycastAll(transform.position, movement_xz.normalized, facing_dir.magnitude));
            hits.AddRange(Physics.RaycastAll(transform.position, new Vector3(Mathf.Sign(movement_xz.x), 0, 0), facing_dir.magnitude));
            hits.AddRange(Physics.RaycastAll(transform.position, new Vector3(0, 0, Mathf.Sign(movement_xz.z)), facing_dir.magnitude));

            var move = hits.ToArray();

            float closest = 1e30f;
            for (int i = 0; i < move.Length; i++)
            {
                var test = move[i];
                if (test.collider.gameObject == gameObject) continue;
                if (!test.collider.isTrigger)
                {
                    if (Vector3.Dot(new Vector3(test.point.x - transform.position.x, 0, test.point.z - transform.position.z).normalized, facing_dir) > 0)
                    {
                        if (Vector3.Dot(facing_dir.normalized, test.normal) < 0)
                        {
                            float dist = Vector3.Distance(test.point, transform.position);
                            if (dist < closest)
                            {
                                closest = dist;
                                on_wall = true;
                                wall_normal = test.normal;
                            }
                        }
                    }

                }
            }
            if (last_on_wall && !on_wall)
            {
                if (!just_dashed)
                {
                    dash = 0;
                }
            }
        }

        if (movement.y <= 0) {
            float highest_y = -1e30f;
            bool found = false;
            var down = Physics.SphereCastAll(transform.position, 0.25f, Vector3.down, Mathf.Abs(foot_pos.y - head_pos.y) + Mathf.Abs(movement.y) + 0.25f);
            for (int i = 0; i < down.Length; i++)
            {
                var test = down[i];
                if (test.collider.gameObject == gameObject) continue;
                if (!test.collider.isTrigger)
                {
                    var point = test.point;

                    if (point.y > highest_y && test.normal.y > 0 && point.y >= foot_pos.y + movement.y - 0.1f)
                    {
                        highest_y = point.y;
                        found = true;
                    }
                }
            }
            if (found)
            {
                on_ground = true;

                transform.position += Vector3.up * (highest_y - foot_pos.y);
                motion.y = 0;
                grounded_pos = transform.position;

                jump = false;
                coyote = 0.1f;
            } else
            {
                on_ground = false;
            }
        }

        {
            float lowest_y = 1e30f;
            bool found = false;
            var up = Physics.SphereCastAll(head_pos - new Vector3(0, 0.5f, 0), 0.25f, Vector3.up, Mathf.Abs(movement.y) + 0.5f);
            for (int i = 0; i < up.Length; i++)
            {
                var test = up[i];
                if (test.collider.gameObject == gameObject) continue;
                if (!test.collider.isTrigger)
                {
                    var point = test.point;

                    if (point.y < lowest_y && test.normal.y < 0)
                    {
                        if (point.y < head_pos.y + movement.y && point.y > transform.position.y)
                        {
                            lowest_y = point.y;
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                transform.position += Vector3.up * (lowest_y - head_pos.y);
                if (motion.y > 0)
                {
                    motion.y = 0;
                }
            }
        }

        if (movement_xz.magnitude > 0)
        {
            var move = Physics.CapsuleCastAll(foot_pos + Vector3.up * 0.75f, head_pos - Vector3.up * 0.25f, 0.25f, movement_xz.normalized, movement_xz.magnitude);
            for (int i = 0; i < move.Length; i++)
            {
                var test = move[i];
                if (test.collider.gameObject == gameObject) continue;
                if (!test.collider.isTrigger)
                {
                    if (Vector3.Dot(new Vector3(test.point.x - transform.position.x, 0, test.point.z - transform.position.z).normalized, movement_xz) > 0)
                    {
                        if (Vector3.Dot(movement_xz.normalized, test.normal) < 0)
                        {
                            movement_xz -= new Vector3(movement_xz.x * Mathf.Abs(test.normal.x), 0, movement_xz.z * Mathf.Abs(test.normal.z));
                        }
                    }

                }
            }
            movement.x = movement_xz.x;
            movement.z = movement_xz.z;
        }

        transform.position += movement;

        if (dash <= 0)
        {
            motion = (movement / delta);
            motion.x /= faster;
            motion.z /= faster;
        } else
        {
            motion.y = movement.y / delta;
        }

        if (!on_ground)
        {
            float max = -66 * (on_wall ? 0.25f : 1.0f);
            if (motion.y > max)
            {
                motion.y -= 30 * delta;
            }
            if (coyote > 0)
            {
                coyote -= 0.1f;
            }
        } else
        {
            motion.y = 0;
            if (dash > 0)
            {
                dash -= delta;
            }
            jump = false;
            coyote = 0.1f;
            if (faster > 1)
            {
                faster -= delta * 4;
            }
            if (faster < 1)
            {
                faster = 1;
            }
        }

        if (transform.position.y < -10)
        {
            motion *= 0;
            transform.position = grounded_pos;
        }

        animator.SetBool("diving", dash > 0);
        animator.SetBool("walking", input.magnitude > 0);
        animator.SetBool("jump", jump && dash <= 0);
        animator.SetBool("on_wall", !on_ground && on_wall);
        animator.SetFloat("sprinting", running ? 1 : 0);
    }
}
