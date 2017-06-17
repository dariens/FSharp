(** 
### Fibonacci in F# 
The following is a simple recursive function to calculate the fibonacci of
a particular value:
*)
let rec fib n =
    match n with
    | 0 -> 0
    | 1 -> 1
    | n -> n + fib(n-1)

(** To use this function you simply pass an integer to the function.
Follows an example which creates a variable _fib5_ and binds it to the value
created by passing **_5_** to the **_fib_** function *)
(*** define-output:fib5 ***)
let fib5 = fib 5

(** The variable **_fib5_** now contains the follwing value: *)
(*** include-value: fib5 ***)

(**
## Inline Math
A simple inline equation looks like $ \sum$.
*)

(**
### To include the 'it' value
*)

let numbers = [1..10]

(*** define-output: numbersSquared ***)
printfn "<p>This is a paragraph.</p>"
(*** include-output: numbersSquared ***)

(**
### Test
The following should look like code: ``(*** include-output: numbersSquared ***)``
*)

