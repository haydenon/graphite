open System
open System.IO
open System.Reflection

open Microsoft.Extensions.Configuration
open Npgsql

open Graphite.Migrations

open CommandLine

let [<Literal>] MigrationNamespace = "Graphite.Migrations.Migrations"

[<Verb("sql", HelpText = "Generate sql between migrations")>]
type SqlOptions = {
    [<Value(0, MetaName = "start", Default = 0, HelpText = "Starting migration")>] startMigration : int;
    [<Value(1, MetaName = "end", Default = Int32.MaxValue, HelpText = "End migration")>] endMigration : int;
    [<Option('f', "file", HelpText = "File to save sql to")>] file : string;
}

[<Verb("update", HelpText = "Update database")>]
type UpdateOptions = {
    [<Value(0, MetaName = "Migration", Default = Int32.MaxValue, HelpText = "Migration to update database to")>] migration : int;
}

[<Verb("list", HelpText = "List migrations")>]
type ListOption = 
  new() = {}

[<Verb("current", HelpText = "Current migration")>]
type CurrentOption = 
  new() = {}

let log debug msg =
    if debug then printfn "%s" msg

let noop _ =
    ()

let getConfig () =
    let configBuilder = new ConfigurationBuilder()
    configBuilder
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
        .AddEnvironmentVariables()
        .Build()

let getMigrations = 
    let migrationsAssembly = Assembly.GetExecutingAssembly();
    let migrations = 
        migrationsAssembly.GetTypes()
        |> Array.filter (fun t -> t.Namespace = MigrationNamespace && typeof<IMigration>.IsAssignableFrom(t))
        |> Array.map (fun t -> Activator.CreateInstance(t) :?> IMigration)
        |> Array.sortBy (fun m -> m.Index)
    let duplicate =
        Array.groupBy (fun (m : IMigration) -> Index.value m.Index) migrations
        |> Array.exists (fun grp -> (Array.length (snd grp)) > 1)
    if duplicate then failwith "Migrations contain duplicate indexes"
    migrations

let migrationTable (config : IConfiguration) =
    config.["MigrationTable"]

let connection (config : IConfiguration) =
    let connectionString = config.GetSection("Data").["ConnectionString"]
    new NpgsqlConnection(connectionString)

let scalar<'T> (connection : NpgsqlConnection) query = 
    use cmd = connection.CreateCommand()
    cmd.CommandText <- query
    cmd.ExecuteScalar() :?> 'T

