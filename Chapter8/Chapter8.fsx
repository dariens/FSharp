#r @"packages/FSharp.Text.RegexProvider/lib/net40/FSharp.Text.RegexProvider.dll"

open FSharp.Text.RegexProvider

type PhoneRegex = Regex< @"(?<AreaCode>^\d{3})-(?<PhoneNumber>\d{3}-\d{4}$)">

let results = PhoneRegex().Match("425-123-2345")

let areaCode = results.Groups.["AreaCode"].Value

type FaultyAreaCodeRegex = Regex< @"(?<AreaCode>^\d{3})">


/// Using XML as a Concrete Language Format

let inp = """<?xml version="1.0" encoding="utf-8" ?>
          <Scene>
                <Composite>
                <Circle radius='2' x='1' y='0'/>
                <Composite>
                  <Circle radius='2' x='4' y='0'/>
                  <Square side='2' left='-3' top='0'/>
                </Composite>
                <Ellipse top='2' left='-2' width='3' height='4'/>
                </Composite>
          </Scene>"""

open System.Xml

let doc = new XmlDocument()

doc.LoadXml(inp)

doc.ChildNodes

fsi.AddPrinter(fun (x:XmlNode) -> x.OuterXml)

doc.ChildNodes

doc.ChildNodes.Item(1)

doc.ChildNodes.Item(1).ChildNodes.Item(0)

open System.Drawing
open System.Xml

type Scene =
    | Ellipse of RectangleF
    | Rect of RectangleF
    | Composite of Scene list

    static member Circle(center : PointF, radius) =
        Ellipse(RectangleF(center.X - radius, center.Y - radius,
                           radius * 2.0f, radius * 2.0f))

    static member Square(left, top, side) =
        Rect(RectangleF(left, top, side, side))

let extractFloat32 attrName (attribs : XmlAttributeCollection) =
    float32 (attribs.GetNamedItem(attrName).Value)

let extractPointF (attribs : XmlAttributeCollection) =
    PointF(extractFloat32 "x" attribs, extractFloat32 "y" attribs)

let extractRectangleF (attribs : XmlAttributeCollection) =
    RectangleF(extractFloat32 "left" attribs, extractFloat32 "top" attribs,
               extractFloat32 "width" attribs, extractFloat32 "height" attribs)

let rec extractScene (node : XmlNode) =
    let attribs = node.Attributes
    let childNodes = node.ChildNodes
    match node.Name with
    | "Circle" ->
        Scene.Circle(extractPointF(attribs), extractFloat32 "radius" attribs)
    | "Ellipse" ->
        Scene.Ellipse(extractRectangleF(attribs))
    | "Rectangle" ->
        Scene.Rect(extractRectangleF(attribs))
    | "Square" ->
        Scene.Square(extractFloat32 "left" attribs, extractFloat32 "top" attribs,
                     extractFloat32 "side" attribs)
    | "Composite" ->
        Scene.Composite [for child in childNodes -> extractScene(child)]
    | _ -> failwithf "unable to convert XML '%s'" node.OuterXml
            
let extractScenes (doc : XmlDocument) =
    [for node in doc.ChildNodes do
        if node.Name = "Scene" then
            yield (Composite
                       [for child in node.ChildNodes -> extractScene(child)])]

fsi.AddPrinter(fun (r:RectangleF) ->
   sprintf "[%A,%A,%A,%A]" r.Left r.Top r.Width r.Height)

extractScenes doc

#r @"packages\FSharp.Data\lib\net40\FSharp.Data.dll"

#r "System.Xml.Linq"

open FSharp.Data
open FSharp.Data.HtmlNode

[<Literal>]
let customersXmlSample = """
<Customers>
    <Customer name="ACME">
        <Order Number="A012345">
            <OrderLine Item="widget" Quantity="1"/>
        </Order>
        <Order Number="A012346">
            <OrderLine Item="trinket" Quantity="2"/>
        </Order>
    </Customer>
    <Customer name="Southwind">
        <Order Number="A012347">
            <OrderLine Item="skyhook" Quantity="3"/>
            <OrderLine Item="gizmo" Quantity="4"/>
        </Order>
    </Customer>
</Customers>"""

type InputXml = XmlProvider<customersXmlSample>

let inputs = InputXml.GetSample().Customers

let orders =
    [ for customer in inputs do
          for order in customer.Orders do
              for line in order.OrderLines do
                  yield (customer.Name,order.Number,line.Item,line.Quantity)]


[<Literal>]
let orderLinesXmlSample = """
<OrderLines>
    <OrderLine Customer="ACME" Order="A012345" Item="widget" Quantity="1"/>
    <OrderLine Customer="ACME" Order="A012346" Item="trinket" Quantity="2"/>
    <OrderLine Customer="Southwind" Order="A012347" Item="skyhook" Quantity="3"/>
    <OrderLine Customer="Southwind" Order="A012347" Item="gizmo" Quantity="4"/>
</OrderLines>"""

type OutputXml = XmlProvider<orderLinesXmlSample>

let orderLines =
    OutputXml.OrderLines
        [| for (name, number, item, quantity) in orders do
            yield OutputXml.OrderLine(name, number, item, quantity) |]

orderLines.XElement.ToString()




