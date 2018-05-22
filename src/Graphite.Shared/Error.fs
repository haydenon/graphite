module Graphite.Shared.Errors

type ValidationError =
  | MinLength of int
  | MaxLength of int
  | NullValue
  | InvalidFormat of string

type AppError =
  | ValidationFailures of (ValidationError * string) list // Validation error * source
  | NotFound
  // Authenication
  | NotAuthenticated
  | InvalidToken of string // Token
  | DuplicateEmailAddress of string // Email
  | PasswordMustContain
  | IncorrectUserOrPassword of string //Email
  | LockedOut of string //Email
  | InvalidPasswordReset
  // Others
  | NoUserWithEmail of string
  // API
  | MappingError of string // Description
  | BadModel
  | UnexpectedError

let validationMessage err (source : string) =
  let source = source.ToLower()
  match err with
  | MinLength len -> sprintf "The %s value is smaller than the minimum charater length of %i" source len
  | MaxLength len -> sprintf "The %s value is larger than the maximum charater length of %i" source len
  | NullValue -> sprintf "You must provide the %s value" source
  | InvalidFormat format -> sprintf "The %s must be a proper %s" source format

let message =
  function
  | UnexpectedError -> ["An unexpected error occurred" ]
  | ValidationFailures(errors) -> List.map (fun err -> validationMessage (fst err) (snd err)) errors
  | DuplicateEmailAddress(_) -> [ "The email address provided is already in use" ]
  | PasswordMustContain -> [ "Password must be 8 characters or longer" ]
  | IncorrectUserOrPassword(_) -> [ "Incorrect email address or password" ]
  | BadModel -> [ "The model provided was not valid" ]
  | InvalidToken(_) -> ["The token provided was not valid" ]
  | MappingError(description) -> [description]
  | NotFound -> ["The resource was not found"]
  | NotAuthenticated -> ["You must be authenticated"]
  | InvalidPasswordReset -> ["Invalid password reset link"]
  | LockedOut(_) -> ["You are currently locked out due to too many failed sign in attempts. Please try again later"]
  | NoUserWithEmail(email) -> [sprintf "There is no user with the email address %s" email]

let code =
  function
  | _ -> 0

type ErrorView = {
  Code   : int
  Errors : string list
}

let toErrorModel error : ErrorView =
  let code = code error
  { Errors = message error; Code = code }