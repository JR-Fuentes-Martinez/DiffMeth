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

    public List<double[]>? MakeSplineDiff()
    {
        List<double[]> Retorna = new List<double[]>(3);

        if (ys == null || (Penalizada && (FactorPenalty < -15.0 || FactorPenalty > 15.0))) {
            return null;
        }
        try {
            double[] xs = new double[ys.Length];
            double[] xs2 = new double[ys.Length * Multiplo];

            for (int i = 0; i < ys.Length; i++)
            {
                xs[i] = i + 1;
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

            alglib.spline1dconvdiffcubic(xs, ys, xs.Length, 0, 0.0, 0, 0.0, xs2, xs2.Length, out f1, out d1);

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

            return Retorna;
        }
        catch {
             return null;
        }       
    }
    public GenericChart.GenericChart Make_Graf(List<double[]> Lista) {

        string Xunit, Yunit, Y2unit, Titulo;

        Yunit = "u(y)";
        Y2unit = "Acum [u(y) : u(x)]";
        Titulo = "Funcion y acumulacion en base a derivada";        
        Xunit = "u(x)";

        string ColorF = "blue";
        string ColorD = "maroon";
        
        LinearAxis xAxis = new();
        xAxis.SetValue("title", $"{Xunit}");
        xAxis.SetValue("showgrid", false);
        xAxis.SetValue("showline", true);

        Font Fuente1 = new();
        Fuente1.SetValue("color",ColorF);
        Font Fuente2 = new();
        Fuente2.SetValue("color", ColorD);
        Font FTitulo = new();
        FTitulo.SetValue("size", 14);

        LinearAxis yAxis = new();
        yAxis.SetValue("title", $"{Yunit}");
        yAxis.SetValue("showgrid", false);
        yAxis.SetValue("showline", true);
        yAxis.SetValue("linecolor", ColorF);
        yAxis.SetValue("titlefont", Fuente1);
        yAxis.SetValue("tickfont", Fuente1);
        yAxis.SetValue("side", "left");

        LinearAxis yAxis2 = new();
        yAxis2.SetValue("title", $"{Y2unit}");
        yAxis2.SetValue("showgrid", false);
        yAxis2.SetValue("showline", true);
        yAxis2.SetValue("linecolor", ColorD);
        yAxis2.SetValue("titlefont", Fuente2);
        yAxis2.SetValue("tickfont", Fuente2);
        yAxis2.SetValue("overlaying", "y");
        yAxis2.SetValue("side", "right");

        Legend Leyenda = new();
        Leyenda.SetValue("orientation", "h");

        Title Titul = new();
        Titul.SetValue("xanchor", "center");
        Titul.SetValue("yanchor", "top");
        Titul.SetValue("x", 0.5);
        Titul.SetValue("y", 0.9);
        Titul.SetValue("font", FTitulo);

        Layout layout = new Layout();  
        layout.SetValue("title", Titul);
        layout.SetValue("xaxis", xAxis);
        layout.SetValue("yaxis", yAxis);
        layout.SetValue("yaxis2", yAxis2);
        layout.SetValue("legend", Leyenda);    
        layout.SetValue("showlegend", true);

        Line T1 = new();
        T1.SetValue("color", ColorF);
        Line T2 = new();
        T2.SetValue("color", ColorD);

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
        trace2.SetValue("name", "Derivada");
        trace2.SetValue("yaxis", "y2");
        trace2.SetValue("line", T2);

        FSharpList<Trace> niceSharpList = ListModule.OfSeq(new List<Trace>(){trace1, trace2});
        var Configura = Config.init(ToImageButtonOptions: ToImageButtonOptions.init(Format: StyleParam.ImageFormat.SVG));

        var retorna =  GenericChart
            .ofTraceObjects(true, niceSharpList)
            .WithLayout(layout)
            .WithTitle(Titulo)
            .WithConfig(Configura);
        
        return retorna;
    }
}
