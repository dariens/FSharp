(** 
### Chapter 1: _An Introduction to F#_ 
$$ \dfrac{x}{y} $$
`(** ## Hello **)`
*)

type LatexOutput = {Latex : string}
let latex = {Latex = "\dfrac{x}{y}"}
latex.Latex
(***include-value: latex***)

type Markdown = Markdown of string


Markdown("
### First Level heading" + string (1 + 1)
)