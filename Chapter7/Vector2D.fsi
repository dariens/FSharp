type Vector2D =
    { DX : float; DY : float}

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Vector2D =
   let length v = sqrt(v.DX * v.DX + v.DY * v.DY)
   let zer0 = {DX = 0.0; DY = 0.0}




