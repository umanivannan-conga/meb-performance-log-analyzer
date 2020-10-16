using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;

namespace LogAnalyzer
{
  class Program
  {
    static void Main(string[] args)
    {
      var d = new DirectoryInfo(args[0]);
      var files = d.GetFiles("*.log");
      var stats = new List<LogStats>();
      foreach (var fileInfo in files)
      {
        try
        {
          var stat = new LogStats { FileName = fileInfo.FullName };
          var lines = File.ReadAllLines(fileInfo.FullName);


          foreach (var line in lines)
          {
            if (!line.Contains("|USER_DEBUG|")) continue;

            var logParts = line.Split("|");

            var message = logParts.LastOrDefault();

            if (message != null && message.StartsWith("Total", StringComparison.InvariantCulture))
            {
              var messageParts = message.Split("-");

              switch (messageParts[0].Trim())
              {
                case "Total SOQL queries":
                  stat.TotalSoqlQueries = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
                case "Total records retrieved by SOQL queries":
                  stat.TotalRecordsRetreivedBySoqlQueries = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
                case "Total number of DML statements issued":
                  stat.TotalNoOfDmlIssued = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
                case "Total number of DML rows processed":
                  stat.TotalNoOfDmlProcessed = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
                case "Total heap size (in bytes)":
                  stat.TotalHeapSizeInBytes = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
                case "Total CPU time (in milliseconds)":
                  stat.TotalCpuTimeInMs = Convert.ToInt64(messageParts[1].Split("out of")[0].Trim());
                  break;
              }
            }

            if (message != null && message.StartsWith("Finished processing", StringComparison.InvariantCulture))
            {
              stat.Summary = message;
            }
            if (message != null && message.StartsWith("ScheduleId - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.ScheduleId = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("sectionsCount - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.SectionsCount = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("getSectionsData - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.GetSectionsData = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("getProductPriceRuleMap - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.GetProductPriceRuleMap = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("getAccountParticipantMap - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.GetAccountParticipantMap = message.Split("-")[1].Trim();
            }

            if (message != null && message.StartsWith("getPriceRuleSets - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.GetPriceRuleSets = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("getTransactionLines - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.GetTransactionLines = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("deleteRecordsForPayoutSchedules - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.DeleteRecordsForPayoutSchedules = message.Split("-")[1].Trim();
            }
            if (message != null && message.StartsWith("bucketTransactionsByPriceRules - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.BucketTransactionsByPriceRules = message.Split("-")[1].Trim();
            }

            if (message != null && message.StartsWith("processQualificationAndBenefitLines - ", StringComparison.InvariantCulture))
            {
              if (message.Split("-").Length > 0)
                stat.ProcessQualificationAndBenefitLines = message.Split("-")[1].Trim();
            }
          }
          stats.Add(stat);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error Processing " + fileInfo.Name);
          Console.WriteLine(ex.Message);
        }

      }

      List<String> csvLines = new List<string>
      {
        //"Run Summary",
        //"Total SOQL queries," + stats.Average(item => item.TotalSoqlQueries),
        //"Total records retrieved by SOQL queries," + stats.Average(item => item.TotalRecordsRetreivedBySoqlQueries),
        //"Total records retrieved by Database.getQueryLocator," + stats.Average(item => item.TotalRecordsRetrivedBySoqlDatabaseQueryLocator),
        //"Total number of DML statements issued," + stats.Average(item => item.TotalNoOfDMLIssued),
        //"Total number of DML rows processed," + stats.Average(item => item.TotalNoOfDMLProcessed),
        //"Total heap size (in bytes)," + stats.Average(item => item.TotalHeapSizeInBytes),
        //"Total CPU time (in milliseconds)," + stats.Average(item => item.TotalCPUTimeInMs),
        //""
      };
      stats = stats.OrderBy(item => item.ScheduleId).ToList();

      foreach (var stat in stats)
      {
        csvLines.Add("File name," + stat.FileName);
        var summary = stat.Summary != null
          ? stat.Summary.Replace(",", "|")
          : "No Summary found in file";
        csvLines.Add("Summary," + summary);
        csvLines.Add("ScheduleId," + stat.ScheduleId);
        csvLines.Add("Total SOQL queries," + stat.TotalSoqlQueries);
        csvLines.Add("Total records retrieved by SOQL queries," + stat.TotalRecordsRetreivedBySoqlQueries);
        csvLines.Add("Total number of DML statements issued," + stat.TotalNoOfDmlIssued);
        csvLines.Add("Total number of DML rows processed," + stat.TotalNoOfDmlProcessed);
        csvLines.Add("Total heap size (in bytes)," + stat.TotalHeapSizeInBytes);
        csvLines.Add("Total CPU time (in milliseconds)," + stat.TotalCpuTimeInMs);

        csvLines.Add("SectionCount," + stat.SectionsCount);
        csvLines.Add("getSectionsData," + stat.GetSectionsData);
        csvLines.Add("getPriceRuleSets," + stat.GetPriceRuleSets);
        csvLines.Add("getProductPriceRuleMap," + stat.GetProductPriceRuleMap);
        csvLines.Add("getAccountParticipantMap," + stat.GetAccountParticipantMap);
        csvLines.Add("getTransactionLines," + stat.GetTransactionLines);

        csvLines.Add("deleteRecordsForPayoutSchedules," + stat.DeleteRecordsForPayoutSchedules);
        csvLines.Add("bucketTransactionsByPriceRules," + stat.BucketTransactionsByPriceRules);
        csvLines.Add("processQualificationAndBenefitLines," + stat.ProcessQualificationAndBenefitLines);


      }

      //File.WriteAllLines(d.FullName + "/" + Path.GetFileNameWithoutExtension(d.Name) + ".csv", csvLines);
      WriteCsv(stats, d.FullName + "/" + Path.GetFileNameWithoutExtension(d.Name) + ".csv");

    }

    public static void WriteCsv<T>(IEnumerable<T> items, string path)
    {
      Type itemType = typeof(T);
      var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

      using (var writer = new StreamWriter(path))
      {
        writer.WriteLine(string.Join(", ", props.Select(p => p.Name)));

        foreach (var item in items)
        {
          writer.WriteLine(string.Join(", ", props.Select(p => p.GetValue(item, null))));
        }
      }
    }
  }

  public class LogStats
  {

    public string ScheduleId { get; set; }

    public long TotalSoqlQueries { get; set; }
    public long TotalRecordsRetreivedBySoqlQueries { get; set; }
    public long TotalNoOfDmlIssued { get; set; }
    public long TotalNoOfDmlProcessed { get; set; }
    public long TotalHeapSizeInBytes { get; set; }
    public long TotalCpuTimeInMs { get; set; }

    public string SectionsCount { get; set; }
    public string GetSectionsData { get; set; }
    public string GetPriceRuleSets { get; set; }
    public string GetAccountParticipantMap { get; set; }
    public string GetProductPriceRuleMap { get; set; }
    public string GetTransactionLines { get; set; }
    public string ProcessQualificationAndBenefitLines { get; set; }
    public string BucketTransactionsByPriceRules { get; set; }
    public string DeleteRecordsForPayoutSchedules { get; set; }
    public string FileName { get; set; }
    public string Summary { get; set; }
  }
}
