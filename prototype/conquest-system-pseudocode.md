// Conquest System Pseudocode
// For Unreal Engine 5 planning

enum class ERank
{
    Peasant,
    Adventurer,
    Knight,
    Lord,
    Baron,
    Duke,
    King,
    Emperor
};

enum class ELocationType
{
    Camp,
    Village,
    Town,
    Castle,
    Airship,
    NavalVessel,
    Capital
};

enum class EOwnershipType
{
    Neutral,
    PlayerOwned,
    KingdomOwned,
    EnemyOwned,
    FactionControlled
};

struct FConquestLocation
{
    FString Name;
    ELocationType LocationType;
    EOwnershipType Ownership;
    ERank RequiredRank;
    int DefenderCount;
    int InfluenceCost;
    bool bStoryProtected;
};

bool CanCaptureLocation(ERank PlayerRank, int PlayerInfluence, FConquestLocation Location)
{
    if (Location.bStoryProtected)
    {
        return false;
    }

    if (PlayerRank < Location.RequiredRank)
    {
        return false;
    }

    if (PlayerInfluence < Location.InfluenceCost)
    {
        return false;
    }

    if (Location.DefenderCount > 0)
    {
        return false;
    }

    return true;
}

void CaptureLocation(FConquestLocation& Location, bool bForPlayerKingdom)
{
    if (bForPlayerKingdom)
    {
        Location.Ownership = EOwnershipType::PlayerOwned;
    }
    else
    {
        Location.Ownership = EOwnershipType::KingdomOwned;
    }

    // Rewards
    AddGoldIncome(Location);
    UnlockRecruitment(Location);
    IncreasePlayerInfluence(Location);
    TriggerEnemyRetaliation(Location);
}

void TriggerEnemyRetaliation(FConquestLocation Location)
{
    // Enemy faction reacts to conquest
    // Example future systems:
    // - Send raid party
    // - Declare war
    // - Increase bounty
    // - Start rebellion event
}