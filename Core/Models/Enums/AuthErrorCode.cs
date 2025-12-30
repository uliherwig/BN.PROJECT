namespace BN.PROJECT.Core;

public enum AuthErrorCode
{
    None,
    UserNotFound,
    EmailSendError,
    EmailAlreadyExists,
    InvalidCredentials, 
    EmailNotVerified,
    Unauthorized,
    InternalServerError,
    UnKnown
}