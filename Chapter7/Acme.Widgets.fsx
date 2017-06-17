namespace Acme.Widgets

open System.Drawing

type Wheel = Square | Round | Triangle

type Widget = {id : int; wheels : Wheel list; Size: string}

type Lever = PlasticLever | WoodenLever

namespace Acme.Suppliers

type LeverSupplier = {name : string; leverKind : Acme.Widgets.Lever}

