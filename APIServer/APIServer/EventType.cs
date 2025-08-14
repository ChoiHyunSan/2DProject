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

    // AccountDb 3000 ~ 4999
    CreateAccountUserData = 3000,
    GetAccountUserData = 3001,
    
    // MemoryDb 4000 ~ 4999
    RegisterSession = 4000,
    GetSession = 4001,
    SessionLock = 4002,
    SessionUnLock = 4003,
    CacheGameData = 4004,
    
    // Controller 5000 ~ 5999
    CreateAccount = 5000,
    Login = 5001,
    
}