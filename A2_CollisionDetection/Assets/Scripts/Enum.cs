using System;

//Author: ZiQi Li, for Comp521 A2, McGill University
namespace SomeEnum
{
    //define a enum type for direction of muzzle (to right/to left)
    public enum MuzzleDirection
    {
        LEFT,
        RIGHT
    }

    //define a enum type for collision handling (terrain, water and ballons)
    public enum ColliderType
    {
        TERRAIN,
        WATER,
        BALLOON
    }
}

