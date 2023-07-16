﻿using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace KristofferStrube.Blazor.SVGEditor.Samples.CustomElements;

public class Node : Circle
{
    public Node(IElement element, SVGEditor svg) : base(element, svg)
    {
        string? id = element.GetAttribute("id");
        if (id is null || svg.Elements.Any(e => e.Id == id))
        {
            Id = Guid.NewGuid().ToString();
        }
        foreach (Connector connector in svg.Elements.Where(e => e is Connector connector && (connector.From?.Id == Id || connector.To?.Id == Id)))
        {
            connector.UpdateLine();
            connector.Rerender();
        }
    }

    public override Type Presenter => typeof(NodeEditor);

    public new double R { get => 50; set => base.R = value; }

    public override string Stroke
    {
        get => base.Stroke;
        set
        {
            base.Stroke = value;
            int[] parts = value[1..].Chunk(2).Select(part => int.Parse(part, System.Globalization.NumberStyles.HexNumber)).ToArray();
            Fill = "#" + string.Join("", parts.Select(part => Math.Min(255, part + 50).ToString("X2")));
        }
    }

    public HashSet<Connector> RelatedConnectors { get; } = new();

    public override void HandlePointerMove(PointerEventArgs eventArgs)
    {
        base.HandlePointerMove(eventArgs);
        if (SVG.EditMode == EditMode.Move)
        {
            foreach (Connector connector in RelatedConnectors)
            {
                connector.UpdateLine();
            }
        }
    }

    public override void BeforeBeingRemoved()
    {
        foreach (Connector connector in RelatedConnectors)
        {
            SVG.RemoveElement(connector);
        }
    }

    public static new void AddNew(SVGEditor SVG)
    {
        IElement element = SVG.Document.CreateElement("CIRCLE");
        element.SetAttribute("data-elementtype", "node");

        Node node = new(element, SVG)
        {
            Changed = SVG.UpdateInput,
            Stroke = "#28B6F6",
            R = 50
        };

        (double x, double y) startPos = SVG.LocalDetransform((SVG.LastRightClick.x, SVG.LastRightClick.y));
        (node.Cx, node.Cy) = startPos;

        SVG.ClearSelectedShapes();
        SVG.SelectShape(node);
        SVG.AddElement(node);
    }
}