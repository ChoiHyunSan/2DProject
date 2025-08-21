namespace APIServer;

public enum EventType
{
    // MasterDb 1000 ~ 1999
    LoadAttendance = 1000,
    LoadCharacter = 1001,
    LoadItem = 1002,
    LoadRune = 1003,
    LoadQuest = 1004,
    LoadStage = 1005,
    
    LoadMasterDb = 6,
    LoadAccountDb = 7,
    LoadGameDb = 8,
    
    // GameDb 2000 ~ 2999
    CreateDefaultData = 2000,
    CreateUserGameData = 2001,
    InsertCharacter = 2002,
    InsertItem = 2003,
    InsertRune = 2004,
    InsertAttendanceMonth = 2005,
    InsertAttendanceWeek = 2006,
    InsertQuest = 2007,
    RollBackDefaultData = 2008,
    GetUserGoods = 2009,
    UpdateUserGoods = 2010,
    InsertNewCharacter = 2011,
    CheckUserHaveCharacter = 2012,
    UpdateItemLevel    = 2013,
    UpdateRuneLevel = 2014,
    GetUserCurrency = 2015,
    GetUserInventory = 2016,
    CheckItemEquipped = 2017,
    CheckRuneExists = 2018,
    CheckItemExists = 2019,
    CheckRuneEquipped = 2020,
    
    
    // AccountDb 3000 ~ 4999
    CreateAccountUserData = 3000,
    GetAccountUserData = 3001,
    
    // MemoryDb 4000 ~ 4999
    RegisterSession = 4000,
    GetSession = 4001,
    SessionLock = 4002,
    SessionUnLock = 4003,
    CacheGameData = 4004,
    CacheStageInfo = 4005,
    LoadStageInfo = 4006,
    UpdateKillMonster = 4007,
    DeleteStageInfo = 4008,
    
    // Controller 5000 ~ 5999
    CreateAccount = 5000,
    Login = 5001,
    PurchaseCharacter = 5002,
    SellItem = 5003,
    SellRune = 5004,
    EquipItem = 5005,
    EquipRune = 5006,
    EnhanceItem = 5007,
    EnhanceRune = 5008,
    GetClearStage = 5009,
    EnterStage = 5010,
    KillMonster = 5011,
    ClearStage = 5012,
    LoadGameData = 5013,
    AttendanceCheck = 5014,
    Register = 5015,
    LoginCheck = 5016,
    GetProgressQuest = 5017,
    GetCompleteQuest = 5018,
    RewardQuest = 5019,
}