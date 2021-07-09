using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;

#pragma warning disable 618, 649
namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        // 플레이어 앉기 설정
        [Header("Crouch Setting")]
        public float crouchSpeed;
        public float crouchHeight;
        private bool isCrouching;
        private float playerHeight;


         void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);

            // 앉기
            playerHeight = m_CharacterController.height;
        }


         void Update()
        {
            RotateView();
            // 점프 상태가 여기를 읽고 누락되지 않았는지 확인
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;

            // 앉기 키 설정
            isCrouching = Input.GetKey(KeyCode.LeftControl);

            // 앉기
            if (isCrouching)
            {
                m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, crouchHeight, Time.deltaTime * 8f);
            }
            else
            {
                float lastHeight = m_CharacterController.height;
                m_CharacterController.height = Mathf.Lerp(m_CharacterController.height, playerHeight, Time.deltaTime * 8f);
                Vector3 tmpPosition = transform.position;

                // 캐릭터 컨트롤러 높이 만큼 y축으로 곗고 더해 바닥과의 충돌을 방지함.
                tmpPosition.y += (m_CharacterController.height - lastHeight);
                transform.position = tmpPosition;
            }
        }


        void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


         void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // 카메라를 조준하는 방향이기 때문에 항상 카메라를 따라 앞으로 이동
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


         void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


         void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


         void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


         void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


         void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // 독립형 실행의 빌드 경우, 키를 눌러 걷기 / 뛰기 수정
            // 캐릭터의 걷기와 뛰기를 추정함
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // 세팅 값의 속도를 걷거나 뛰게함
            // 앉기 속도
            if(isCrouching && !m_Jumping)
            {
                speed = crouchSpeed;
            }
            else
            {
                speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            }

            m_Input = new Vector2(horizontal, vertical);

            // normalize 입력이 1을 초과하는 경우
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }

            // 앞쪽으로만 달리기. 키가 입력되지 않으면 속도를 걷는 속도로 고정
            if(!m_IsWalking)
            {
                speed = m_Input.y <= 0 ? m_WalkSpeed : m_RunSpeed;
                //if(m_Input.y <= 0)
                //{
                //    speed = m_WalkSpeed;
                //}
                //else
                //{
                //    speed = m_RunSpeed;
                //}
                
                // 만약에 스테미나가 0 이하면 속도가 걷는 속도로 변경
                if(transform.GetComponent <PlayerManager>().stamina <= 0)
                {
                    speed = m_WalkSpeed;
                }
            }

        }


         void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


         void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            // 만약 캐릭터가 이것 위에 있으면 리지드바디 못 움직이게 하는 설정
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }
    }
}
