//namespace Pong.Ball;
<<<<<<< HEAD
=======
/// <summary>
/// PongBallController Class: Manages the behavior, movement, and interactions of the pong ball in the game.
/// - MoveLocal(): Handles the core movement logic, including:
///    1. Scoring: Triggers OnScore() when hitting left/right boundaries.
///    2. Rebounding: Uses Rebounder's forceMap to change trajectory when colliding with a player's stick.
///    3. Vertical Wall Collisions: Reverses y-axis velocity when hitting top/bottom walls.
/// </summary>


>>>>>>> f106512 (Added summary via comments on headers & other important files)
using Pong.Ball;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Pong;
using Pong.Physics;
using Pong.GamePlayer.Force;

using static Pong.GameCache;
using static Pong.GameHelpers;

namespace Pong.Ball {
    public partial class PongBallController : MonoBehaviour
    {
        // parameters of trajectory
        // contains base viewport velocity (Vector2) and float[] yAccelerationAndBeyond in terms of viewport percentage y
        private readonly Motion2D viewportMotion = new Motion2D();

        // trajectory facilitation
        private float elapsedTrajectoryTime = 0.0f;
        private bool hasTrajectory = false;

        // goal detection
        private Action OnScore, OnRebound;

        // collision detection helper
        private RectangularBodyFrame bodyFrame;
        private Rebounder rebounder;

        // initialization check
        private bool isInitialized = false;

        public void Initialize(Action scoreProcedure, Action reboundProcedure) {
            OnScore = scoreProcedure;
            OnRebound = reboundProcedure;

            isInitialized = true;
        }

        void Awake()
        {
            // cancel motion: no more velocity and further derivatives! all 0
            viewportMotion.ZeroOut();
        }

        // Start is called before the first frame update
        void Start()
        {
            bodyFrame = GetComponent<RectangularBodyFrame>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isInitialized) {
                return;
            }

            if (hasTrajectory) {
                // calculate current velocity
                Vector2 currentTotalViewportVelocity = viewportMotion.CalculateTotalVelocity(elapsedTrajectoryTime);

                // motion
                MoveLocal(ToLocal(currentTotalViewportVelocity));

                elapsedTrajectoryTime += Time.deltaTime;
            }
        }

        //* Deals with Movement, and Collision + Interactions as a Result of that movement
        public void MoveLocal(Vector3 localDelta_dt) {
            // origin is in the center
            Vector3 MAX_POS = BG_TRANSFORM.localScale / 2f;
            Vector3 MIN_POS = -MAX_POS;

            // local velocity * delta time = local delta pos ("delta y")
            Vector3 actualLocalDelta = localDelta_dt * Time.deltaTime;
            
            //* Scoring Collisions
            if (bodyFrame.leftEdgeX + actualLocalDelta.x <= MIN_POS.x) { //* Left Boundary
                actualLocalDelta.x = MIN_POS.x - bodyFrame.leftEdgeX;

                //Debug.Log("BALL: bodyFrame.leftEdgeX = " + bodyFrame.leftEdgeX + ", actualLocalDelta.x = " + actualLocalDelta.x);

                //* Score Point
                OnScore();

                // Integrate Changes
                transform.localPosition += actualLocalDelta;
                return;
            } else if (bodyFrame.rightEdgeX + actualLocalDelta.x >= MAX_POS.x) { //* Right Boundary
                actualLocalDelta.x = MAX_POS.x - bodyFrame.rightEdgeX;

                //Debug.Log("BALL: bodyFrame.rightEdgeX = " + bodyFrame.rightEdgeX + ", actualLocalDelta.x = " + actualLocalDelta.x);
                
                //* Score Point
                OnScore();

                // Integrate Changes
                transform.localPosition += actualLocalDelta;
                return;
            }

            //* Rebounding Player
            if (bodyFrame.CollidesWith(rebounder.bodyFrame)) {
                Vector2 collisionPoint = bodyFrame.CollisionPoint(rebounder.bodyFrame);

                // modify trajectory
                bool abnormal = rebounder.forceMap.ApplyAt(collisionPoint, viewportMotion);

                if (abnormal) {
                    elapsedTrajectoryTime = 0f;
                }

                //TODO: play paddle hit sound

                // Rebound
                OnRebound();
                return;
            }
            
            //* Vertical Wall Collisions            
            if (bodyFrame.topEdgeY + actualLocalDelta.y >= MAX_POS.y) { //* Top Wall
                actualLocalDelta.y = MAX_POS.y - bodyFrame.topEdgeY;

                //* Collide with the top wall => reverse y trajectory
                viewportMotion.velocity.y *= -1;
                //TODO: play wall sound
            } else if (bodyFrame.bottomEdgeY + actualLocalDelta.y <= MIN_POS.y) { //* Bottom Wall
                actualLocalDelta.y = MIN_POS.y - bodyFrame.bottomEdgeY;
                
                //* Collide with the bottom wall => reverse y trajectory
                viewportMotion.velocity.y *= -1;
                //TODO: play wall sound
            }
            
            //* Integrate Changes
            transform.localPosition += actualLocalDelta;
        }

        public void BeginTrajectory() {
            hasTrajectory = true;
            elapsedTrajectoryTime = 0.0f;
        }

        public void HaltTrajectory() {
            hasTrajectory = false;
            elapsedTrajectoryTime = 0.0f;

            ZeroMotion();
        }

        private void ZeroMotion() {
            // reset ball state
            ResetBallState();

            // cancel motion: no more velocity and further derivatives! all 0
            viewportMotion.ZeroOut();
        }

        public void ResetBallState() {
            bodyFrame.ResetState();
        }

        public void SetFreeze(bool freeze) {
            hasTrajectory = !freeze;
        }

        // [(x, y), (x', y'), (x'', y''), ...]
        // in terms of viewport %
        public Vector2[] RetrieveBallTrajectory() {
            Vector2 vpLocalPos = ToViewport(transform.localPosition);

            return viewportMotion.RetrieveTrajectory(vpLocalPos);
        }

        //public int GetBallStatus() { return ballStatus; }

        public bool HasTrajectory() { return hasTrajectory; }
        public float GetElapsedTrajectoryTime() { return elapsedTrajectoryTime; }

        public Vector2 ViewportVelocity {
            get {
                return viewportMotion.velocity; 
            }
            set {
                viewportMotion.velocity.Set(value.x, value.y);
            }
        }

        public Rebounder Rebounder {
            get { return rebounder; }
            set { rebounder = value; }
        }
    }
}