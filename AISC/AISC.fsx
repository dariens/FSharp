type StructuralShape=
    abstract member Area : float


type SingleAngle(verticalLeg, horizontalLeg, thickness, radius) =
    interface StructuralShape with
        member this.Area = (verticalLeg + horizontalLeg - thickness) * thickness : double


type DoubleAngle(singleAngle:SingleAngle) =
    let singleAngle = singleAngle :> StructuralShape
    interface StructuralShape with
         member this.Area = singleAngle.Area * 2.0
    
    new(verticalLeg,horizontalLeg,thickness,radius) =
        DoubleAngle(SingleAngle(verticalLeg,horizontalLeg,thickness,radius))



let twoByTwoByQuarter = SingleAngle(2.0,2.0,0.25,0) 


let doubleTwoByTwoByQuarter = DoubleAngle(twoByTwoByQuarter) 

let doubeTwo = DoubleAngle(2.0,2.0,0.25,0) 

let sShapes = [twoByTwoByQuarter :> StructuralShape;doubleTwoByTwoByQuarter :> StructuralShape]

    

