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
    FailedEquipItem                = 2017_500,
    FailedEquipRune                = 2018_500,  
    FailedUpdateClearStage         = 2019_500,
    FailedRewardClearStage         = 2020_500,
    FailedSellItem                  = 2021_500,
    FailedGetClearStage            = 2022_500,
    AttendanceAlreadyComplete     = 2023_400,
    AttendanceAlreadyDoneToday    = 2024_400,
    FailedAttendance                = 2025_500,
    FailedAttendanceReward         = 2026_500,
    CannotFindUserAttendance       = 2027_404,
    FailedRewardQuest              = 2028_500,
    FailedCompleteQuest            = 2029_500,
    CannotFindMail                  = 2030_404,
    FailedReceiveMail             = 2031_500,
    FailedSendMail                  = 2032_500,
    FailedGetMail                 = 2033_500,
    FailedEnhanceCharacter        = 2034_500,
    FailedUnEquipItem              = 2035_500,
    FailedUnEquipRune              = 2036_500,
    FailedTransaction              = 2037_500,
    
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
    CannotFindSession               = 4013_404,
    CannotFindInStageInfo           = 4014_404,
    CannotFindQuestList               = 4015_404,
    FailedRedisUpdate               = 4016_500,
    CannotFindCharacterData        = 4017_404,
    CannotFindItemData              = 4018_404,
    CannotFindRuneData              = 4019_404,
    CannotFindUserGameData        = 4020_404,
    
    // Controller (비즈니스/검증/권한/리소스 미존재 등)
    DuplicatedEmail                = 5000_400,
    FailedPasswordVerify           = 5003_401,
    CannotPurchaseCharacter        = 5005_400,
    CannotInsertNewCharacter       = 5006_400,
    AlreadyHaveCharacter           = 5009_400,
    CannotFindInventoryItem        = 5011_404, 
    CannotSellEquipmentItem        = 5012_404,
    AlreadyEquippedItem            = 5016_400,
    AlreadyEquippedRune            = 5017_400,
    CannotFindInventoryRune        = 5018_404,
    CannotFindCharacter            = 5019_404,
    AlreadyMaximumLevelItem        = 5020_400,
    AlreadyMaximumLevelRune        = 5021_400,
    GoldShortage                   = 5022_400,
    InvalidPrice                   = 5023_500,
    CannotLoadStageInfo            = 5024_404,
    CannotKillMonster              = 5025_400,
    CannotFindMonsterCode          = 5012_404,
    StageInProgress                = 5013_400,
    FailedEnhanceItem               = 5014_500,
    FailedEnhanceRune               = 5015_500,
    FailedEnterStage                = 5016_500,
    FailedClearStage                = 5017_500,
    FailedKillMonster               = 5018_500,
    FailedRegister                  = 5019_500,
    FailedLogin                     = 5020_500,
    CannotFindCompleteQuest       = 5021_404,
    AlreadyEarnReward              = 5022_400,
    FailedRefreshQuest              = 5023_500,
    FailedAttendanceReset         = 5024_500,
    NotEquiptItem                    = 5025_400,
    NotEquiptRune                    = 5025_400,
    CannotFindAccountUser          = 5026_403,
}