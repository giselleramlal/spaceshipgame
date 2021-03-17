using Godot;
using System;
using System.Collections.Generic;

public class PillarOfAutumnDefenceController : AbstractDefenceController
{
    PillarOfAutumnSensorsController SensorsController { get { return parentShip.SensorsController as PillarOfAutumnSensorsController; } }
    PillarOfAutumnNavigationController NavigationController { get { return parentShip.NavigationController as PillarOfAutumnNavigationController; } }
    PillarOfAutumnPropulsionController PropulsionController { get { return parentShip.PropulsionController as PillarOfAutumnPropulsionController; } }

    //Queue of things we want to hit
    private List<Target> targetQueue = new List<Target>();

    private float maxSquaredRange = 1000000;

    private static float chainShotDelay = 0.1f; //Pass this as argument for deltaTime in ReadyChainShot method

    public PillarOfAutumnDefenceController() {
        // ReadyChainShot(new Target(new Vector2(600, 0), Vector2.Zero, 0, 0), chainShotDelay);
        // ReadyChainShot(new Target(new Vector2(800, 200), Vector2.Zero, 0, 2), chainShotDelay);
        // ReadyChainShot(new Target(new Vector2(800, -200), Vector2.Zero, 0, 4), chainShotDelay);
    }

    public override void DefenceUpdate(ShipStatusInfo shipStatusInfo, TurretControls turretControls, float deltaTime)
    {
        Vector2 shipPos = shipStatusInfo.positionWithinSystem;
        Vector2 shipVel = shipStatusInfo.linearVelocity;
        //Direction is a relative unit vector
        Vector2 direction = shipStatusInfo.forwardVector;

        //Periodically firing forwards for testing purposes
        
        FireForwardsPeriodicallyChain(turretControls, shipPos + shipVel);
        /*
        //Dirty lambda because .NET has no priority queue
        //Sort objects by distance to ship, and by time until we should fire
        targetQueue.Sort((a, b) => {
            if (b.waitTime != a.waitTime) {
                return (b.waitTime < a.waitTime) ? 1: -1;
            } else {
                /*Distance check
                float d1 = shipPos.DistanceSquaredTo(a.position);
                float d2 = shipPos.DistanceSquaredTo(b.position);
                return (d1 - d2) >= 0 ? -1: 1;
                
                //Sort by most imminent close threat
                float d1 = GetClosestDistance(shipPos, shipVel, a.position, a.velocity);
                float d2 = GetClosestDistance(shipPos, shipVel, b.position, b.velocity);
                float t1 = GetAnticipatedClosestTime(shipPos, shipVel, a.position, a.velocity);
                float t2 = GetAnticipatedClosestTime(shipPos, shipVel, b.position, b.velocity);

                //Are both things plausibly threatening?
                if ((d1 <= a.size * 3)  == (d2 <= b.size * 3)) {
                    //Are both actually going to hit
                    if (t1 > 0 && t2 > 0) {
                        //Which one comes closest first
                        return (t1 - t2) >= 0 ? -1: 1;
                    } else if (t1 > 0 && t2 < 0) {
                        return -1;
                    } else if (t1 < 0 && t2 > 0) {
                        return 1;
                    }
                    return 0;
                //Only the 2nd target is threatening
                } else if (d2 <= b.size * 4) {
                    if (t2 > 0) {
                        return 1;
                    } else if (t1 > 0) {
                        return -1;
                    }
                    return 0;
                //Only the 1st target is threatening
                } else if (d1 <= b.size * 4) {
                    if (t1 > 0) {
                        return -1;
                    } else if (t1 > 0) {
                        return 1;
                    }
                    return 0;
                }
                return (d1 - d2) >= 0 ? -1: 1;
            }
        });
        */

        //Look through the queue of things to hit, and fire ready torpedos at them
        int torpedosToShoot = Math.Min(GetReadyTubes(turretControls), targetQueue.Count);
        if (targetQueue.Count > 0) { 
            for (int i = 0; i < torpedosToShoot; i++) {
                Target next = targetQueue[0];
                if (next.waitTime <= 0) {
                    Vector2 nextTarget = next.position;
                    if (shipPos.DistanceSquaredTo(next.position) < maxSquaredRange) {
                        targetQueue.RemoveAt(0);
                        float distance = shipPos.DistanceTo(nextTarget);
                        //Fuse is timed to catch the asteroid away from center of explosion
                        float fuseTimer = 1000000f;
                        FireOneShot(turretControls, GetAimVector(shipPos, nextTarget, next.velocity, Torpedo.LaunchSpeed), fuseTimer);
                    }
                }
            }
        }

        UpdateTargetWaitTimes(deltaTime);
        
        
        //Sort queue


        /* Old fire forward code
        Vector2 shipPos = shipStatusInfo.positionWithinSystem;
        Vector2 direction = shipStatusInfo.forwardVector;
        FireAvailable(turretControls, shipPos + direction, 1000);
        */
    }

    private Target DataToTarget(EMSReading data) {
        float dist = SensorsController.getCurrentActiveSensors().GConstant * data.Amplitude;
        return new Target(
            new Vector2(Mathf.Cos(data.Angle) * dist, Mathf.Sin(data.Angle) * dist),
            data.Velocity,
            data.Radius,
            0
        );
    }

    public override void DebugDraw(Font font)
    {
        //Student code goes here
    }

    /// Temporary method to help simplify firing,
    /// Given desired fuse and target position, it will cause a single tube to shoot a torpedo, if any are ready
    private void FireOneShot(TurretControls turret, Vector2 position, float fuse) {
        turret.aimTo = position;
        for (int i = 0; i < 4; i++) {
            if (turret.GetTubeCooldown(i) == 0) {
                turret.TriggerTube(i, fuse);
                return;
            }
        }
    }

