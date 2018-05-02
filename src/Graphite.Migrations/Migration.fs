namespace Graphite.Migrations

type Command = {
  Up   : string
  Down : string
}

module Index =
  type T = Index of int
  let create ind =
    if ind <= 0 then failwith "Index must be positive"
    Index ind
  let apply f (Index i) = f i
  let value s = apply id s

type IMigration =
  abstract member Index    : Index.T
  abstract member Name     : string
  abstract member Commands : Command list