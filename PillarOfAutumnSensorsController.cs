using Godot;
using System;
using System.Collections.Generic;

public class PillarOfAutumnSensorsController : AbstractSensorsController
{
    PillarOfAutumnNavigationController NavigationController { get { return parentShip.NavigationController as PillarOfAutumnNavigationController; } }
    PillarOfAutumnPropulsionController PropulsionController { get { return parentShip.PropulsionController as PillarOfAutumnPropulsionController; } }
    PillarOfAutumnDefenceController DefenceController { get { return parentShip.DefenceController as PillarOfAutumnDefenceController; } }

    IActiveSensors currentActiveSensors;
    PassiveSensors currentPassiveSensors;
    List<EMSReading> asteroidList = new List<EMSReading>();
    public static float GConstantNumber;
    

    public override void SensorsUpdate(ShipStatusInfo shipStatusInfo, IActiveSensors activeSensors, PassiveSensors passiveSensors, float deltaTime)
    {
        //Student code goes here
        GConstantNumber= activeSensors.GConstant;
        currentActiveSensors = activeSensors;
        currentPassiveSensors = passiveSensors;

        //List<EMSReading> aSensorData = activeSensors.PerformScan(0, Convert.ToSingle(2*Math.PI), 100000);
        //for(int i = 0; i < aSensorData.Count; ++i){
        //    if(aSensorData[i].ScanSignature == "Rock:90|Common:10"){
        //        asteroidList.Add(aSensorData[i]);
        //    }
        //}

    }

    public List<EMSReading> getCurrentAsteroidList() {
        return asteroidList;
    }

    public IActiveSensors getCurrentActiveSensors() {
        return currentActiveSensors;
    }

    public PassiveSensors getCurrentPassiveSensors() {
        return currentPassiveSensors;
    }

    public override void DebugDraw(Font font)
    {
        //Student code goes here
    }

    /*
    public struct SpatialBody{
        public String type; // warpgate, asteroid, planet
        public Vector2 coordinates;
        public SpatialBody (String type, Vector2 coordinates){
            this.coordinates=coordinates;
            this.coordinates=coordinates;
        }
    }

    public struct GalaxyAlphaData {
        public String solName;
        public List<SpatialBody> spatialBodyList;
        public GalaxyAlphaData (String solName, List<SpatialBody> spatialBodyList){
            this.solName=solName;
            this.spatialBodyList=spatialBodyList;
        }
    }

    public List<SpatialBody> solSystemList=new List<SpatialBody>(){alphaCentauri,solAsteroid1,solAsteroid2,solAsteroid3};
    
    public List<SpatialBody> alphaCentauriList=new List<SpatialBody>() {kepler438,kepAsteroids1,KepAsteroids2};

    public List<SpatialBody> kepler438List=new List<SpatialBody>(){planetKepler};
     
    // info for objects in different systems (alpha galaxy)
    public GalaxyAlphaData solSystem= new GalaxyAlphaData("Sol System",solSystemList);
    public GalaxyAlphaData solSystem= new GalaxyAlphaData("Alpha Centauri System",alphaCentauriList );
    public GalaxyAlphaData solSystem= new GalaxyAlphaData("Kepler 438 System",kepler438List );

    

    SpatialBody alphaCentauri = new SpatialBody ("Warp gate", new Vector2(800,0));
    SpatialBody solAsteroid1 = new SpatialBody ("Asteroid", new Vector2(800,200));
    SpatialBody solAsteroid2 = new SpatialBody ("Asteroid", new Vector2(800,-200));
    SpatialBody solAsteroid3 = new SpatialBody ("Asteroid", new Vector2(600,0));

    SpatialBody kepler438 = new SpatialBody ("Warp gate", new Vector2(312.978,386.366));
    SpatialBody kepAsteroids1 = new SpatialBody ("Asteroid", new Vector2(-395,-271.967));
    SpatialBody kepAsteroids2 = new SpatialBody ("Asteroid", new Vector2(153.251,166.202));

    SpatialBody planetKepler = new SpatialBody ("Planet", new Vector2(1276.38,107.665));

    */
}
