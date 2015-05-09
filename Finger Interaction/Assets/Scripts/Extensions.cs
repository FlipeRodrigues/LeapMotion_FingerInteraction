/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * 
 * 																			 *
 * Project: Leap Motion Interaction											 *
 * File: Extensions.cs												 		 *	
 * Description: Defines some new functions in Unity's standard classes		 *
 * Author: Filipe Miguel Sobreira Rodrigues									 *													 
 * Revision Hystory: - 3/2015 -> first release  							 *                                                     
 * 																			 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using UnityEngine;
using System.Collections;

public static class Extensions
{
	// RIGIDBODY EXTENSIONS:

	// Checks if the RigidBody in question is moving or not (arbitrary definition)
	public static bool HasStopped (this Rigidbody rigidBody)
	{
		float linearVelocity = rigidBody.velocity.sqrMagnitude;
		float angularVelocity = rigidBody.angularVelocity.sqrMagnitude;

		return (linearVelocity + angularVelocity < 0.01f);
	}
}
