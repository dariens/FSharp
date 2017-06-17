
(*
type PersonalName =
    {FirstName: string;
     MiddleInitial: string option;
     LastName: string}

type EmailAddress = EmailAddress of string

type EmailContactInfo =
    {EmailAddress : EmailAddress;
     IsMailVerified: bool}

type ZipCode = ZipCode of string
type StateCode = StateCode of string

type PostalAddress =
    {Address1: string;
     Address2: string;
     City: string;
     State: StateCode
     Zip: ZipCode}

type PostalContactInfo =
    {Address: PostalAddress;
     IsAddressValid: bool}

type Contact =
    {Name: PersonalName;
     EmailContactInfo: EmailContactInfo;
     PostalContactInfo: PostalContactInfo}

let CreateEmailAddress (s: string) =
    if System.Text.RegularExpressions.Regex.IsMatch(s,@"^\S+@\S+\.\S+$")
        then Some (EmailAddress s)
        else None

let CreateStateCode (s: string) =
    let s' = s.ToUpper()
    let stateCodes = ["AZ"; "CA"; "NY"]
    if stateCodes |> List.exists ((=) s')
        then Some (StateCode s')
        else None

CreateStateCode "CA"
CreateStateCode "NH"



let test1 = CreateEmailAddress "darien.shannon@gmail.com"
let test2 = CreateEmailAddress "example.com"
match test2 with
| Some (EmailAddress s) -> printfn "%s" s
| None -> printfn "bad email"

type CreationResult<'T> =
    | Success of 'T
    | Error of string

let CreateEmailAddress2 (s: string) =
    if System.Text.RegularExpressions.Regex.IsMatch(s,@"^\S+@\S+\.\S+$")
        then Success (EmailAddress s)
        else Error "Email address must contain an @ sign"

let test3 = CreateEmailAddress2 "darien@example.com"

match test3 with
| Success (EmailAddress s) -> printfn "EMAIL: %s" s
| Error e -> printfn "ERROR: %s" e

let CreateEmailAddressWithContinuations success failure (s: string) =
    if System.Text.RegularExpressions.Regex.IsMatch(s,@"^\S+@\S+\.\S+$") 
        then success (EmailAddress s)
        else failure "Email address must contain an @ sign"

let success (EmailAddress s) = printfn "Success creating eamil %s" s
let failure msg = printfn "error creating email: %s" msg

CreateEmailAddressWithContinuations success failure "example.com"

let success2 e = Some e
let failure2 _ = failwith "bad email address"

CreateEmailAddressWithContinuations success2 failure2 "test.com"


let success3 e = Some e
let failure3 _ = None
let createEmail = CreateEmailAddressWithContinuations success3 failure3
createEmail "x@example.com"
createEmail "bad email"
*)

/// Creating modules for wrapper types

module EmailAddress =

    type T = EmailAddress of string

    let createWithCont success failure (s: string) =
        if System.Text.RegularExpressions.Regex.IsMatch(s,@"^\S+@\S+\.\S+$") 
            then success  (EmailAddress s)
            else failure "Email address must contain an @ sign"

    let create s =
        let success e = Some e
        let failure _ = None
        createWithCont success failure s

    let apply f (EmailAddress e) = f e

    let value e = apply id e

module ZipCode = 

    type T = ZipCode of string

    let createWithCont success failure (s: string) =
        if System.Text.RegularExpressions.Regex.IsMatch(s,@"^\d{5}$") 
            then success (ZipCode s)
            else failure "Zip code must be 5 digits"

    let create s =
        let success e = Some e
        let failure _ = None
        createWithCont success failure s

    let apply f (ZipCode e) = f e

    let value e = apply id e


module StateCode =

    type T = StateCode of string

    let createWithCont success failure (s: string) =
        let s' = s.ToUpper()
        let stateCodes = ["AZ"; "CA"; "NY"; "NV"]
        if stateCodes |> List.exists ((=) s')
            then success (StateCode s')
            else failure "State is not in the list"

    let create s =
        let success e = Some e
        let failure _ = None
        createWithCont success failure s

    let apply f (StateCode e) = f e

    let value e = apply id e


type Name =
    {Firstname: string;
     MiddleInitial: string option;
     Lastname: string;}

type EmailContactInfo =
    {EmailAddress: EmailAddress.T;
     IsEmailVerified: bool;}

type PostalAddress =
    {Address1: string;
     Address2: string option;
     City: string;
     State: StateCode.T;
     Zip: ZipCode.T;}

type PostalContactInfo =
    {Address: PostalAddress;
     IsAddressValid: bool;}

type ContactInfo =
    | EmailOnly of EmailContactInfo
    | PostOnly of PostalContactInfo
    | EmailAndPost of EmailContactInfo * PostalContactInfo

type Contact =
    {Name: Name;
     ContactInfo: ContactInfo;}


let contactFromEmail name emailStr =
    let emailOpt = EmailAddress.create emailStr

    match emailOpt with
    | Some email ->
        let emailContactInfo =
            {EmailAddress = email; IsEmailVerified = false}
        let contactInfo = EmailOnly emailContactInfo
        Some {Name = name; ContactInfo = contactInfo}
    | None -> None

let name = {Firstname = "Darien"; MiddleInitial = None; Lastname = "Shannon"}

let contactOpt = contactFromEmail name "darien.shannon@gmail.com"

let updatePostalAddress contact newPostalAddress =
    let {Name = name; ContactInfo = contactInfo} = contact
    let newContactInfo =
        match contactInfo with
        | EmailOnly email ->
            EmailAndPost (email, newPostalAddress)
        | PostOnly _ ->
            PostOnly newPostalAddress
        | EmailAndPost (email, _) ->
            EmailAndPost (email, newPostalAddress)
    {Name = name; ContactInfo = newContactInfo}

let contact = contactOpt.Value

let newPostalAddress =
    let state = StateCode.create "CA"
    let zip = ZipCode.create "97210"
    {
        Address =
            {
            Address1 = "123 Main";
            Address2 = None;
            City = "Berly Hills";
            State = state.Value;
            Zip = zip.Value;
            };
        IsAddressValid = false
    }

let newContact = updatePostalAddress contact newPostalAddress










    





