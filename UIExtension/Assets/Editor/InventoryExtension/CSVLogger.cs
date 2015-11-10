using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public enum EmptyLineBehavior
{
	NoColumns,
	EmptyColumn,
	Ignore,
	EndOfFile,
}

public abstract class CsvFileCommon
{
	protected char[] SpecialChars = new char[] { ';', '"', '\r', '\n' };
	
	private const int DelimiterIndex = 0;
	private const int QuoteIndex = 1;
	
	public char Delimiter
	{
		get { return SpecialChars[DelimiterIndex]; }
		set { SpecialChars[DelimiterIndex] = value; }
	}
	
	public char Quote
	{
		get { return SpecialChars[QuoteIndex]; }
		set { SpecialChars[QuoteIndex] = value; }
	}
}


public class CSVLogger : CsvFileCommon, IDisposable
{
	private StreamWriter Writer;
	private string OneQuote = null;
	private string TwoQuotes = null;
	private string QuotedFormat = null;

	private static int currentTask = 1;
	private string user = "nutzer1";
    private bool firstRow = true;
	
	public CSVLogger(string path)
	{
		Writer = new StreamWriter(path);
        WriteRow(new List<string>(new string[] {}));
	}
	
	public void WriteRow(List<string> columns)
	{
		List<string> row = GetStandardLogElement();
		foreach(string column in columns)
		{
			row.Add(column);
		}

		if (row == null)
			throw new ArgumentNullException("columns");
		
		if (OneQuote == null || OneQuote[0] != Quote)
		{
			OneQuote = String.Format("{0}", Quote);
			TwoQuotes = String.Format("{0}{0}", Quote);
			QuotedFormat = String.Format("{0}{{0}}{0}", Quote);
		}
		
		for (int i = 0; i < row.Count; i++)
		{
			if (i > 0)
				Writer.Write(Delimiter);
			if (row[i].IndexOfAny(SpecialChars) == -1)
				Writer.Write(row[i]);
			else
				Writer.Write(QuotedFormat, row[i].Replace(OneQuote, TwoQuotes));
		}
		Writer.WriteLine();
	}
	
	public void Dispose()
	{
		Writer.Dispose();
        Debug.Log("CSV File ended");
	}

	public static void StartTask()
	{
		Debug.Log ("Stated task" + currentTask);
	}

	public static void EndTask()
	{
		Debug.Log ("Ended task" + currentTask);
		currentTask++;
	}

	private List<string> GetStandardLogElement()
	{
        if(firstRow)
        {
            firstRow = false;
            return new List<string>(new string[] { "Task", "Username","TimeStamp", "ActionType", "ObjectName", "Stacksize", "DropType", "OnPage"});           
        }
        string task = "Task" + currentTask;
		Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		string timeStamp = unixTimestamp.ToString ();
		return new List<string>(new string[] { task, user, timeStamp});
	}

    public void SetUserName(string user)
    {
        this.user = user;
    }
}