    private void FireForwardsPeriodically(TurretControls turret, Vector2 forwards) {
        if (GetReadyTubes(turret) == 4) {
            FireOneShot(turret, forwards, 1000000);
        }
    }

    private void FireForwardsPeriodicallyChain(TurretControls turret, Vector2 forwards) {
        if (GetReadyTubes(turret) == 4) {
            ReadyChainShot(forwards, chainShotDelay);
            ReadyChainShot(NavigationController.getDestination(), chainShotDelay);
        }
    }

    /// Returns the number of tubes that are ready to fire
    private int GetReadyTubes(TurretControls turret) {
        int ready = 0;
        for (int i = 0; i < 4; i++) {
            if (turret.GetTubeCooldown(i) == 0) {
                ready++;
            }
        } 
        return ready;
    }

    protected class Target {
        public Vector2 position;
        public Vector2 velocity;
        public float size;
        public float waitTime;

        public Target(Vector2 pos, Vector2 vel, float size, float time) {
            this.position = pos;
            this.velocity = vel;
            this.size = size;
            this.waitTime = time;
        }

        public Target(Target t) {
            this.position = t.position;
            this.velocity = t.velocity;
            this.size = t.size;
            this.waitTime = t.waitTime;
        }
    }
    
    private void ReadyChainShot(Vector2 position, float deltaTime, int shotsFired=2) {
        for (int i = 0; i < shotsFired; i++) {
            targetQueue.Add(new Target(position, Vector2.Zero, 0, deltaTime * i));
        }
    }

    private void ReadyChainShot(Target target, float deltaTime, int shotsFired=2) {
        for (int i = 0; i < shotsFired; i++) {
            targetQueue.Add(new Target(target));
            target.waitTime += deltaTime;
        }
    }

    private void ScatterShot() {
        
    }

    private void UpdateTargetWaitTimes(float deltaTime) {
        foreach(Target target in targetQueue) {
            target.waitTime -= deltaTime;
        }
    }

    private bool Collision(ShipStatusInfo shipInfo, Vector2 asteroidPos, float asteroidRadius) {
        Vector2 shipPos = shipInfo.positionWithinSystem;
        float shipRadius = shipInfo.shipCollisionRadius;

        float distance = shipPos.DistanceTo(asteroidPos);

        return distance <= (shipRadius + asteroidRadius);
    }

    private float GetAnticipatedClosestTime(Vector2 position, Vector2 shipVelocity, Vector2 targetPosition, Vector2 targetVelocity) {
        if (((targetVelocity.x - shipVelocity.x)*(targetVelocity.x - shipVelocity.x) + (targetVelocity.y - shipVelocity.y) * (targetVelocity.y - shipVelocity.y)) == 0) {
            return -1;
        } else {
            return -((targetPosition.x - position.x)*(targetVelocity.x - shipVelocity.x) + (targetPosition.y - position.y)*(targetVelocity.y - shipVelocity.y))/((targetVelocity.x - shipVelocity.x)*(targetVelocity.x - shipVelocity.x) + (targetVelocity.y - shipVelocity.y) * (targetVelocity.y - shipVelocity.y));
        }
    }

    private float GetClosestDistance(Vector2 position, Vector2 shipVelocity, Vector2 targetPosition, Vector2 targetVelocity) {
        float t = GetAnticipatedClosestTime(position, shipVelocity, targetPosition, targetVelocity);
        if (t > 0) {
            float dx = targetPosition.x + targetVelocity.x * t - position.x - shipVelocity.x * t;
            float dy = targetPosition.y + targetVelocity.y * t - position.y - shipVelocity.y * t;
            return Mathf.Sqrt((dx * dx) + (dy * dy));
        } else {
            float dx = targetPosition.x - position.x;
            float dy = targetPosition.y - position.y;
            return Mathf.Sqrt((dx * dx) + (dy * dy));
        }
    }

    ///RETURNS Vector2.Zero if no possible hit
    private Vector2 GetAimVector(Vector2 shipPosition, Vector2 targetPosition, Vector2 targetVelocity, float rocketSpeed) {
        if (targetVelocity.DistanceSquaredTo(Vector2.Zero) < 100) {
            return targetPosition;
        }
        Vector2 targetRel = targetPosition - shipPosition;
        float a = targetVelocity.x * targetVelocity.x + targetVelocity.y * targetVelocity.y;
        float b = 2 * (targetVelocity.x * targetRel.x + targetVelocity.y * targetRel.y);
        float c = targetRel.x * targetRel.x + targetRel.y * targetRel.y;
        float det = b * b - 4 * a * c;
        if (det > 0) {
            float t1 = (-b + Mathf.Sqrt(det))/(2 * a);
            float t2 = (-b - Mathf.Sqrt(det))/(2 * a);
            float t = 0;
            if (t1 > 0) {
                if (t1 < t2) {
                    t = t1;
                } else if (t2 > 0) {
                    t = t2;
                }
            } else if (t2 > 0) {
                t = t2;
            } else {
                return Vector2.Zero;
            }
            float vx = (targetPosition.x + targetVelocity.x * t)/(t * rocketSpeed);
            float vy = (targetPosition.y + targetVelocity.y * t)/(t * rocketSpeed);
            return new Vector2(vx * rocketSpeed * t, vy * rocketSpeed * t);

        } else {
            return Vector2.Zero;
        }
    }
}
