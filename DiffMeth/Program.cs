using LibDiffMeth;
using Plotly.NET;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

double[] ys = new double[100];
for (int i = 0; i < 100; i++)
{
    ys[i] = Math.Pow(i, 2);
}

Diferentiator Diff = new Diferentiator(){
    Multiplo = 10,
    ys = ys,
    Penalizada = false,
    FactorPenalty = +3.0
};
var res = Diff.MakeSplineDiff();
if (res != null) {
    var graf = Diff.Make_Graf(res);
    graf.Show();
}