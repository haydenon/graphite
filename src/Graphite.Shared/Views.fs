namespace Graphite.Shared.Views

[<CLIMutable>]
type SignInView = {
  Email      : string
  Password   : string
  RememberMe : bool
}

[<CLIMutable>]
type RegisterView = {
  Email       : string
  DisplayName : string
  Password    : string
}

[<CLIMutable>]
type UserView = {
  Email       : string
  DisplayName : string
}

[<CLIMutable>]
type StoreView = {
  Id   : string
  Name : string
}
