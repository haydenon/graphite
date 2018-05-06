module Graphite.Dapper.Helpers

let escapeLike (str : string) =
  str.Replace("\\", "\\\\")
     .Replace("%", "\\%")
     .Replace("_", "\\_")

let like str =
  sprintf "%%%s%%" (escapeLike str)
