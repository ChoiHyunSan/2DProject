namespace APIServer;

/// <summary>
/// 에러 코드를 정리하는 Enum 
/// (에러 코드) + (Status_Code) 형식으로 이뤄져있다.
/// 
/// </summary>
public enum ErrorCode
{
    None = 0_200,

    // Db (공통)
    FailedDataLoad = 100_500,

    // Middleware 500 ~ 999
    FailedParseAuthorizeInfo = 500_400,
    FailedAuthorizeTokenVerify = 501_401,

    // MasterDb 1000 ~ 1999
    FailedGetMasterData = 1000_500,

    // GameDb 2000 ~ 2999
    FailedInsertData               = 2000_500,
    FailedRollbackDefaultData      = 2001_500,
    FailedLoadAllGameData          = 2002_500,
    FailedInsertCharacterItem      = 2003_500,
    FailedInsertCharacterRune      = 2004_500, 
    FailedCreateUserData           = 2005_500,
    FailedLoadUserData             = 2006_500,
    FailedInsertNewCharacter       = 2007_500,
    FailedUpdateGoldAndGem         = 2008_500,
    FailedPurchaseCharacter        = 2009_500,
    FailedDeleteInventoryItem      = 2010_500,
    FailedGetUserGoldAndGem        = 2011_500,
    FailedUpdateUserGoldAndGem     = 2012_500,
    FailedUpdateItemLevel          = 2013_500,
    FailedUpdateRuneLevel          = 2014_500,
    CannotFindUserCurrency         = 2015_404,
    FailedUpdateData               = 2016_500,
    
    // AccountDb 3000 ~ 3999
    FailedCreateAccountUserData    = 3000_500,
    FailedGetAccountUserData       = 3001_500,
    FailedCreateAccount            = 3002_500,

    // MemoryDb 4000 ~ 4999
    FailedRegisterSession          = 4000_500,
    FailedGetSession               = 4001_500,
    SessionNotFound                = 4002_404,
    FailedSessionLock              = 4003_500,
    FailedSessionUnLock            = 4004_500,
    AlreadySessionLock             = 4005_409,
    SessionLockNotFound            = 4006_404,
    FailedCacheGameData            = 4007_500,
    FailedGetItemEnhanceData       = 4008_500,
    FailedGetRuneEnhanceData       = 4009_500,
    FailedCacheStageInfo           = 4010_500,
    FailedLoadStageInfo            = 4011_500,
    FailedDeleteStageInfo          = 4012_500,    
    
    // Controller (비즈니스/검증/권한/리소스 미존재 등)
    DuplicatedEmail                = 4000_400,
    FailedPasswordVerify           = 4003_401,
    CannotPurchaseCharacter        = 4005_400,
    CannotInsertNewCharacter       = 4006_400,
    AlreadyHaveCharacter           = 4009_400,
    CannotFindInventoryItem        = 4011_404, 
    CannotSellEquipmentItem        = 4012_404,
    AlreadyEquippedItem            = 4016_400,
    AlreadyEquippedRune            = 4017_400,
    CannotFindInventoryRune        = 4018_404,
    CannotFindCharacter            = 4019_404,
    AlreadyMaximumLevelItem        = 4020_400,
    AlreadyMaximumLevelRune        = 4021_400,
    GoldShortage                   = 4022_400,
    InvalidPrice                   = 4023_500,
    CannotLoadStageInfo            = 4024_404,
    CannotKillMonster              = 4025_400,
    CannotFindMonsterCode          = 4012_404,
    StageInProgress                = 4013_400,
}