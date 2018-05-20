namespace Graphite.Shared.Views

[<CLIMutable>]
type SignInView = {
  Email      : string
  Password   : string
  RememberMe : bool
}

type UserView = {
  Email       : string
  DisplayName : string
}