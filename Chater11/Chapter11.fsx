open System.IO

let watcher = new FileSystemWatcher(@"C:\Users\darien.shannon\Desktop\Test", EnableRaisingEvents = true)

watcher.Changed.Add(fun args -> printfn "File %s was changed!" args.Name)
watcher.Dispose()

/// Creating and Publishing Events

open System
open System.Windows.Forms

type RandomTicker(approxInterval) =
    let timer = new Timer()
    let rnd = new System.Random(99)
    let tickEvent = new Event<int> ()
    
    let chooseInterval() : int =
        approxInterval + approxInterval / 4 - rnd.Next(approxInterval / 2)

    do timer.Interval <- chooseInterval()

    do timer.Tick.Add(fun args ->
        let interval = chooseInterval()
        tickEvent.Trigger interval
        timer.Interval <- interval)

    member x.RandomTick = tickEvent.Publish
    member x.Start() = timer.Start()
    member x.Stop() = timer.Stop()
    interface IDisposable with
        member x.Dispose() = timer.Dispose()

    member x.Dispose() = (x :> IDisposable).Dispose()

let rt = new RandomTicker(1000)

rt.RandomTick |> Observable.add (fun evArgs -> printfn "Tick!")

rt.RandomTick.Add(fun nextInterval -> printfn "Tick, next = %A" nextInterval)

rt.Start()

rt.Dispose()

let form = new Form()

form.Show()

form.MouseMove
    |> Event.filter (fun args -> args.X > 100)
    |> Event.add (fun args -> printfn "Mouse, (X, Y) = (%A, %A)" args.X args.Y)

form.Dispose()



/// Fetching Multiple Web Pages in Parallel, Asynchronously

open System.Net
open System.IO

let museums =
    [ "MOMA", "http://moma.org/";
      "British Museum", "http://www.thebritishmuseum.ac.uk/";
      "Prado", "http://www.museodelprado.es/" ]

let tprintfn fmt =
   printf "[Thread %d]" System.Threading.Thread.CurrentThread.ManagedThreadId
   printfn fmt

let fetchAsync(nm, url : string) =
    async {
        tprintfn "Creating request for %s..." nm
        let req = WebRequest.Create(url)
        
        let! resp = req.AsyncGetResponse()
        
        tprintfn "Getting resonse stream for %s..." nm
        let stream = resp.GetResponseStream()
        
        tprintfn "Reading response for %s..." nm
        let reader = new StreamReader(stream)
        let html = reader.ReadToEnd()
        
        tprintfn "Read %d characters for %s..." html.Length nm
        }
Async.Parallel [for nm, url in museums -> fetchAsync(nm, url)]
    |> Async.Ignore
    |> Async.RunSynchronously


let failingAsync = async {do printfn "fail"}

Async.RunSynchronously failingAsync

let failingAsyncs = [async {do failwith "fail A"};
                     async {do failwith "fail B"}]

Async.RunSynchronously (Async.Parallel failingAsyncs)

Async.RunSynchronously (Async.Catch failingAsync)

/// Agents

type Agent<'T> = MailboxProcessor<'T>

let counter =
   new Agent<_>(fun inbox ->
       let rec loop n =
           async {printfn "n = %d, waiting..." n
                  let! msg = inbox.Receive()
                  return! loop (n + msg)}
       loop 0)

counter.Start()

counter.Post(1)

counter.Post(2)

type internal msg = Increment of int | Fetch of AsyncReplyChannel<int> | Stop

type CountingAgent() =
    let counter = MailboxProcessor.Start(fun inbox ->
        let rec loop n =
            async {let! msg = inbox.Receive()
                   match msg with
                   | Increment m ->
                       return! loop(n + m)
                   | Stop ->
                       return ()
                   | Fetch replyChannel ->
                       do replyChannel.Reply n
                       return! loop n}
        loop(0))

    member a.Increment(n) = counter.Post(Increment n)
    member a.Stop() = counter.Post Stop
    member a.Fetch() = counter.PostAndReply(fun replyChannel -> Fetch replyChannel)

let counter2 = new CountingAgent()

counter2.Increment(1)

counter2.Fetch()

counter2.Increment(2)

counter2.Fetch()

counter2.Stop()

counter2.Increment(1)



