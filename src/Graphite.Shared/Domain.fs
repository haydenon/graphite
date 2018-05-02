namespace Graphite.Shared.Domain

type JWT = string

type UserData = {
    UserName : string
    Token    : JWT
}