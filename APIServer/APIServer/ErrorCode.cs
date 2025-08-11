namespace APIServer;

public enum ErrorCode
{
    None = 0,
    
    // Db
    FailedDataLoad = 100,
    
    // MasterDb 1000 ~ 1999
    
    
    // GameDb 2000 ~ 2999
    FailedInsertData = 2000,
    FailedRollbackDefaultData = 2001,
    
    // AccountDb 3000 ~ 3999
    FailedCreateAccountUserData = 3000, 
    
    
    // MemoryDb 4000 ~ 4999
    FailedRegisterSession = 4000,
    
    // Controller 5000 ~ 5999
    DuplicatedEmail = 4000,
    FailedCreateUserData = 4001,
    FailedCreateAccount = 4002,
    FailedPasswordVerify = 4003,
    FailedLoadUserData = 4004,
}