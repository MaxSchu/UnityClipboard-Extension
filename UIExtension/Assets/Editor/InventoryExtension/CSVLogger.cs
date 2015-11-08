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
	/// <summary>
	/// Initializes a new instance of the CsvFileWriter class for the
	/// specified stream.
	/// </summary>
	/// <param name="stream">The stream to write to</param>
	public CSVLogger(Stream stream)
	{
		Writer = new StreamWriter(stream);
	}
	
	/// <summary>
	/// Initializes a new instance of the CsvFileWriter class for the
	/// specified file path.
	/// </summary>
	/// <param name="path">The name of the CSV file to write to</param>
	public CSVLogger(string path)
	{
		Writer = new StreamWriter(path);
        WriteRow(new List<string>(new string[] {}));
	}
	
	/// <summary>
	/// Writes a row of columns to the current CSV file.
	/// </summary>
	/// <param name="columns">The list of columns to write</param>
	public void WriteRow(List<string> columns)
	{
		List<string> row = GetStandardLogElement();
		foreach(string column in columns)
		{
			row.Add(column);
		}

		// Verify required argument
		if (row == null)
			throw new ArgumentNullException("columns");
		
		// Ensure we're using current quote character
		if (OneQuote == null || OneQuote[0] != Quote)
		{
			OneQuote = String.Format("{0}", Quote);
			TwoQuotes = String.Format("{0}{0}", Quote);
			QuotedFormat = String.Format("{0}{{0}}{0}", Quote);
		}
		
		// Write each column
		for (int i = 0; i < row.Count; i++)
		{
			// Add delimiter if this isn't the first column
			if (i > 0)
				Writer.Write(Delimiter);
			// Write this column
			if (row[i].IndexOfAny(SpecialChars) == -1)
				Writer.Write(row[i]);
			else
				Writer.Write(QuotedFormat, row[i].Replace(OneQuote, TwoQuotes));
		}
		Writer.WriteLine();
	}
	
	// Propagate Dispose to StreamWriter
	public void Dispose()
	{
		Writer.Dispose();
        Debug.Log("CSV File ended");
	}

	public static void NextTask() 
	{
		currentTask++;
        Debug.Log("Switched to task" + currentTask);
	}

	private List<string> GetStandardLogElement()
	{
        if(firstRow)
        {
            firstRow = false;
            return new List<string>(new string[] { "Task", "Username","TimeStamp", "ActionType", "ObjectName", "Stacksize", "DropType", "OnPage"});           
        }
        string task = "Task" + currentTask;
		string timeStamp = DateTime.Now.ToString();
		return new List<string>(new string[] { task, user, timeStamp});
	}

    public void SetUserName(string user)
    {
        this.user = user;
    }
}
