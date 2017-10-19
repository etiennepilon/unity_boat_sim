﻿/*
 * This class computes very rougly the physics of the boat
 * Assumptions:
 *  - The angular drag is calculated with the same formula as the linear one
 *  - The paddle as a constant force that is applied
 *  - The moment of inertia of the boat is estimated to a cylinder spinning at its center (for now)
 *  - Assume water speed is 0 m/s for drag calculation
 */
using UnityEngine;
using System;

public class boatController{
	// --
	// Constant related to the geometry and forces of the boat
	private const double c_mass = 100 /*kg*/, 
	c_boat_width = 1.0 /*m*/,
	c_boat_length = 3.0 /*m*/,
	c_boat_height = 0.8 /*m*/,
	c_percentage_of_immersion_in_water = 0.5,
	c_front_drag_coeff = 0.3,
	c_side_drag_coeff = 4,
	c_water_mass_density = 1000 /* kg/m3 */;
	public double c_forward_paddle_force = 120 /* N */;


	private double time_delta /* seconds */, c_boat_front_area, c_boat_side_area, inertial_moment, side_drag_distance; 
	public double absolute_linear_speed { get; set;} /* m/s */
	public double angular_speed { get; set;} /* rad/s */
	public double heading {get; set;}
	public float p_x {get; set;} 
	public float p_y {get; set;}
	// --
	// Initialize with default values - possible to overload
	public boatController(double delta_time)
	{
		absolute_linear_speed = 0;
		angular_speed = 0;
		time_delta = delta_time;
		// -- Considered a perfect triangle for now.
		c_boat_front_area = (c_percentage_of_immersion_in_water * c_boat_width) * (c_percentage_of_immersion_in_water * c_boat_height) / 2;
		// -- Considered a rectangle for now
		c_boat_side_area = (c_percentage_of_immersion_in_water * c_boat_length) * (c_boat_height);
		inertial_moment = c_mass * (c_boat_length * c_boat_length) / 6;
		side_drag_distance = c_boat_length / 2 * 0.9;
		heading = 0; p_x = 0; p_y = 0;
	}
	/*
	 This section calculates the drag of the water (rough estimate) and the linear speed.
	*/
	private double frontDrag()
	{
		return c_front_drag_coeff * c_water_mass_density * c_boat_front_area * (absolute_linear_speed * absolute_linear_speed)/2;
	}
	public void updateLinearVelocityWithForce(double external_force)
	{
		double linear_acc = (external_force - frontDrag ()) / c_mass;
		absolute_linear_speed = absolute_linear_speed + linear_acc * time_delta;
		if (absolute_linear_speed < 0) {
			absolute_linear_speed = 0;
		}
	}
	/*
	 This section calculates the drag of the water (rough estimate) and the angular speed.
	*/
	private double sideDrag()
	{
		// --
		// Assumption: We can estimate the linear speed with the angular speed because for small time intervals, the delta is almost the same.
		return c_side_drag_coeff * c_water_mass_density * c_boat_side_area * (angular_speed * angular_speed)/2;
	}
	// --
	// Parameters
	// - external_force: Force applied by the paddle
	// - distance_from_center_of_mass: Distance of the paddle from the boat
	// --
	// Assumptions
	// - The force is applied directly perpedicularly to the center of mass (the avatar is seated in the middle of the boat)
	// - The drag on the boat is applied at the constant distance from the center of mass
	// - Paddling to the left is a negative distance from the center of mass (thus, the drag becomes positive)
	public void updateAngularVelocityWithForceAndDistance(double external_force, double distance_from_center_of_mass)
	{
		double angular_acc = 0;
		if (angular_speed >= 0) {
			angular_acc = (external_force * distance_from_center_of_mass - sideDrag () * side_drag_distance) / inertial_moment;
		} else {
			angular_acc = (external_force * distance_from_center_of_mass + sideDrag () * side_drag_distance) / inertial_moment;
		}
		angular_speed = angular_speed + angular_acc * time_delta;
	}

	public void updateHeadingAndPosition(){
		heading += angular_speed * time_delta;
		p_x += (float)((absolute_linear_speed * time_delta)* Math.Cos (heading));
		p_y += (float)((absolute_linear_speed * time_delta) * Math.Sin (heading));
	}
}