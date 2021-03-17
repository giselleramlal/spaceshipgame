using Godot;
using System;

public class PillarOfAutumnPropulsionController : AbstractPropulsionController
{
    PillarOfAutumnSensorsController SensorsController { get { return parentShip.SensorsController as PillarOfAutumnSensorsController; } }
    PillarOfAutumnNavigationController NavigationController { get { return parentShip.NavigationController as PillarOfAutumnNavigationController; } }
    PillarOfAutumnDefenceController DefenceController { get { return parentShip.DefenceController as PillarOfAutumnDefenceController; } }

    private float error = 0;
    private float error2 = 0;
    private float errorx = 0;
    private float errory = 0;
    public override void PropulsionUpdate(ShipStatusInfo shipStatusInfo, ThrusterControls thrusterControls, float deltaTime)
    {
        //Student code goes here
        Vector2 shipPosition = shipStatusInfo.positionWithinSystem;
        Vector2 shipVelocity = shipStatusInfo.linearVelocity;

        Vector2 destination = NavigationController.getDestination();
        int destinationStatus = NavigationController.getLandingInfo();

        Vector2 displacement = new Vector2(destination.x - shipPosition.x, destination.y - shipPosition.y);
        Vector2 direction = shipStatusInfo.forwardVector;

        if (destinationStatus == 0 && displacement.Length() < 100) {
            thrusterControls.TriggerWarpJump();
        } else if (displacement.Length() < 1000) {
            thrusterControls.TriggerLandingSequence();
        }

        if (displacement.Length() > 1000) {
            move(shipVelocity, shipPosition, displacement, direction, thrusterControls, deltaTime, destination);
        } else {
            land(shipPosition, displacement, direction, thrusterControls, deltaTime, destination);
        }
        
    }
    public void move(Vector2 shipVelocity, Vector2 shipPosition, Vector2 displacement, Vector2 direction, ThrusterControls thrusterControls, float deltaTime,Vector2 destination) {
        //counter productive velocity
        Vector2 cvelocity = shipVelocity-shipVelocity.Project(displacement);
        float shipAngle = 0;
        if(cvelocity.Length() > 10){
            shipAngle = direction.AngleTo(-cvelocity);
        }else{
            shipAngle = direction.AngleTo(displacement);
        }
        float derivative = (shipAngle-error)/deltaTime;
        error = shipAngle;
        float thrust = 1F*derivative+1F*error;
        // everything2 refers to translation
        float derivative2 = (displacement.Length()-error2)/deltaTime;
        error2 = displacement.Length();
        float thrust2 = 0;
        if(Math.Abs(shipAngle) > 0.1){
            thrust2 = 0;
        }else if(cvelocity.Length() > 10){
            thrust2 = 1;
        }else{
            thrust2 = 0.3F*derivative2+ 0.1F*error2;
        }
        if(thrust2> 0){
            thrusterControls.StarboardRetroThrottle = 0;
            thrusterControls.PortRetroThrottle = 0;
            thrusterControls.MainThrottle = thrust2;
        }else if(thrust2< 0){
            thrusterControls.StarboardRetroThrottle = -thrust2;
            thrusterControls.PortRetroThrottle = -thrust2;
            thrusterControls.MainThrottle = 0;
        }else{
            thrusterControls.StarboardRetroThrottle = 0;
            thrusterControls.PortRetroThrottle = 0;
            thrusterControls.MainThrottle = 0;
        }
        if(shipAngle < 0){
            thrusterControls.StarboardForeThrottle = -thrust;
            thrusterControls.PortAftThrottle = -thrust;
            thrusterControls.PortForeThrottle = 0;
            thrusterControls.StarboardAftThrottle = 0;
        }else if(shipAngle > 0){
            thrusterControls.StarboardForeThrottle = 0;
            thrusterControls.PortAftThrottle = 0;
            thrusterControls.PortForeThrottle = thrust;
            thrusterControls.StarboardAftThrottle = thrust;
        }
    }

    public void land(Vector2 shipPosition, Vector2 displacement, Vector2 direction, ThrusterControls thrusterControls, float deltaTime, Vector2 destination) {
        float shipAngle = direction.AngleTo(new Vector2(0, -1));
        float derivative = (shipAngle-error)/deltaTime;
        error = shipAngle;
        float thrust = derivative+error;

        if(shipAngle < -0.1){
            thrusterControls.MainThrottle = 0;
            thrusterControls.PortRetroThrottle = 0;
            thrusterControls.StarboardRetroThrottle = 0;
            thrusterControls.StarboardForeThrottle = -thrust;
            thrusterControls.PortForeThrottle = 0;
            thrusterControls.PortAftThrottle = -thrust;
            thrusterControls.StarboardAftThrottle = 0;
        }else if(shipAngle > 0.1){
            thrusterControls.MainThrottle = 0;
            thrusterControls.PortRetroThrottle = 0;
            thrusterControls.StarboardRetroThrottle = 0;
            thrusterControls.StarboardForeThrottle = 0;
            thrusterControls.PortForeThrottle = thrust;
            thrusterControls.PortAftThrottle = 0;
            thrusterControls.StarboardAftThrottle = thrust;
        }else{
            thrusterControls.StarboardForeThrottle = 0;
            thrusterControls.PortForeThrottle = 0;
            thrusterControls.PortAftThrottle = 0;
            thrusterControls.StarboardAftThrottle = 0;
            float derivativex = (displacement.x-errorx)/deltaTime;
            errorx = displacement.x;
            float thrustx = 2.5F*derivativex+1F*errorx;

            if(thrustx > 0){
                thrusterControls.PortAftThrottle += thrustx;
                thrusterControls.PortForeThrottle += thrustx;
            }else if(thrustx < 0){
                thrusterControls.StarboardAftThrottle += -thrustx;
                thrusterControls.StarboardForeThrottle += -thrustx;
            }

            float derivativey = (displacement.y-errory)/deltaTime;
            errory = displacement.y;
            float thrusty = 1.25F*derivativey+0.5F*errory;
    
            if(thrusty < 0){
                thrusterControls.MainThrottle = -thrusty;
                thrusterControls.PortRetroThrottle = 0;
                thrusterControls.StarboardRetroThrottle = 0;
            }  else if(thrusty > 0){
                thrusterControls.MainThrottle = 0;
                thrusterControls.PortRetroThrottle = thrusty;
                thrusterControls.StarboardRetroThrottle = thrusty;

            }
        }


        

    }

    public override void DebugDraw(Font font)
    {
        //Student code goes here
    }
}
