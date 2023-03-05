using System.Text.Json;
using Plotly.NET;
using Plotly.NET.LayoutObjects;
using Plotly.NET.ConfigObjects;
using Microsoft.FSharp.Collections;

namespace LibDiffMeth;

public class Diferentiator
{
    public double[]? ys {get; set;}
    public int Multiplo {get; set;} = 5;
    public bool Penalizada {get; set;} = false;
    public double FactorPenalty {get; set;} = 0.0;
    public double FactorEscala {get; protected set;} = 0.0;
    public string TipoExtremos {get; set;} = "p";

    public List<double[]>? MakeSplineDiff()
    {
        List<double[]> Retorna = new List<double[]>(4);

        if (ys == null || (Penalizada && (FactorPenalty < -15.0 || FactorPenalty > 15.0))) {
            return null;
        }
        try {
            int BoundT = 0;
            double[] xs = new double[ys.Length];
            double[] xs2 = new double[ys.Length * Multiplo];

            if (TipoExtremos.ToLower() == "n") BoundT = 1;

            for (int i = 0; i < ys.Length; i++)
            {
                xs[i] = i;
            }
            for (int i = 0; i < xs2.Length; i++)
            {
                xs2[i] = i / Multiplo;
            }
            double[] f1 = new double[xs2.Length];
            double[] d1 = new double[xs2.Length];
            alglib.spline1dinterpolant inter;
            alglib.spline1dfitreport repo;
            int info;
            alglib.spline1dconvdiffcubic(xs, ys, xs.Length, BoundT, 0.0, BoundT, 0.0, xs2, xs2.Length, out f1, out d1);

            double[] d1pen = new double[xs2.Length];

            if (Penalizada) {
                alglib.spline1dfitpenalized(xs2, d1, xs2.Length, 50, FactorPenalty, out info, out inter, out repo);
                        
                for (int i = 0; i < d1.Length; i++)
                {
                    d1pen[i] = alglib.spline1dcalc(inter, xs2[i]);
                } 
            }
            else {
                d1pen = d1;
            }
            double[] f1Dev = new double[xs2.Length];      

            for (int i = 0; i < xs2.Length; i++)
            {
                double suma = 0.0;
                for (int j = 0; j < i; j++)
                {
                    suma += d1pen[j];
                }
                f1Dev[i] = (suma + d1pen[i]) / Multiplo;                
            }
            var Primerof1 = f1[0];
            var Primerof1Dev = f1Dev[0];

            for (int i = 0; i < xs2.Length; i++)
            {
                f1Dev[i] -= Primerof1Dev;
                f1[i] -= Primerof1;
            }
            Retorna.Add(xs2);
            Retorna.Add(f1);
            Retorna.Add(f1Dev);
            Retorna.Add(d1pen);

            return Retorna;
        }
        catch {
             return null;
        }       
    }
    public GenericChart.GenericChart Make_Graf(List<double[]> Lista) {

        string Xunit, Yunit, Y2unit, Y3unit, Titulo;

        Yunit = "u(y)";
        Y3unit = "u(y) : u(x)";
        Y2unit = "Acum [ u(y) : u(x) ]";
        Titulo = "Función y acumulación en base a derivada";        
        Xunit = "u(x)";

        string ColorF = "blue";
        string ColorD = "maroon";
        string ColorFD = "green";
        
        LinearAxis xAxis = new();
        xAxis.SetValue("title", Xunit);
        xAxis.SetValue("showgrid", false);
        xAxis.SetValue("showline", true);
        xAxis.SetValue("domain", new double[]{0.25, 0.75});

        Font Fuente1 = new();
        Fuente1.SetValue("color",ColorF);
        Font Fuente2 = new();
        Fuente2.SetValue("color", ColorD);
        Font Fuente3 = new();
        Fuente3.SetValue("color", ColorFD);
        Font FTitulo = new();
        FTitulo.SetValue("size", 18);

        LinearAxis yAxis = new();
        yAxis.SetValue("title", Yunit);
        yAxis.SetValue("showgrid", true);
        yAxis.SetValue("showline", true);
        yAxis.SetValue("linecolor", ColorF);
        yAxis.SetValue("titlefont", Fuente1);
        yAxis.SetValue("tickfont", Fuente1);
        yAxis.SetValue("side", "left");

        LinearAxis yAxis2 = new();
        yAxis2.SetValue("title", Y2unit);
        yAxis2.SetValue("showgrid", true);
        yAxis2.SetValue("showline", true);
        yAxis2.SetValue("linecolor", ColorD);
        yAxis2.SetValue("titlefont", Fuente2);
        yAxis2.SetValue("tickfont", Fuente2);
        yAxis2.SetValue("overlaying", "y");
        yAxis2.SetValue("side", "right");

        LinearAxis yAxis3 = new();
        yAxis3.SetValue("title", Y3unit);
        yAxis3.SetValue("showgrid", false);
        yAxis3.SetValue("showline", true);
        yAxis3.SetValue("linecolor", ColorFD);
        yAxis3.SetValue("titlefont", Fuente3);
        yAxis3.SetValue("tickfont", Fuente3);
        yAxis3.SetValue("overlaying", "y");
        yAxis3.SetValue("side", "right");
        yAxis3.SetValue("anchor", "free");
        yAxis3.SetValue("position", 0.8);
       
        Legend Leyenda = new();
        Leyenda.SetValue("orientation", "h");

        Title Titul = new();
        Titul.SetValue("xanchor", "center");
        Titul.SetValue("yanchor", "top");
        Titul.SetValue("x", 0.5);
        Titul.SetValue("y", 0.95);
        Titul.SetValue("font", FTitulo);

        Layout layout = new Layout();  
        layout.SetValue("title", Titul);
        layout.SetValue("xaxis", xAxis);
        layout.SetValue("yaxis", yAxis);
        layout.SetValue("yaxis2", yAxis2);
        layout.SetValue("yaxis3", yAxis3);
        layout.SetValue("legend", Leyenda);    
        layout.SetValue("showlegend", true);
        layout.SetValue("autosize", true);

        Line T1 = new();
        T1.SetValue("color", ColorF);
        Line T2 = new();
        T2.SetValue("color", ColorD);
        Line T3 = new();
        T3.SetValue("color", ColorFD);

        Trace trace1 = new Trace("scatter");
        trace1.SetValue("x", Lista[0]);
        trace1.SetValue("y", Lista[1]);
        trace1.SetValue("mode", "line");
        trace1.SetValue("name", "Funcion");
        trace1.SetValue("line", T1);
            
        Trace trace2 = new Trace("scatter");
        trace2.SetValue("x", Lista[0]);
        trace2.SetValue("y", Lista[2]);
        trace2.SetValue("mode", "line");
        trace2.SetValue("name", "Acumulacion derivada");
        trace2.SetValue("yaxis", "y2");
        trace2.SetValue("line", T2);

        Trace trace3 = new Trace("scatter");
        trace3.SetValue("x", Lista[0]);
        trace3.SetValue("y", Lista[3]);
        trace3.SetValue("mode", "line");
        trace3.SetValue("name", "Derivada");
        trace3.SetValue("yaxis", "y3");
        trace3.SetValue("line", T3);

        FSharpList<Trace> niceSharpList = ListModule.OfSeq(new List<Trace>(){trace1, trace2, trace3});
        var Configura = Config.init(ToImageButtonOptions: ToImageButtonOptions.init(Format: StyleParam.ImageFormat.SVG),
            Responsive: true, FillFrame: true, Autosizable: true);

        var retorna =  GenericChart            
            .ofTraceObjects(false, niceSharpList)
            .WithLayout(layout)
            .WithTitle(Titulo)
            .WithConfig(Configura);

        return retorna;
    }
}
