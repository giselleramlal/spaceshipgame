using Godot;
using System;
using System.Collections.Generic;

public class PillarOfAutumnNavigationController : AbstractNavigationController
{
	PillarOfAutumnSensorsController SensorsController { get { return parentShip.SensorsController as PillarOfAutumnSensorsController; } }
	PillarOfAutumnPropulsionController PropulsionController { get { return parentShip.PropulsionController as PillarOfAutumnPropulsionController; } }
	PillarOfAutumnDefenceController DefenceController { get { return parentShip.DefenceController as PillarOfAutumnDefenceController; } }

	private Vector2 curVect;
	private Vector2 nextGate;
	private Dictionary<String, String> path;
	private String nextSystem;
	private int landingInstruction;
	private String previousSystem = "";
	private float scanTime = 0.5f;
	private bool scanned = false;
	private float currentTime = 0;
	public override void NavigationUpdate(ShipStatusInfo shipStatusInfo, GalaxyMapData galaxyMapData, float deltaTime)
	{
	  //GD.Print(shipStatusInfo.currentSystemName);
	   //Student code goes here
	  	if (currentTime <= 0.05){
			path = new Dictionary<string, string>();
			previousSystem = "";
			Dictionary<string, bool> visited = new Dictionary<string, bool>();
			Dictionary<string, double> costs = new Dictionary<string, double>();
			Dictionary<string, string> last = new Dictionary<string, string>();
			foreach (GalaxyMapNodeData d in galaxyMapData.nodeData){
				visited[d.systemName] = false;
				costs[d.systemName] = Double.MaxValue;
			}
			costs[shipStatusInfo.currentSystemName] = 0;
			//since n is small, o(n^2) is basically 0
			for (int i = 0; i < galaxyMapData.nodeData.Length; ++i){
				string curNode = "";
				double minCost = Double.MaxValue;
				foreach (GalaxyMapNodeData d in galaxyMapData.nodeData){
					if (!visited[d.systemName]){
						if (costs[d.systemName] < minCost){
							minCost = costs[d.systemName];
							curNode = d.systemName;
						}
					}
				}
				visited[curNode] = true;
				//dig through the list of edges to figure out which ones to relax
				//yes, this is inefficient
				foreach (GalaxyMapEdgeData d in galaxyMapData.edgeData){
					if (d.nodeA.systemName == curNode){
						if(costs[d.nodeB.systemName] > costs[curNode] + d.edgeCost){
							costs[d.nodeB.systemName] = costs[curNode] + d.edgeCost;
							last[d.nodeB.systemName] = curNode;
						}
					}
				}
			}
			string curFind = SolarSystemNames.Kepler438;
			while(curFind != shipStatusInfo.currentSystemName){
				path[last[curFind]] = curFind;
				curFind = last[curFind];
				//GD.Print(curFind);
			}
		}
		if (previousSystem != shipStatusInfo.currentSystemName){
			previousSystem = shipStatusInfo.currentSystemName;
			scanTime = currentTime + 0.5f;
			scanned = false;
		}


		//GD.Print(scanned, currentTime, scanTime);

		if (!scanned && currentTime > scanTime){
			if (shipStatusInfo.currentSystemName != "Kepler 438 System"){
				nextSystem = path[shipStatusInfo.currentSystemName];
				landingInstruction = 0;
				IPassiveSensors passiveSensors = SensorsController.getCurrentPassiveSensors();
				IActiveSensors activeSensors = SensorsController.getCurrentActiveSensors();
				foreach (PassiveSensorReading g in passiveSensors.PassiveReadings){
					if (g.Signature == GravitySignature.WarpGate){
						List<EMSReading> aSensorData = activeSensors.PerformScan(g.Heading, 0.01f, 2000);
						for (int i = 0; i < aSensorData.Count; ++i){
							if (aSensorData[i].SpecialInfo == nextSystem){
								nextGate[0] = aSensorData[i].Amplitude * activeSensors.GConstant * Convert.ToSingle(Math.Cos(aSensorData[i].Angle)) + shipStatusInfo.positionWithinSystem[0];
								nextGate[1] = aSensorData[i].Amplitude * activeSensors.GConstant * Convert.ToSingle(Math.Sin(aSensorData[i].Angle)) + shipStatusInfo.positionWithinSystem[1];
								//GD.Print(nextGate[0]);
								//GD.Print(nextGate[1]);
								goto outOfLoop1;
							}
						}
					}
				} 
outOfLoop1: //c# doesn't have labeled break statements, so this is gonna have to do
				scanned = true;
			}
			else{
				landingInstruction = 1;
				IPassiveSensors passiveSensors = SensorsController.getCurrentPassiveSensors();
				IActiveSensors activeSensors = SensorsController.getCurrentActiveSensors();
				foreach (PassiveSensorReading g in passiveSensors.PassiveReadings){
					if (g.Signature == GravitySignature.Planetoid){
						List<EMSReading> aSensorData = activeSensors.PerformScan(g.Heading, 0.01f, 2000);
						for (int i = 0; i < aSensorData.Count; ++i){
							if (aSensorData[i].ScanSignature == "Common:70|Metal:20|Water:10"){
								nextGate[0] = aSensorData[i].Amplitude * activeSensors.GConstant * Convert.ToSingle(Math.Cos(aSensorData[i].Angle)) + shipStatusInfo.positionWithinSystem[0];
								nextGate[1] = aSensorData[i].Amplitude * activeSensors.GConstant * Convert.ToSingle(Math.Sin(aSensorData[i].Angle)) + shipStatusInfo.positionWithinSystem[1];
								//GD.Print(nextGate[0]);
								//GD.Print(nextGate[1]);
								goto outOfLoop2;
								// GD.Print(aSensorData[i].ScanSignature);
							}
						}
					}
				} 
outOfLoop2: //c# doesn't have labeled break statements, so this is gonna have to do
				scanned = true;
			}
		}

		currentTime += deltaTime;
	}

	// public Vector2 giveDestination(){
	// 	return nextGate;
	// }

	public override void DebugDraw(Font font)
	{
	   //Student code goes here
	}

	public Vector2 getDestination(){
		return nextGate;
	}

	public int getLandingInfo(){
		//0 = warp
		//1 = land
		return landingInstruction;
	}

}
