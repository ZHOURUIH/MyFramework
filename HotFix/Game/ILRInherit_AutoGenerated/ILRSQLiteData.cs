using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
	
public class ILRSQLiteData : SQLiteData
{	
	public override void parse(SqliteDataReader reader) { base.parse(reader); }
	public override void insert(ref string valueString) { base.insert(ref valueString); }
	public override void insert(MyStringBuilder valueString) { base.insert(valueString); }
	public override bool checkData() { return base.checkData(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
