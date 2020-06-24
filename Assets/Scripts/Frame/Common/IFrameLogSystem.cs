using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IFrameLogSystem
{
	void logProcedure(string info);
	void logHttpOverTime(string info);
	void logGameError(string info);
}