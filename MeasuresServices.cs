using System.Data;
using AspNet.Models;
using Utils;

public class MeasuresServices
{
    public static IEnumerable<MeasureViewModel> GetMeasures(string product, DateTime from, DateTime to)
    {
        using var db = new balanceContext();
        var measures = db.Izmervanes
            .Select(i => new MeasureViewModel()
            {
                Kanbel = i.Kanbel,
                Dtime = i.Dtime,
                TruckId = i.Truck.Id,
                TruckPlvl = i.Truck.Plvl,
                Plrem = i.Plrem,
                Bruto = i.Bruto,
                Brutotm = i.Brutotm,
                Tara = i.Tara,
                Taratm = i.Taratm,
                Neto = i.Neto,
                VodId = i.Vodmp.Id,
                Name = i.Vodmp.Name,
                Sname = i.Vodmp.Fam,
                Fam = i.Vodmp.Sname,
                CompanyId = i.Company.Id,
                CompanyName = i.Company.Name,
                Cantnomer = i.Cantnomer,
                ProductName = i.Product.ProductName
            })
            .Where(i => i.ProductName.Contains(product))
            .Where(i => i.Brutotm >= from)
            .Where(i => i.Brutotm <= to.AddDays(1))  //users input days
            .Where(i => i.Tara > 0)
            .Where(i => i.Bruto > 0)
            .ToList();

        return measures;
    }

    public static IEnumerable<ProductViewModel> GetProducts()
    {
        using var db = new balanceContext();
        var products = db.Products
            .Select(p => new ProductViewModel()
            {
                Name = p.ProductName
            })
            .ToList();

        return products;
    }

    public static Dictionary<string, Dictionary<string, string>> GetTrucksPlannedMonthly()
    {
        string planXlxFile = Directory.GetFiles(Speditor.opisPath, Speditor.planFile).FirstOrDefault();
        var warnings = new Dictionary<string, List<string>>();

        if (planXlxFile != null)
        {
            var worksheets = Excel.ReadWithEPPlus<List<DataTable>>(planXlxFile);
            var trucksPlannedMonthly = new Dictionary<string, Dictionary<string, string>>();
            foreach (var ws in worksheets)
            {
                //ws = worksheets.Where(w => w.TableName == today).FirstOrDefault();
                var trucksPlannedDaily = new Dictionary<string, string>();

                foreach (DataRow dataRow in ws.Rows)
                {
                    try
                    {
                        var plRem = TextFile.ReplaceCyrillic(dataRow.Field<string>("??????????????")?.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpper().Trim());
                        var destination = dataRow.Field<string>("????????.??????????");

                        if (!trucksPlannedDaily.ContainsKey(plRem))
                            trucksPlannedDaily.Add(plRem, destination);
                        else
                        {
                            string warning = $"?????????????? ?? ??? {plRem} ?? ?????????????????? ???????????? ???? 1 ?????? ???? {ws.TableName}";
                            if (!warnings.ContainsKey(ws.TableName))
                            {
                                warnings.Add(ws.TableName, new List<string>());
                            }
                            warnings[ws.TableName].Add(warning);
                        }
                    }
                    catch (NullReferenceException nre)
                    {
                        TextFile.Log(DateTime.Now.ToString() + " " + nre.Message);
                    }
                }

                trucksPlannedMonthly.Add(ws.TableName, trucksPlannedDaily);
            }
            return trucksPlannedMonthly;
        }
        else
        {
            return null;
        }
    }
}