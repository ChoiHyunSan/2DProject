namespace APIServer;

/// <summary>
/// 에러 코드를 정리하는 Enum 
/// (에러 코드) + (Status_Code) 형식으로 이뤄져있다.
/// 
/// </summary>
public enum ErrorCode
{
    None = 0_200,
    
    // Db 
    FailedDataLoad = 100_500,
    
    // Middleware 500 ~ 999
    FailedParseAuthorizeInfo = 500_400,
    FailedAuthorizeTokenVerify = 501_401,
    
    // MasterDb 1000 ~ 1999
    
    
    // GameDb 2000 ~ 2999
    FailedInsertData = 2000_500,
    FailedRollbackDefaultData = 2001_500,
    FailedLoadAllGameData = 2002_500,
    
    // AccountDb 3000 ~ 3999
    FailedCreateAccountUserData = 3000_500, 
    FailedGetAccountUserData = 3001_500,
    
    // MemoryDb 4000 ~ 4999
    FailedRegisterSession = 4000_500,
    FailedGetSession = 4001_500,
    SessionNotFound = 4002_404,
    FailedSessionLock = 4003_500,
    FailedSessionUnLock = 4004_500,
    AlreadySessionLock = 4005_409,
    SessionLockNotFound = 4006_404,
    
    // Controller 5000 ~ 5999
    DuplicatedEmail = 4000_400,
    FailedCreateUserData = 4001_500,
    FailedCreateAccount = 4002_500,
    FailedPasswordVerify = 4003_401,
    FailedLoadUserData = 4004_500,
}