using LibDiffMeth;
using Plotly.NET;

const int Iteraciones = 500;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

double[] ys = new double[Iteraciones];
for (int i = 0; i < Iteraciones; i++)
{
    ys[i] = Math.Sin(i * (Math.PI /180));
}
Diferentiator Diff = new Diferentiator(){
    Multiplo = 1,
    ys = ys,
    Penalizada = false,
    FactorPenalty = +3.0,
    TipoExtremos = "p"  //"n" => natural    "p" => parabólicos (por defecto)
};
var res = Diff.MakeSplineDiff();
if (res != null) {
    var graf = Diff.Make_Graf(res);
    graf.SaveHtml("graf.html", true);
}
