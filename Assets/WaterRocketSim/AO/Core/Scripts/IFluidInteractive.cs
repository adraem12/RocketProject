using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AerodynamicObjects
{
    /// <summary>
    /// Defines the interaction between an aerodynamic object and a fluid zone. This will include particles and wings.
    /// </summary>
    public interface IFluidInteractive
    {
        /// <summary>
        /// The Fluid Zones that are currently affecting this object.
        /// </summary>
        List<FluidZone> FluidZones
        {
            get;
            set;
        }

        /// <summary>
        /// Add this fluid zone to the object's list of fluid zones.
        /// The object will then be affected by this fluid zone.
        /// </summary>
        public void SubscribeToFluidZone(FluidZone fluidZone)
        {
            FluidZones.Add(fluidZone);
        }

        /// <summary>
        /// Remove this fluid zone from the object's list of fluid zones.
        /// The object will no longer be affected by this fluid zone.
        /// </summary>
        public void UnsubscribeFromFluidZone(FluidZone fluidZone)
        {
            FluidZones.Remove(fluidZone);
        }
    }
}