let scalarOption<'T> (connection : NpgsqlConnection) query = 
    use cmd = connection.CreateCommand()
    cmd.CommandText <- query
    let res = cmd.ExecuteScalar()
    match Convert.IsDBNull(res) with
    | true  -> None
    | false -> Some(res :?> 'T)

let execute (connection : NpgsqlConnection) query = 
    use cmd = connection.CreateCommand()
    cmd.CommandText <- query
    cmd.ExecuteNonQuery()

let initialise log (connection : NpgsqlConnection) migrationTable =
    connection.Open()
    let query = Queries.checkMigrationTable migrationTable
    let tableExists = scalar<bool> connection query
    if not tableExists then
        log "Creating migration table..."
        execute connection (Queries.createMigrationTable migrationTable) |> ignore

let getCurrentMigration (connection : NpgsqlConnection) migrationTable =
    let query = Queries.currentMigration migrationTable
    scalarOption<int> connection query

let listMigrations () =
    printfn "Migrations:"
    getMigrations
    |> Array.iter (fun m -> printfn "- %i_%s" (Index.value m.Index) m.Name)

let isIndex index (migration : IMigration) = 
    index = Index.value migration.Index

let migrationAt migrations index =
    Array.tryFind (isIndex index) migrations

let validateStartEnd migrations start last =
    match start with
    | 0 -> ()
    | _ ->
        match migrationAt migrations start with
        | Some _ -> ()
        | None   -> failwith "Start migration doesn't exist"

    match migrationAt migrations last with
    | Some _ -> ()
    | None   -> failwith "End migration doesn't exist"

let getSqlForMigration up migrationTable (migration : IMigration) =
    let cmdSql cmd =
        if up
        then cmd.Up
        else cmd.Down
    let sql = 
        migration.Commands
        |> List.fold (fun acc cmd -> sprintf "%s%s\n\n" acc (cmdSql cmd)) ""
    let query =
        match up with
        | true  -> Queries.insertMigration
        | false -> Queries.removeMigration
    query migrationTable migration
    |> sprintf "%s%s\n\n" sql

let checkDirection start last =
    let up = start < last
    if up then
        (up, start, last)
    else
        (up, last, start)

let getSql migrationTable (migrations : IMigration []) start last =
    let (up, start, last) = checkDirection start last
    validateStartEnd migrations start last
    let bottom =
        match start with
        | 0 -> 0
        | _ -> (Array.findIndex (isIndex start) migrations) + 1
    let top = (Array.findIndex (isIndex last) migrations)
    let indexes = [|bottom..top|]
    let migrations = [| for i in indexes -> migrations.[i]|] 
    let migrations = if up then migrations else Array.rev migrations
    migrations
    |> List.ofArray
    |> List.map (getSqlForMigration up migrationTable)
    |> List.reduce (fun acc sql -> sprintf "%s%s\n" acc sql)

let generateSql (opts : SqlOptions) =
    let generate () =
        let migrations = getMigrations
        if Array.isEmpty migrations then failwith "No migrations to generate sql for"
        let start = opts.startMigration
        let last =
            match opts.endMigration with
            | Int32.MaxValue -> Index.value (Array.last migrations).Index
            | e              -> e
        let config = getConfig()
        let migrationTable = (migrationTable config)
        let sql = getSql migrationTable migrations start last
        if String.IsNullOrWhiteSpace opts.file then
            printfn "%s" sql
        else 
            File.WriteAllText(opts.file, sql)
    match opts.endMigration = opts.startMigration with
    | true ->  failwith "Start and end migration must be different"
    | false -> generate()

let updateDatabase (opts : UpdateOptions) =
    let migrations = getMigrations
    let migration =
        match opts.migration with
        | Int32.MaxValue -> Index.value (Array.last migrations).Index
        | e              -> e
    let config = getConfig()
    use connection = connection config
    let migrationTable = (migrationTable config)
    initialise noop connection migrationTable
    let current =
        match getCurrentMigration connection migrationTable with
        | Some ind -> ind
        | None     -> 0
    let update () =
        let sql = getSql migrationTable migrations current migration
        execute connection sql |> ignore
    match migration = current with
    | true  -> ()
    | false -> update()
    log true "Done"
    

let currentMigration debug = 
    let log = log debug
    let config = getConfig()
    use connection = connection config
    let migrationTable = (migrationTable config)
    initialise log connection migrationTable
    let current =
        match getCurrentMigration connection migrationTable with
        | Some ind -> ind
        | None     -> 0
    if debug
    then
        match current with
        | 0 -> log "Database has no migrations applied"
        | ind ->
            let nameQuery = Queries.currentMigrationName migrationTable
            let currentName = scalar<string> connection (nameQuery ind)
            log <| sprintf "Currently at migration %i - %s" ind currentName
    else
        printfn "%i" current

let tryRun f =
    let run () = 
        f()
        0
    try run() with
    | err ->
        printfn "%s" err.Message
        1

[<EntryPoint>]
let main argv =
    let result = Parser.Default.ParseArguments<SqlOptions, UpdateOptions, ListOption, CurrentOption>(argv)
    match result with
    | :? CommandLine.Parsed<obj> as command ->
        match command.Value with
        | :? SqlOptions as opts    -> tryRun(fun () -> generateSql opts)
        | :? UpdateOptions as opts -> tryRun(fun () -> updateDatabase opts)
        | :? ListOption            -> tryRun(fun () -> listMigrations())
        | :? CurrentOption         -> tryRun(fun () -> currentMigration false)
        | _ -> 1
    | _ -> tryRun(fun () -> currentMigration true)
