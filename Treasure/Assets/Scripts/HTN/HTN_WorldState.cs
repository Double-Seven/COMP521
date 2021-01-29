using System;


// Author: ZiQi Li
// A class represents the World State of the game (will be used for HTN tree)
public class HTN_WorldState
{

    // elements in this world state
    public bool isInAttackRange { set; get; }  // represent isPLAYER_IN_ATTACK_RANGE of the monster
    public bool isInYellingRange { set; get; }  // represent isInYellingRange of the monster
    //public bool hasObstablesOnMap { set; get; }  // represent hasOBSTACLES_ON_MAP for monster to throw
    public bool hasCratesInRange { set; get; }  // represent hasCratesInRange for monster to throw (range is the attack range)
    public bool isInFrontOfObstacle { set; get; }  // represent whether the monster is in front of the chosen obstacle to throw
    public bool isTheChosenObstacleDestroyed { set; get; }  // represent whether the crate we choose to throw is destroyed. (Monster may choose an obstacle on the air before it's destroyed)


    public HTN_WorldState(bool isInAttackRange, bool isInYellingRange, bool hasCratesInRange, bool isInFrontOfObstacle, bool isTheChosenObstacleDestroyed)
    {
        this.isInAttackRange = isInAttackRange;
        this.isInYellingRange = isInYellingRange;
        this.hasCratesInRange = hasCratesInRange;
        this.isInFrontOfObstacle = isInFrontOfObstacle;
        this.isTheChosenObstacleDestroyed = isTheChosenObstacleDestroyed;
    }


    public HTN_WorldState clone()
    {
        return new HTN_WorldState(this.isInAttackRange, this.isInYellingRange, this.hasCratesInRange, this.isInFrontOfObstacle, this.isTheChosenObstacleDestroyed);
    }
}
