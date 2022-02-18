/// <summary>
var a = 3;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Accounting
{
    public class Analytics
    {
        private readonly Dictionary<AccountType, (double ignoreBelow, double reportAbove, double percentage)>
    sanityRules = new Dictionary<AccountType, (double ignoreBelow, double reportAbove, double percentage)>
{
            { AccountType.SavingAccount, (500, 10000, 1) },
            { AccountType.Securities, (1000, 70000, 10) },
            { AccountType.OperationalAccount, (150, 150, 100) },
            { AccountType.Credit, (500, 5000, 0.5) },
            { AccountType.CurrentAccount, (70000, 50000, 100) },
            { AccountType.CashInTransit, (1, 1, 100) }
};

        public Accounting Accounting { get; set; }
        
        public class PortfolioStructure
        {
            public double SavingAccounts { get; set; }
            public double OtherAccounts { get; set; }
            public double Securities { get; set; }

            public double Deposites { get; set; }

            public double Credits { get; set; }

            public double TotalAssets { get; set; }

            public double TotalOwnAssets { get; set; }

            public double Total { get; set; }
            public object Wealth { get; internal set; }
        }

        public class StructuralData
        {
            public string Category { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
            public double Percentage { get; set; }
            public double IRR12 { get; set; }
        }

        public class EconomyStructure
        {
            public double Income { get; set; }
            public double Donations { get; set; }
            public double Consumption { get; set; }
            public double ExplicitConsumption { get; set; }
            public double Savings { get; set; }
            public double Gain { get; set; }
            public double CreditDrawing { get; set; }
            public double AssetsChange { get; internal set; }
            public double OwnAssetsChange { get; internal set; }
            public double DepositChange { get; internal set; }
            public double CommonConsumption { get; internal set; }

            public double Assets { get; internal set; }
            public double OwnAssets { get; internal set; }
            public double Deposit { get; internal set; }
            public double Credit { get; set; }
            public double Wealth { get; set; }
        }

        public class ProductData
        {
            public double Investment { get; set; }
            public double CumulativeInvestment { get; set; }
            public double PeriodCumulativeInvestment { get; set; }
            public double Value{ get; set; }
            public double? IRR6 { get; set; }
            public double? IRR12 { get; set; }
            public double? IRR36 { get; set; }
            public double? IRR60 { get; set; }
            public double? IRR120 { get; set; }
            public double? IRRMax { get; set; }
            public double NormalizedInvestment { get; internal set; }
            public List<double?> IRR12Plus { get; internal set; }
        }
        public class ListingRow
        {
            public DateTime Date{ get; set; }
            public double? Amount{ get; set; }
            public double Balance { get; set; }
            public string Currency { get; set; }
            public string PairedAccount { get; set; }
            public string Note { get; set; }
            public bool CalculatedRow{ get; set; }
        }

        public Dictionary<object, double> GetAccountGroupSum(IEnumerable<IGrouping<bool?, Account>> enumerable, DateTime date)
        {
            return enumerable.Select(x =>
           new KeyValuePair<object, double>(x.Key, x.Sum(xx => xx.GetBalanceInCZK(date))))
                .OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Value);
        }


        public Analytics(Accounting accounting)
        {
            this.Accounting = accounting;
        }

        
        public List<(DateTime Date, double Amount)> GetSuspiciousUnrecordedChanges(Account account, DateTime since,  (double ignoreBelow, double reportOver, double percentage) param)
        {
            if(account.Currency != "CZK")
            {
                param.ignoreBelow = account.CurrencyConverter.Convert(param.ignoreBelow, "CZK", account.Currency, DateTime.Now);
                param.reportOver = account.CurrencyConverter.Convert(param.reportOver, "CZK", account.Currency, DateTime.Now);
            }
            var prevMonth = DateTime.Today.AddMonths(-1);
            var uc = account.GetUnrecordedChanges(since, new DateTime(prevMonth.Year, prevMonth.Month, 1));
            var susp = uc.Where(x => Math.Abs(x.Amount) > param.ignoreBelow && (Math.Abs(x.Amount) > param.reportOver || Math.Abs(x.Amount) > Math.Abs(param.percentage / 100.0 * account.GetBalance(x.Date))));
            return susp.ToList();
        }

        /// <summary>
        /// Lists suspicious balance changes according to sanityRules
        /// </summary>
        public string GetSuspiciousUnrecordedChanges(DateTime since)
        {
            var res = new StringBuilder();
            foreach (var a in Accounting.Accounts.Where(x => x.AccountType != AccountType.Virtual))
            {
                var problems = new List<(DateTime Date, double Amount)>();
                switch (a.AccountType)
                {
                    case AccountType.CashInTransit: problems = (GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.CashInTransit])); break;
                    /*
            var prevMonth = DateTime.Today.AddMonths(-1);
            var uc = account.GetUnrecordedChanges(since, new DateTime(prevMonth.Year, prevMonth.Month, 1));
            var susp = uc.Where(x => Math.Abs(x.Amount) > param.ignoreBelow && (Math.Abs(x.Amount) > param.reportOver || Math.Abs(x.Amount) > Math.Abs(param.percentage / 100.0 * account.GetBalance(x.Date))));
            return susp.ToList();
                    */
                    case AccountType.CurrentAccount: problems = (GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.CurrentAccount])); break;
                    case AccountType.Credit: problems = GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.Credit]); break;
                    case AccountType.OperationalAccount: problems = GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.OperationalAccount]); break;
                    case AccountType.Securities: problems = GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.Securities]); break;
                    case AccountType.SavingAccount: problems = GetSuspiciousUnrecordedChanges(a, since, sanityRules[AccountType.SavingAccount]); break;
                }
                if (problems.Any())
                {
                    res.AppendLine(a.Ticker);
                    res.AppendLine(string.Join("\n", problems.Select(x => $"{x.Date.ToShortDateString()}\t {Math.Round(x.Amount, 2)}")));
                    res.AppendLine();
                }
            }
            return res.ToString();
        }
        public List<ListingRow> GetAccountListing(Account a, DateTime firstDay, DateTime lastDay)
        {
            //todo pres hrany mesicu

            var res = new List<ListingRow>();
            var balance = 0.0;

            var dt = new DateTime(firstDay.Year, firstDay.Month, 1);

            while (dt <= lastDay)
            {
                if (a.AccountType != AccountType.Virtual)
                {
                    if (dt.Day == 1) balance = a.GetBalance(dt);

                    if (dt == firstDay)
                    {
                        //pocatecni
                        res.Add(new ListingRow { Date = dt, Amount = null, Balance = balance, CalculatedRow = true, Currency = "", Note = "Počáteční zůstatek" });
                    }
                    else if (dt > firstDay && dt.Day == 1 && dt < lastDay)
                    {
                        res.Add(new ListingRow { Date = dt, Amount = null, Balance = balance, CalculatedRow = true, Currency = "", Note = "Měsíční zůstatek" });
                    }
                }
                // operace
                var ops = a.GetOperations(dt, dt);

                foreach (var o in ops)
                {
                    balance += a.CurrencyConverter.Convert(o.Amount, o.Currency.Length > 0 ? o.Currency : a.Currency, a.Currency, o.Date );
                    res.Add(new ListingRow
                    {
                        Date = dt,
                        Amount = o.Amount,
                        Balance = balance,
                        CalculatedRow = false,
                        Currency = o.Currency,
                        PairedAccount = o.PairedAccountTicker.StartsWith("_") ? o.PairedAccountTicker.Substring(1) : o.PairedAccountTicker,
                        Note = o.Note.Length > 0 ? o.Note : o.PairedOpearation != null ? o.PairedOpearation.Note : ""
                    });
                }

                if (a.AccountType != AccountType.Virtual)
                {
                    if (dt.Day == DateTime.DaysInMonth(dt.Year, dt.Month))
                    {
                        //dopocet
                        var amount = a.GetUnrecordedChanges(dt, dt).FirstOrDefault();
                        if (Math.Abs(amount.Amount) >= 1)
                        {
                            var desc = a.AccountType == AccountType.Credit || a.AccountType == AccountType.Deposit || a.AccountType == AccountType.SavingAccount ? "Úroky/Poplatky"
                                : a.AccountType == AccountType.Securities ? "Výnos /ztráta" : "Dopočtený pohyb";
                            res.Add(new ListingRow { Date = dt, Amount = amount.Amount, Balance = a.GetBalance(dt.AddDays(1)), CalculatedRow = true, Currency = "", Note = desc });
                        }

                    }

                    if (dt == lastDay)
                    {
                        //konecny
                        res.Add(new ListingRow { Date = dt, Amount = null, Balance = a.GetBalance(dt.AddDays(1)), CalculatedRow = true, Currency = "", Note = "Konečný zůstatek" });
                    }
                }
                dt = dt.AddDays(1);
            }
            return res;
        }

        public Dictionary<DateTime, PortfolioStructure> GetStructure(DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var allRes = new Dictionary<DateTime, PortfolioStructure>();
            while (date <= lastMonth)
            {
                var res = new PortfolioStructure();
                foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.SavingAccount))
                {
                    res.SavingAccounts += a.GetBalanceInCZK(date);
                }

                foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount || x.AccountType == AccountType.CashInTransit))
                {
                    res.OtherAccounts += a.GetBalanceInCZK(date);
                }

                foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Securities))
                {
                    res.Securities += a.GetBalanceInCZK(date);
                }

                foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Deposit))
                {
                    res.Deposites += a.GetBalanceInCZK(date);
                }

                foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Credit || x.AccountType == AccountType.CreditCard))
                {
                    res.Credits += a.GetBalanceInCZK(date);
                }

                res.TotalAssets = res.SavingAccounts + res.Securities + res.OtherAccounts;
                res.TotalOwnAssets = res.TotalAssets + res.Deposites;
                res.Wealth = res.TotalOwnAssets + res.Credits;
                allRes.Add(date, res);
                date = date.AddMonths(1);
              //  Console.WriteLine($"{date.ToShortDateString()} {Math.Round(res.TotalOwnAssets, 0)}");
            }
            return allRes;
        }

        public ProductData GetPerformance(IEnumerable<Account> accounts, DateTime date)
        {
            var res = new ProductData
            {
                IRR12Plus = new List<double?>()
            };

            for (int i = 12; i < 120 + 12; i++)
            {
                res.IRR12Plus.Add(GetIRR(accounts, date, i));
            }
            
            res.IRR12 = res.IRR12Plus[12 - 12];
            res.IRR36 = res.IRR12Plus[36 - 12];
            res.IRR60 = res.IRR12Plus[60 - 12];
            res.IRR120 = res.IRR12Plus[120 - 12];
            res.Value = accounts.Sum(x => x.GetBalanceInCZK(date));
            return res;
        }

        /// <summary>
        /// very aproximate XR (all buys/sells calculated for the first day of month)
        /// </summary>
        /// <returns></returns>
        public List<(DateTime Date, double RelativeQuote)> GetRelativeQuote(Account a, DateTime firstMonth, DateTime lastMonth)
        {
            //todo  co kdyz nejakou dobu bude 0? Aha?
           
            var balances = a.GetBalanceList(firstMonth, lastMonth);

            var res = new List<(DateTime Date, double RelativeQuote)>();
            var xr = 1.0;
            res.Add((firstMonth, 1));
            for(int i = 1; i < balances.Count() ; i++)
            {
                var date = firstMonth.AddMonths(i);
                var ops = a.GetOperations(date.AddMonths(-1), date);
                //Console.WriteLine($"{balances[i].Date.ToShortDateString()} {balances[i].Amount}");
                var opss = ops.Sum(x => a.CurrencyConverter.Convert(x.Amount, (x.Currency.Length > 0 ? x.Currency : a.Currency), a.Currency, x.Date));
                var inc = (balances[i - 1].Amount + opss) == 0 ? 0 : balances[i].Amount / (balances[i - 1].Amount + opss);
                 xr *= inc;
                res.Add((date, xr));
                //Console.WriteLine(xr);
                //Console.WriteLine($"{opss}");
            }
            return res;

        }

        public double GetLiquidity(Account a, DateTime date, int yearsToHold)
        {
            var start = date.AddYears(-yearsToHold);
            var xr = GetRelativeQuote(a, start, date);
            var counts= new List<(DateTime Date, double Count)>();

            var balances = a.GetBalanceList(start, date);

            counts.Add((balances[0].Date,  balances[0].Amount / xr[0].RelativeQuote));

            for (int i = 1; i < balances.Count() ; i++)
            {
                var cDate = start.AddMonths(i);
                var ops = a.GetOperations(cDate.AddMonths(-1), cDate);
                var buys = ops.Where(x => x.Amount >= 0).Sum(x => a.CurrencyConverter.Convert(x.Amount, (x.Currency.Length > 0 ? x.Currency : a.Currency), a.Currency, x.Date));
                var sells = ops.Where(x => x.Amount < 0).Sum(x => a.CurrencyConverter.Convert(x.Amount, (x.Currency.Length > 0 ? x.Currency : a.Currency), a.Currency, x.Date));

                if(sells > 0 )
                {
                    // sell oldest first 
                    throw new NotImplementedException();
                }

                counts.Add((cDate, buys / xr[i].RelativeQuote));

                
            }
            var countOlder = counts.Where(x => x.Date <= start).Sum(x => x.Count);
            var price = countOlder * xr.Last().RelativeQuote;
            return price;
        }

       
        /// <summary>
        /// price of assets held at least for the length of an investment horizon of each asset type
        /// </summary>
        public double GetHorizonLiquidity(List<Account> accounts, DateTime date)
        {
            var res = accounts.Sum(a => a.ConvertToCZK(a.Horizon > 0 ? GetLiquidity(a, date, a.Horizon) : a.GetBalance(date), date));
            return res;
        }

        private double GetLiquidity(List<Account> accounts, DateTime date, int yearsToHold)
        {
            var res = accounts.Sum(a => a.ConvertToCZK(GetLiquidity(a, date, yearsToHold), date));
            
            return res;
        }

        public Dictionary<DateTime, PortfolioStructure> GetStructure3(DateTime firstMonth, DateTime lastMonth)
        {
            var dc = new DataCollection();
            var dt = new DataTable();

            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            while (date <= lastMonth)
            {
                var sas = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.SavingAccount);
                var saBalance = sas.Sum(x => x.GetBalanceInCZK(date));
                var stocks = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Securities && x.AccountSubType == AccountSubType.Stock);
                var stocksBalance = stocks.Sum(x => x.GetBalanceInCZK(date));
                var bonds = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Securities && x.AccountSubType == AccountSubType.Bond);
                var bondsBalance = bonds.Sum(x => x.GetBalanceInCZK(date));
                var re = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Securities && x.AccountSubType == AccountSubType.RealEstates).Sum(x => x.GetBalanceInCZK(date));
                var total = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.SavingAccount || x.AccountType == AccountType.Securities || x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount).Sum(x => x.GetBalanceInCZK(date)); ;
                var money = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.SavingAccount || x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount /*|| x.AccountType == AccountType.MoneyMarket*/).Sum(x => x.GetBalanceInCZK(date)); ;
                dt.Rows.Add(
                    new Dictionary<string, object>() { 
                        { "date", date },
                        { "savings", saBalance  },
                        { "stocks", stocksBalance },
                        { "bonds", bondsBalance },
                        { "realEstates", re},
                        { "money", money},
                        { "other", total - money - stocksBalance- bondsBalance - re},
                        { "total", total}
                    });
                date = date.AddMonths(1);
            }
            dc.Tables.Add("History", dt);
            ExcelReport.Generate(@"..\..\data\template2.xlsx", dc, "d:\\out.xlsx");
            //ExcelReport.Generate("d:\\template.xlsx", dc, "d:\\out.xlsx");
           return null;
        }

        public Dictionary<DateTime, PortfolioStructure> GetStructure2(DateTime firstMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);

            var mainGroups = new[] {
                ("Spoření", Accounting.AccountTypeGroups.SavingAccounts),
                ("Akcie", Accounting.AccountTypeGroups.Stocks),
                ("Obligace", Accounting.AccountTypeGroups.Bonds),
                ("Nemovitosti", Accounting.AccountTypeGroups.RealEstates)
            };

            var total = Accounting.AccountTypeGroups.Assets.Sum(x => x.GetBalanceInCZK(date));

            var mainTable = GetBasicPerfAndLiquidity(mainGroups, date, total);
            mainTable.Rows = mainTable.Rows.OrderByDescending(x => x["Price"]).ToList();

            var dc = new DataCollection();

            var products = Accounting.AccountTypeGroups.Assets.Where(x => x.GetBalanceInCZK(date) > 999).Select( x => (x.Ticker, new List<Account> { x }));
            var productTable = GetBasicPerfAndLiquidity(products, date, total);
            productTable.Rows = productTable.Rows.OrderByDescending(x => x["Price"]).ToList();
 
            var stocksPerf = GetPerformance(Accounting.AccountTypeGroups.Stocks, date);
            var d = DataTable.PropertiesToDictionary(stocksPerf);
            var bondsPerf = GetPerformance(Accounting.AccountTypeGroups.Bonds, date);
            var sasPerf = GetPerformance(Accounting.AccountTypeGroups.SavingAccounts, date);
            var allPerf = GetPerformance(Accounting.AccountTypeGroups.InvestmentAccounts, date);
            var dt3 = new DataTable();
            for (int i = 0; i < stocksPerf.IRR12Plus.Count; i++)
            {
                dt3.Rows.Add(new Dictionary<string, object>() {
                    {"year", (i + 12.0)/ 12.0 },
                    { "stocks", stocksPerf.IRR12Plus[i] },
                    { "bonds", bondsPerf.IRR12Plus[i] },
                    { "sas", sasPerf.IRR12Plus[i] },
                    { "all", allPerf.IRR12Plus[i] },
                });
            }
            dc.Tables.Add("AssetsSummary", mainTable);
            dc.Tables.Add("Assets", productTable);
            dc.Tables.Add("IRRs", dt3);

            dc.Properties.Add("date", date);
            dc.Properties.Add("total", total);
            dc.Properties.Add("from", DateTime.Today);
            dc.Properties.Add("to", DateTime.Today.AddDays(15));

            ExcelReport.Generate(@"..\..\data\template.xlsx", dc, "d:\\out.xlsx");
            //ExcelReport.Generate("d:\\template.xlsx", dc, "d:\\out.xlsx");
            return null;
        
        }

        /// <summary>
        /// 1 - 10 yr IRRs, horizon and tax liquidity
        /// </summary>
        /// <param name="total">total price used for percentage</param>
        /// <returns></returns>
        public  DataTable GetBasicPerfAndLiquidity(IEnumerable<(string, List<Account>)> accountGroups, DateTime date, double total)
        {
            var main = accountGroups.Select(x => new
            {
                Name = x.Item1,
                Price = x.Item2.Sum(xx => xx.GetBalanceInCZK(date)),
                Percentage = x.Item2.Sum(xx => xx.GetBalanceInCZK(date)) / total,
                Irr1 = x.Item2.First().AccountType == AccountType.Securities || x.Item2.First().AccountType == AccountType.SavingAccount ?  GetIRR(x.Item2, date, 1 * 12) : null,
                Irr3 = x.Item2.First().AccountType == AccountType.Securities || x.Item2.First().AccountType == AccountType.SavingAccount ? GetIRR(x.Item2, date, 3 * 12) : null,
                Irr5 = x.Item2.First().AccountType == AccountType.Securities || x.Item2.First().AccountType == AccountType.SavingAccount ? GetIRR(x.Item2, date, 5 * 12) : null,
                Irr7 = x.Item2.First().AccountType == AccountType.Securities || x.Item2.First().AccountType == AccountType.SavingAccount ? GetIRR(x.Item2, date, 7 * 12) : null,
                Irr10 = x.Item2.First().AccountType == AccountType.Securities || x.Item2.First().AccountType == AccountType.SavingAccount ? GetIRR(x.Item2, date, 10 * 12) : null,
                TaxLiquidity = x.Item2.First().AccountType == AccountType.Securities  ? GetLiquidity(x.Item2, date, 3) : x.Item2.Sum( xx => xx.GetBalanceInCZK(date)),
                HorizonLiquidity = GetHorizonLiquidity(x.Item2, date)
            });
           return new DataTable(main.Select(x => DataTable.PropertiesToDictionary(x)));
        }

        
        public List<(DateTime, StructuralData)> GetStructureEx(DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var allRes = new List<(DateTime, StructuralData)>();
            while (date <= lastMonth)
            {
                PortfolioStructure res = GetStructByDepositType(date);
                allRes.Add((date, new StructuralData { Category = "AssetType", Name = "Peníze", Value = res.OtherAccounts, Percentage = res.OtherAccounts / res.TotalAssets, IRR12 = 0 }));
                allRes.Add((date, new StructuralData { Category = "AssetType", Name = "Spořící účty", Value = res.SavingAccounts, Percentage = res.SavingAccounts / res.TotalAssets, IRR12 = 0 }));
                allRes.Add((date, new StructuralData { Category = "AssetType", Name = "Cenné papíry", Value = res.Securities, Percentage = res.Securities / res.TotalAssets, IRR12 = 0 }));

                foreach (var a in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.CashInTransit || x.Value.AccountType == AccountType.CurrentAccount || x.Value.AccountType == AccountType.OperationalAccount || x.Value.AccountType == AccountType.SavingAccount || x.Value.AccountType == AccountType.Securities))
                {
                    var irr = 0.0;
                    if(a.Value .AccountType == AccountType.SavingAccount || a.Value.AccountType == AccountType.Securities)
                    {
                        irr = GetIRR(a.Value, date, 12) ?? 0;
                    }
                    allRes.Add((date, new StructuralData { Category = "Product", Name = a.Value.Ticker, Value = a.Value.GetBalanceInCZK(date), Percentage = a.Value.GetBalanceInCZK(date) / res.TotalAssets, IRR12 = irr }));
                }

                var curstruct = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.CashInTransit || x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount || x.AccountType == AccountType.SavingAccount || x.AccountType == AccountType.Securities)
                    .GroupBy(x => x.Currency);

                foreach (var c in curstruct)
                {
                    allRes.Add((date, new StructuralData
                    {
                        Category = "Currency",
                        Name = c.Key,
                        Value = c.Sum(x => x.GetBalanceInCZK(date)),
                        Percentage = c.Sum(x => x.GetBalanceInCZK(date)) / res.TotalAssets
                    }));
                }

                var liqStruct = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.CashInTransit || x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount || x.AccountType == AccountType.SavingAccount || x.AccountType == AccountType.Securities)
                  .GroupBy(x => x.Liquidity).OrderBy(x => x.Key);
                foreach (var c in liqStruct)
                {
                    var name = c.Key == 0 ? "Okamžitá" : c.Key >= 365 ? "Rok" : c.Key + " dní";

                    allRes.Add((date, new StructuralData
                    {
                        Category = "Liquidity",
                        Name = name,
                        Value = c.Sum(x => x.GetBalanceInCZK(date)),
                        Percentage = c.Sum(x => x.GetBalanceInCZK(date)) / res.TotalAssets
                    }));
                }

                var tatrgStruct = Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.CashInTransit || x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount || x.AccountType == AccountType.SavingAccount || x.AccountType == AccountType.Securities)
                        .GroupBy(x => x.AccountSubType).OrderBy(x => x.Key);
                foreach (var c in tatrgStruct)
                {
                    var name = "";
                    switch (c.Key)
                    {
                        case AccountSubType.Bond: name = "Obligace"; break;
                        case AccountSubType.Stock: name = "Akcie"; break;
                        case AccountSubType.RealEstates: name = "Nemovitosti"; break;
                        case AccountSubType.Mixed: name = "Smíšené"; break;
                        default: name = "Peníze"; break;

                    }

                    allRes.Add((date, new StructuralData
                    {
                        Category = "Targetting",
                        Name = name,
                        Value = c.Sum(x => x.GetBalanceInCZK(date)),
                        Percentage = c.Sum(x => x.GetBalanceInCZK(date)) / res.TotalAssets
                    }));
                }
                date = date.AddMonths(1);
                //  Console.WriteLine($"{date.ToShortDateString()} {Math.Round(res.TotalOwnAssets, 0)}");
            }
            return allRes;
        }

        private PortfolioStructure GetStructByDepositType(DateTime date)
        {
            var res = new PortfolioStructure();
            foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.SavingAccount))
            {
                res.SavingAccounts += a.GetBalanceInCZK(date);
            }

            foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.CurrentAccount || x.AccountType == AccountType.OperationalAccount || x.AccountType == AccountType.CashInTransit))
            {
                res.OtherAccounts += a.GetBalanceInCZK(date);
            }

            foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Securities))
            {
                res.Securities += a.GetBalanceInCZK(date);
            }

            foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Deposit))
            {
                res.Deposites += a.GetBalanceInCZK(date);
            }

            foreach (var a in Accounting.AccountsDict.Values.Where(x => x.AccountType == AccountType.Credit || x.AccountType == AccountType.CreditCard))
            {
                res.Credits += a.GetBalanceInCZK(date);
            }

            res.TotalAssets = res.SavingAccounts + res.Securities + res.OtherAccounts;
            res.TotalOwnAssets = res.TotalAssets + res.Deposites;
            return res;
        }

        public Dictionary<DateTime, EconomyStructure> GetEconomy(DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var structure = GetStructure(firstMonth, lastMonth);
            var allRes = new Dictionary<DateTime, EconomyStructure>();
            while (date < lastMonth)
            {
                var creditCard = 0.0;
                var res = new EconomyStructure
                {
                    Income = -Accounting.AccountsDict["_Příjem"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount),
                    Donations = -Accounting.AccountsDict["_Dar"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount),
                    ExplicitConsumption = Accounting.AccountsDict["_Spotřeba"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount)
                };
                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.Credit))
                {
                    //todo foreign
                    res.CreditDrawing += -cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount);
                }

                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.CreditCard))
                {
                    //todo foreign
                //    var tmp3 = cr.Value.Operations.Where(x => x.Date >= date);// && x.Date < date.AddMonths(1)).Sum(x => x.Amount); ;
                    var tmp = cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount); ;
              //      res.CreditDrawing += -tmp;
                    creditCard += tmp;
                }

                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.Deposit))
                {
                    //todo foreign
                    res.DepositChange += -cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount);
                }

                foreach (var sa in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.SavingAccount || x.Value.AccountType == AccountType.Securities))
                {
                    var g = GetGain(sa.Value, date, 1);
                    res.Gain += sa.Value.CurrencyConverter.Convert(g, sa.Value.Currency, "CZK", date.AddMonths(1));
                }

                var currentAccountGain = 0.0;
                foreach (var sa in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.CurrentAccount || x.Value.AccountType == AccountType.OperationalAccount ))
                {
                    var g = GetGain(sa.Value, date, 1);
                    currentAccountGain += sa.Value.CurrencyConverter.Convert(g, sa.Value.Currency, "CZK", date.AddMonths(1));
                }
                res.AssetsChange = structure[date.AddMonths(1)].TotalAssets - structure[date].TotalAssets;
                res.OwnAssetsChange = structure[date.AddMonths(1)].TotalOwnAssets - structure[date].TotalOwnAssets;
                //res.Consumption = res.Income + res.Donations + res.Gain - res.AssetsChange + res.CreditDrawing + res.DepositChange + creditCard;
                res.Consumption = res.ExplicitConsumption - currentAccountGain + creditCard;
                res.CommonConsumption = res.Consumption - res.ExplicitConsumption ;
                
                res.Savings = res.Income + res.Donations + res.CreditDrawing - res.Consumption;
                allRes.Add(date, res);
                date = date.AddMonths(1);
                Console.WriteLine($"{date.ToShortDateString()} {Math.Round(res.Income, 0)}");
            }
            return allRes;
        }

        public Dictionary<DateTime, EconomyStructure> GetEconomy2(DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var structure = GetStructure(firstMonth, lastMonth);
            var allRes = new Dictionary<DateTime, EconomyStructure>();
            while (date < lastMonth)
            {
                var creditCard = 0.0;
                var res = new EconomyStructure
                {
                    Income = -Accounting.AccountsDict["_Příjem"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount),
                    Donations = -Accounting.AccountsDict["_Dar"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount),
                    ExplicitConsumption = Accounting.AccountsDict["_Spotřeba"].Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount)
                };
                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.Credit))
                {
                    //todo foreign
                    res.CreditDrawing += -cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount);
                }

                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.CreditCard))
                {
                    //todo foreign
                    //    var tmp3 = cr.Value.Operations.Where(x => x.Date >= date);// && x.Date < date.AddMonths(1)).Sum(x => x.Amount); ;
                    var tmp = cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount); ;
                    //      res.CreditDrawing += -tmp;
                    creditCard += tmp;
                }

                foreach (var cr in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.Deposit))
                {
                    //todo foreign
                    res.DepositChange += -cr.Value.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => x.Amount);
                }

                foreach (var sa in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.SavingAccount || x.Value.AccountType == AccountType.Securities))
                {
                    var g = GetGain(sa.Value, date, 1);
                    res.Gain += sa.Value.CurrencyConverter.Convert(g, sa.Value.Currency, "CZK", date.AddMonths(1));
                }

                var currentAccountGain = 0.0;
                foreach (var sa in Accounting.AccountsDict.Where(x => x.Value.AccountType == AccountType.CurrentAccount || x.Value.AccountType == AccountType.OperationalAccount))
                {
                    var g = GetGain(sa.Value, date, 1);
                    currentAccountGain += sa.Value.CurrencyConverter.Convert(g, sa.Value.Currency, "CZK", date.AddMonths(1));
                }
                res.AssetsChange = structure[date.AddMonths(1)].TotalAssets - structure[date].TotalAssets;
                res.OwnAssetsChange = structure[date.AddMonths(1)].TotalOwnAssets - structure[date].TotalOwnAssets;
                //res.Consumption = res.Income + res.Donations + res.Gain - res.AssetsChange + res.CreditDrawing + res.DepositChange + creditCard;
                res.Consumption = res.ExplicitConsumption - currentAccountGain + creditCard;
                res.CommonConsumption = res.Consumption - res.ExplicitConsumption;

                res.Savings = res.Income + res.Donations + res.Gain - res.Consumption;
                allRes.Add(date, res);
                date = date.AddMonths(1);
                Console.WriteLine($"{date.ToShortDateString()} {Math.Round(res.Income, 0)}");
            }
            return allRes;
        }

        public Dictionary<DateTime, ProductData> GetInvestmentProductHistory(Account a, DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var allRes = new Dictionary<DateTime, ProductData>();
            var prevInvested = a.Operations.Where(x => x.Date < date).Sum(x => a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, a.Currency, x.Date));
        var normalizedInvested =  a.GetBalance(firstMonth);
            var periodInvest = 0.0;
            while (date < lastMonth)
            {
                //var invl = a.Operations.Where(x => x.Date >= date && x.Date < x.Date.AddMonths(1));
                var inv = a.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, a.Currency, x.Date));
                prevInvested += inv;
                periodInvest += inv;
                normalizedInvested += inv;
                var res = new ProductData
                {
                    Value = a.GetBalance(date.AddMonths(1)),
                    Investment = inv,
                    NormalizedInvestment = normalizedInvested,
                    PeriodCumulativeInvestment = periodInvest,
                    IRR6 = GetIRR(a, date.AddMonths(1), 6),
                    IRR12 = GetIRR(a, date.AddMonths(1), 12),
                    IRR36 = GetIRR(a, date.AddMonths(1), 36),
                    IRR60 = GetIRR(a, date.AddMonths(1), 60),
                    IRR120 = GetIRR(a, date.AddMonths(1), 120),
                    IRRMax = GetIRR(a, date.AddMonths(1), 0),
                    CumulativeInvestment = prevInvested
                };

                allRes.Add(date.AddMonths(1).AddDays(-1), res);
                date = date.AddMonths(1);
            }
            return allRes;
        }

        public Dictionary<DateTime, ProductData> GetPortfolioHistory(DateTime firstMonth, DateTime lastMonth)
        {
            var date = new DateTime(firstMonth.Year, firstMonth.Month, 1);
            var prevInvested = 0.0;
            var normalizedInvested = 0.0;
            var periodInvest = 0.0;
            var allRes = new Dictionary<DateTime, ProductData>();
            foreach( var a in Accounting.AccountTypeGroups.InvestmentAccounts)
            {
                prevInvested += a.Operations.Where(x => x.Date < date).Sum(x => a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, "CZK", x.Date));
                normalizedInvested += a.GetBalanceInCZK(firstMonth);
            }
            while (date < lastMonth)
            {
                var inv = 0.0;
                foreach (var a in Accounting.AccountTypeGroups.InvestmentAccounts)
                {
                    inv += a.Operations.Where(x => x.Date >= date && x.Date < date.AddMonths(1)).Sum(x => a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, "CZK", x.Date));
                }

                prevInvested += inv;
                periodInvest += inv;
                normalizedInvested += inv;
                var res = new ProductData
                {
                    Value = GetInvestmentBalance(date.AddMonths(1)),
                    //Value = a.GetBalance(date.AddMonths(1)),
                    Investment = inv,
                    NormalizedInvestment = normalizedInvested,
                    PeriodCumulativeInvestment = periodInvest,
                    IRR6 =  GetInvestmentIRR(date.AddMonths(1), 6),
                    IRR12 = GetInvestmentIRR(date.AddMonths(1), 12),
                    IRR36 = GetInvestmentIRR(date.AddMonths(1), 36),
                    IRR60 = GetInvestmentIRR(date.AddMonths(1), 60),
                    IRR120 =GetInvestmentIRR(date.AddMonths(1), 120),
                    IRRMax =GetInvestmentIRR(date.AddMonths(1), 0),
                    CumulativeInvestment = prevInvested
                };

                allRes.Add(date.AddMonths(1).AddDays(-1), res);
                date = date.AddMonths(1);
            }
            return allRes;
        }

        private double GetInvestmentBalance(DateTime dateTime)
        {
            return Accounting.AccountTypeGroups.InvestmentAccounts.Sum(x => x.GetBalanceInCZK(dateTime));
        }

        private double GetBalance(IEnumerable<Account> accounts, DateTime dateTime)
        {
            return accounts.Sum(x => x.GetBalanceInCZK(dateTime));
        }
        
       // in currency
        public double? GetIRR(Account a, DateTime date, int months)
        {
            if(months == 0)
            {
                months = 12 * 100;
            }
            else
            {
                if (a.GetBalance(date.AddMonths(-months)) == 0 && !a.Operations.Where(x => x.Date < date.AddMonths(-months + 1)).Any()) return null;
            }

            var cashflow2 = a.Operations.Where(x => x.Date < date && x.Date >= date.AddMonths(-months))
                .Select(x => new KeyValuePair<DateTime, double>(x.Date, a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, a.Currency, x.Date)));
            var cashflow = cashflow2
                .GroupBy(x => x.Key)
                .Select(x => new KeyValuePair<DateTime, double> (x.Key, x.Sum(y => y.Value)))
                .ToDictionary(x => x.Key, x => x.Value);
            if (a.GetBalance(date.AddMonths(-months)) > 0)
                {

                if (cashflow.ContainsKey(date.AddMonths(-months)))
                {
                    cashflow[date.AddMonths(-months)] += GetInvestmentBalance(date.AddMonths(-months));
                }

                else
                {
                    var beg = new Dictionary<DateTime, double> { { date.AddMonths(-months), a.GetBalance(date.AddMonths(-months)) } };
                    cashflow = beg.Concat(cashflow).ToDictionary(x => x.Key, x => x.Value); ;

                }
            }
            var irr = IRR(cashflow, date, a.GetBalance(date));
            return irr;
        }
        
        //todo in what currency???
        public double? GetInvestmentIRR( DateTime date, int months)
        {
            var pointZero = new DateTime(1991, 1, 1);
            if (months == 0)
            {
                months = date.Month - pointZero.Month + 12 * (date.Year - pointZero.Year);
            }
            var cashflow = new Dictionary<DateTime, double>();
            foreach(var a in Accounting.AccountTypeGroups.InvestmentAccounts)
            {
                var cf = a.Operations.Where(x => x.Date < date && x.Date >= date.AddMonths(-months))
                    .Select(x => new KeyValuePair<DateTime, double>(x.Date, a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, "CZK", x.Date)));

                foreach(var c in cf)
                {
                    if(cashflow.ContainsKey(c.Key))
                    {
                        cashflow[c.Key] += c.Value;
                    }
                    else
                    {
                        cashflow[c.Key] = c.Value;
                    }
                }
            }
            cashflow = cashflow.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            if (GetInvestmentBalance(date.AddMonths(-months)) > 0)
            {

                if (cashflow.ContainsKey(date.AddMonths(-months)))
                {
                    cashflow[date.AddMonths(-months)] += GetInvestmentBalance(date.AddMonths(-months));
                }
                else
                {
                    var beg = new Dictionary<DateTime, double> { { date.AddMonths(-months), GetInvestmentBalance(date.AddMonths(-months)) } };
                    cashflow = beg.Concat(cashflow).ToDictionary(x => x.Key, x => x.Value); ;

                }
            }
            var irr = IRR(cashflow, date, GetInvestmentBalance(date));
            return irr;
        }

        /// <summary>
        /// IRR of one or more assets (converted to CZK, ie. including currency gain/loss)
        /// </summary>
        public double? GetIRR(IEnumerable<Account> accounts, DateTime date, int months)
        {
            var pointZero = new DateTime(1991, 1, 1);
            if (months == 0)
            {
                months = date.Month - pointZero.Month + 12 * (date.Year - pointZero.Year);
            }
            
            if (GetBalance(accounts, date.AddMonths(-months)) == 0) return null;

            var cashflow = new Dictionary<DateTime, double>();
            foreach (var a in accounts)
            {
                var cf = a.Operations.Where(x => x.Date < date && x.Date >= date.AddMonths(-months))
                    .Select(x => new KeyValuePair<DateTime, double>(x.Date, a.ConvertToCZK(x.Amount, x.Currency, x.Date)));

              //  Console.WriteLine(a.Ticker);
                foreach (var c in cf)
                {
                //    Console.WriteLine(c.Key + ": " + c.Value);
                    if (cashflow.ContainsKey(c.Key))
                    {
                        cashflow[c.Key] += c.Value;
                    }
                    else
                    {
                        cashflow[c.Key] = c.Value;
                    }
                }
            }
            cashflow = cashflow.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            if (GetBalance(accounts, date.AddMonths(-months)) > 0)
            {

                if (cashflow.ContainsKey(date.AddMonths(-months)))
                {
                    cashflow[date.AddMonths(-months)] += GetBalance(accounts, date.AddMonths(-months));
                }
                else
                {
                    var beg = new Dictionary<DateTime, double> { { date.AddMonths(-months), GetBalance(accounts, date.AddMonths(-months)) } };
                    cashflow = beg.Concat(cashflow).ToDictionary(x => x.Key, x => x.Value); ;

                }
            }
            var irr = IRR(cashflow, date, GetBalance(accounts, date));
            return irr;
        }
  
        //todo currency?
        public static double GetGain(Account a, DateTime firstMonth, int months = 1)
        {
            var d = a.GetBalance(firstMonth.AddMonths(months)) - a.GetBalance(firstMonth);
            //todo currencies
            var res = d - a.GetOperations(firstMonth, firstMonth.AddMonths(months).AddDays(-1)).Sum(x => a.CurrencyConverter.Convert(x.Amount, x.Currency.Length > 0 ? x.Currency : a.Currency, a.Currency, x.Date));
            return res;
        }

        ///<summary> IRR calculation algo</summary>
        static public double? IRR(Dictionary<DateTime, double> cashflow, DateTime date, double value)
        {
            if (!cashflow.Any()) return null;
            double max = 10;
            double min = -1;
            int cycles = 1000;
            double x = (max + min) / 2.0;

            for (int i = 0; i < cycles; i++)
            {
                double fv = FV(cashflow, x, date);
                if (fv > value)
                {
                    max = x;
                    x = (max + min) / 2.0;
                }
                else
                {
                    min = x;
                    x = (max + min) / 2.0;
                }
                if (max - min < 0.00001) break;
            }

            return x;
        }
       
        /// <summary>
        /// FV algo
        /// </summary>
        static public double FV(Dictionary<DateTime, double> cashflow, double ir, DateTime d)
        {
            double res = 0;
            foreach (var x in cashflow)
            {
                double diff = (d - x.Key).TotalDays / 365.25;
                res += Math.Pow( 1 + ir, diff) * x.Value;
            }
            return res;
        }
    }
}
